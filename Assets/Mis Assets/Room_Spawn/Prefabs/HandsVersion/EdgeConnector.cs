// Componente para actualizar la línea al mover nodos
using UnityEngine;

public class EdgeConnector : MonoBehaviour
{
    private LineRenderer lr;
    private Transform nodoA;
    private Transform nodoB;
    private float matchEpsilon = 0.01f;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        Vector3 p0 = lr.GetPosition(0);
        Vector3 p1 = lr.GetPosition(1);
        var parent = transform.parent;
        foreach (Transform hijo in parent)
        {
            if (!hijo.name.StartsWith("Nodo_")) continue;
            if (Vector3.Distance(hijo.position, p0) < matchEpsilon)
                nodoA = hijo;
            if (Vector3.Distance(hijo.position, p1) < matchEpsilon)
                nodoB = hijo;
        }
        if (nodoA == null || nodoB == null)
            Debug.LogWarning($"[EdgeConnector] Faltan nodos para {gameObject.name}");
    }

    void Update()
    {
        if (nodoA != null && nodoB != null)
        {
            lr.SetPosition(0, nodoA.position);
            lr.SetPosition(1, nodoB.position);
        }
    }
}
