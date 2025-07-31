using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class ClusterVisualizer : MonoBehaviour
{
    [Header("Referencias")]
    public GeneradorNodos NodoSpawner;       // Referencia al script que generó los nodos
    //debe ejecutarse antes
    public TextAsset graphJson;               // JSON enriquecido con top_keywords y comunidad
    public Material BubbleMaterial;           // Material semitransparente para hull
    public TMP_FontAsset FontAsset;           // Fuente para TextMeshPro

    [Header("Configuración de etiquetas")]
    public float LabelFontSize = 1.2f;
    public Color LabelColor = Color.white;

    private Dictionary<int, List<Transform>> clusters;
    private Dictionary<int, GameObject> bubbleObjects;
    private Dictionary<int, TextMeshPro> labelObjects;
    private Dictionary<int, NodoJson> jsonNodeMap;

    private bool initialized = false;


    [System.Serializable]
    private class NodoJson { 
        public int id; 
        public List<string> top_keywords; 
        public int community; 
        public string community_label; 
    }
    [System.Serializable]
    private class GrafoJson { public List<NodoJson> nodes; }

    IEnumerator Start()
    {
        yield return null; // Esperar un frame

        if (NodoSpawner == null || graphJson == null || BubbleMaterial == null || FontAsset == null)
        {
            Debug.LogError("[ClusterVisualizer] Faltan referencias en Inspector."); yield break;
        }
        if (NodoSpawner.mapaNodos == null || NodoSpawner.mapaNodos.Count == 0)
        {
            Debug.LogError("[ClusterVisualizer] mapaNodos vacío. Verifica GeneradorNodos."); yield break;
        }

        // Parsear JSON
        jsonNodeMap = new Dictionary<int, NodoJson>();
        GrafoJson grafoJson = JsonUtility.FromJson<GrafoJson>(graphJson.text);
        if (grafoJson == null || grafoJson.nodes == null)
        {
            Debug.LogError("[ClusterVisualizer] JSON malformado."); 
            yield break;
        }
        foreach (var nj in grafoJson.nodes) 
            jsonNodeMap[nj.id] = nj;

        clusters = new Dictionary<int, List<Transform>>();
        bubbleObjects = new Dictionary<int, GameObject>();
        labelObjects = new Dictionary<int, TextMeshPro>();

        AgruparNodosPorComunidad();
        CrearClusterVisuals();

        initialized = true;
    }

    void AgruparNodosPorComunidad()
    {
        foreach (var kvp in NodoSpawner.mapaNodos)
        {
            int id = kvp.Key;
            if (!jsonNodeMap.ContainsKey(id)) continue;
            int comm = jsonNodeMap[id].community;
            if (!clusters.ContainsKey(comm)) clusters[comm] = new List<Transform>();
            clusters[comm].Add(kvp.Value.transform);
        }
    }

    void CrearClusterVisuals()
    {
        foreach (var kvp in clusters)
        {
            int comm = kvp.Key;
            List<Transform> nodes = kvp.Value;
            if (nodes.Count == 0) continue;

            // 1) Convex hull
            Vector3[] points = new Vector3[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
                points[i] = nodes[i].position;
            Mesh hullMesh = ConvexHullGenerator.GenerateHull(points);

            // 2) Bubble
            GameObject bubble = new GameObject($"Bubble_Comm_{comm}");
            bubble.transform.parent = transform;
            var mf = bubble.AddComponent<MeshFilter>(); mf.mesh = hullMesh;
            var mr = bubble.AddComponent<MeshRenderer>(); mr.material = BubbleMaterial;
            bubbleObjects[comm] = bubble;

            // 3) Etiqueta semántica de comunidad
            string labelText = jsonNodeMap.ContainsKey(nodes[0].GetInstanceID()/*placeholder*/) ?
                jsonNodeMap[nodes[0].GetInstanceID()].community_label : string.Empty;
            // Mejor referir por id extraído del nombre:
            int nid = GetNodeIdFromName(nodes[0].name);
            if (jsonNodeMap.ContainsKey(nid)) labelText = jsonNodeMap[nid].community_label;

            GameObject labelGO = new GameObject($"Label_Comm_{comm}");
            labelGO.transform.parent = transform;
            var tmp = labelGO.AddComponent<TextMeshPro>();
            tmp.font = FontAsset;
            tmp.fontSize = LabelFontSize;
            tmp.color = LabelColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.text = labelText;
            labelObjects[comm] = tmp;
        }
    }

    void Update()
    {
        if (!initialized) return;
        foreach (var kvp in clusters)
        {
            int comm = kvp.Key;
            var nodes = kvp.Value;
            if (nodes.Count == 0) continue;
            if (!bubbleObjects.ContainsKey(comm) || !labelObjects.ContainsKey(comm)) continue;

            Vector3 centroid = Vector3.zero;
            Vector3[] pts = new Vector3[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                pts[i] = nodes[i].position;
                centroid += pts[i];
            }
            centroid /= nodes.Count;

            Mesh hull = ConvexHullGenerator.GenerateHull(pts);
            var mf = bubbleObjects[comm].GetComponent<MeshFilter>();
            if (mf != null) mf.mesh = hull;

            var lbl = labelObjects[comm];
            if (lbl != null) lbl.transform.position = centroid + Vector3.up * 0.5f;
        }
    }

    int GetNodeIdFromName(string name)
    {
        var parts = name.Split('_');
        if (parts.Length < 2) return -1;
        if (int.TryParse(parts[1], out int id)) return id;
        return -1;
    }
}

public static class ConvexHullGenerator
{
    public static Mesh GenerateHull(Vector3[] points)
    {
        // Placeholder simple: retorna un plano mínimo si solo 1-4 puntos
        Mesh mesh = new Mesh();
        if (points.Length < 3)
        {
            mesh.vertices = points;
            mesh.triangles = new int[0];
            return mesh;
        }
        // Implementa QuickHull aquí
        mesh.vertices = new Vector3[] {
            new Vector3(-0.5f,0,-0.5f), new Vector3(0.5f,0,-0.5f),
            new Vector3(0.5f,0,0.5f), new Vector3(-0.5f,0,0.5f)
        };
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
        return mesh;
    }
}
