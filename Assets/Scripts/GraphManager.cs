using System.Collections.Generic;
//using Newtonsoft.Json;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    [Header("JSON Settings")]
    [Tooltip("Arrastra aquí tu archivo graph_for_unity.json (importado como TextAsset)")]
    public TextAsset graphJsonAsset;

    [Header("Prefabs")]
    public GameObject nodePrefab;
    public GameObject edgePrefab;

    [Header("Layout Parameters")]
    public float repulsion = 1f;
    public float attraction = 0.1f;

    [Header("UI")]
    public GameObject detailsPanelPrefab; // Arrástralo desde el Inspector

    private GraphData graph;
    private Dictionary<string, NodeBehaviour> nodeDict;
    private int numCommunities;

    void Start()
    {
        // 1) Validaciones básicas
        if (graphJsonAsset == null)
        {
            Debug.LogError("❌ GraphManager: no has asignado graphJsonAsset (TextAsset).");
            enabled = false;
            return;
        }
        if (nodePrefab == null)
        {
            Debug.LogError("❌ GraphManager: no has asignado nodePrefab.");
            enabled = false;
            return;
        }
        if (edgePrefab == null)
        {
            Debug.LogError("❌ GraphManager: no has asignado edgePrefab.");
            enabled = false;
            return;
        }

        LoadJson();
        InstantiateGraph();
    }

    void LoadJson()
    {
        //graph = JsonConvert.DeserializeObject<GraphData>(graphJsonAsset.text);
        //numCommunities = GetMaxGroup() + 1;
        //Debug.Log($"✅ JSON cargado: {graph.nodes.Count} nodos, {graph.links.Count} enlaces, {numCommunities} comunidades.");
    }

    int GetMaxGroup()
    {
        int max = 0;
        foreach (var n in graph.nodes)
            if (n.group > max) max = n.group;
        return max;
    }

    void InstantiateGraph()
    {
        nodeDict = new Dictionary<string, NodeBehaviour>();

        // 1) Crear nodos
        foreach (var nd in graph.nodes)
        {
            var go = Instantiate(nodePrefab, Random.insideUnitSphere * 0.5f, Quaternion.identity, transform);
            var nb = go.GetComponent<NodeBehaviour>();
            if (nb == null)
            {
                Debug.LogError($"❌ El prefab '{nodePrefab.name}' no tiene NodeBehaviour.");
                continue;
            }

            nb.id = nd.id;
            nb.group = nd.group;
            nb.entities = nd.entities;

            var rendComp = go.GetComponent<Renderer>();
            if (rendComp != null)
            {
                var col = Color.HSVToRGB((float)nd.group / (float)numCommunities, 0.7f, 0.8f);
                rendComp.material.color = col;
            }

            nodeDict[nd.id] = nb;
        }

        // 2) Crear aristas
        foreach (var ld in graph.links)
        {
            var go = Instantiate(edgePrefab, transform);
            var eb = go.GetComponent<EdgeBehaviour>();
            if (eb == null)
            {
                Debug.LogError($"❌ El prefab '{edgePrefab.name}' no tiene EdgeBehaviour.");
                continue;
            }
            if (nodeDict.TryGetValue(ld.source, out var a) && nodeDict.TryGetValue(ld.target, out var b))
            {
                eb.nodeA = a;
                eb.nodeB = b;
                eb.width = 0.01f * ld.weight;
            }
            else
            {
                Debug.LogWarning($"⚠ Nodo fuente o destino no encontrado para enlace {ld.source}→{ld.target}");
                Destroy(go);
            }
        }

        Debug.Log($"🕸️ Grafo instanciado: {nodeDict.Count} nodos.");

        // Instancia el panel de detalles
        //GameObject detailsPanelPrefab = Resources.Load<GameObject>("DetailsPanel");
        GameObject panelInstance = Instantiate(detailsPanelPrefab, Vector3.zero, Quaternion.identity);
        panelInstance.SetActive(false);

        // Asigna a todos los nodos
        foreach (var node in nodeDict.Values)
        {
            node.detailsPanel = panelInstance.GetComponent<DetailsPanel>();
        }
    }

    void Update()
    {
        // Fuerza repulsiva entre todos los nodos
        foreach (var a in nodeDict.Values)
        {
            Vector3 force = Vector3.zero;
            foreach (var b in nodeDict.Values)
            {
                if (a == b) continue;
                var dir = a.transform.position - b.transform.position;
                float dist = dir.magnitude + 0.01f;
                force += dir.normalized * (repulsion / (dist * dist));
            }
            a.transform.position += force * Time.deltaTime;
        }

        // Fuerza atractiva de las aristas
        foreach (var eb in FindObjectsOfType<EdgeBehaviour>())
        {
            var aPos = eb.nodeA.transform.position;
            var bPos = eb.nodeB.transform.position;
            var dir = bPos - aPos;
            eb.nodeA.transform.position += dir * attraction * Time.deltaTime;
            eb.nodeB.transform.position -= dir * attraction * Time.deltaTime;
        }
    }
}