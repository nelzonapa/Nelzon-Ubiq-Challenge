using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    public string ID { get; private set; }
    public int Group { get; private set; }
    public List<string> Entities { get; private set; }
    public Vector3 Position => transform.position;

    private Vector3 velocity;
    private Renderer rend;
    private TMPro.TextMeshPro label;

    public void Initialize(string id, int group, List<string> entities, int numCommunities)
    {
        ID = id;
        Group = group;
        Entities = entities;

        // Obtener referencias a componentes existentes
        rend = GetComponent<Renderer>();
        label = GetComponentInChildren<TMPro.TextMeshPro>();

        // Configurar con componentes existentes
        if (label != null) label.text = id;
        if (rend != null) rend.material.color = Color.HSVToRGB((float)group / numCommunities, 0.7f, 0.8f);
    }

    public void AddForce(Vector3 force) => velocity += force;
    public void UpdateVelocity(float dampingFactor) => velocity *= dampingFactor;
    public void UpdatePosition(float deltaTime) => transform.position += velocity * deltaTime;
}