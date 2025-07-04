using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EdgeBehaviour : MonoBehaviour
{
    public NodeBehaviour nodeA;
    public NodeBehaviour nodeB;
    public float width = 0.02f;

    private LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.widthMultiplier = width;
    }

    void Update()
    {
        if (nodeA != null && nodeB != null)
        {
            lr.SetPosition(0, nodeA.transform.position);
            lr.SetPosition(1, nodeB.transform.position);
        }
    }
}
