using UnityEngine;
using System.Collections.Generic;

public class GeneradorNodos : MonoBehaviour
{
    [System.Serializable]
    public class PosData
    {
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class DatosNodo
    {
        public int id;
        public string name;
        public PosData pos;
        public int community;
    }

    [System.Serializable]
    public class Arista
    {
        public int source;
        public int target;
        public float weight;
    }

    [System.Serializable]
    public class DatosGrafo
    {
        public List<DatosNodo> nodes;
        public List<Arista> edges;
    }

    [Header("Configuraci�n")]
    public TextAsset archivoJson;     // Asigna aqu� tu graph_enriched.json
    public GameObject prefabNodo;     // Prefab simple para cada nodo
    public Material materialArista;   // Material para las l�neas de arista
    public float grosorArista = 0.02f;// Grosor de las l�neas

    [Header("Ajustes de grafo")]
    public Vector3 graphOffset = Vector3.zero;  // Desplaza todo el grafo
    public float graphScale = 1f;               // Escala uniforme del grafo

    [Header("Colores por comunidad")]
    public Material[] materialesComunidad;

    // Para guardar referencias a cada nodo instanciado
    private Dictionary<int, GameObject> mapaNodos;

    void Start()
    {
        if (archivoJson == null || prefabNodo == null || materialArista == null || materialesComunidad == null || materialesComunidad.Length == 0)
        {
            Debug.LogError("[GeneradorNodos] Asigna JSON, prefabNodo, materialArista y materialesComunidad en el Inspector.");
            return;
        }

        // Parsear JSON
        DatosGrafo grafo = JsonUtility.FromJson<DatosGrafo>(archivoJson.text);
        if (grafo == null || grafo.nodes == null)
        {
            Debug.LogError("[GeneradorNodos] JSON malformado o vac�o.");
            return;
        }

        mapaNodos = new Dictionary<int, GameObject>();

        // 1) Instanciar nodos
        foreach (var nodo in grafo.nodes)
        {
            Vector3 rawPos = new Vector3(nodo.pos.x, nodo.pos.y, nodo.pos.z);
            Vector3 posicion = graphOffset + rawPos * graphScale;
            var obj = Instantiate(prefabNodo, posicion, Quaternion.identity, transform);
            obj.name = $"Nodo_{nodo.id}_{nodo.name}";
            mapaNodos[nodo.id] = obj;

            // Limpiar materiales existentes y asignar color por comunidad
            int comm = nodo.community;
            if (comm >= 0 && comm < materialesComunidad.Length)
            {
                var renders = obj.GetComponentsInChildren<Renderer>();
                foreach (var rend in renders)
                {
                    // Eliminar materiales existentes
                    rend.sharedMaterials = new Material[] { };
                    // Asignar nuevo material instanciado para la comunidad
                    rend.material = Instantiate(materialesComunidad[comm]);
                }
            }
        }

        // 2) Dibujar aristas
        foreach (var ar in grafo.edges)
        {
            if (!mapaNodos.ContainsKey(ar.source) || !mapaNodos.ContainsKey(ar.target))
                continue;

            var goSrc = mapaNodos[ar.source];
            var goDst = mapaNodos[ar.target];

            // Crear objeto vac�o para la l�nea
            var edgeGO = new GameObject($"Arista_{ar.source}_{ar.target}");
            edgeGO.transform.parent = transform;

            var lr = edgeGO.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.material = materialArista;
            lr.startWidth = grosorArista;
            lr.endWidth = grosorArista;
            lr.useWorldSpace = true;

            // Conectar los dos nodos en sus posiciones actuales
            lr.SetPosition(0, goSrc.transform.position);
            lr.SetPosition(1, goDst.transform.position);

            // A�adir componente para seguir nodos din�micamente
            edgeGO.AddComponent<EdgeConnector>();
        }
    }
}
