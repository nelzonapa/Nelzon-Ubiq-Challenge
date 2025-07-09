using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BotonInfoNodo : MonoBehaviour
{
    public int idNodo;
    public string nombreBlogDocumento;
    public string fragmentoResumen;
    public string[] palabrasClave;
    public string[] nombresPropios;
    public string[] entidades;
    public string[] concordancias;
    public int colorGrupo;

    private MeshRenderer meshRenderer;
    private Color originalColor;
    public Color clickedColor = Color.red;

    [Tooltip("Punto desde donde se generará la rejilla de prefabs")]
    public Transform spawnPoint;

    [Tooltip("Lista de prefabs a instanciar.")]
    public GameObject[] prefabsToSpawn;

    [Header("Configuración de la rejilla")]
    [Tooltip("Número de columnas en la rejilla (las filas se calculan automáticamente).")]
    public int gridColumns = 3;
    [Tooltip("Separación horizontal entre elementos.")]
    public float xSpacing = 0.5f;
    [Tooltip("Separación vertical entre elementos.")]
    public float ySpacing = 0.3f;

    private readonly List<GameObject> spawnedInstances = new List<GameObject>();

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            originalColor = meshRenderer.material.color;
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        Debug.Log("¡Botón presionado!");
        if (meshRenderer != null)
            meshRenderer.material.color = clickedColor;

        // Si ya creamos la rejilla, no lo hacemos de nuevo
        if (spawnedInstances.Count > 0 || prefabsToSpawn == null || spawnPoint == null)
            return;

        int total = prefabsToSpawn.Length;
        // Si gridColumns es 0 o 1, forzamos 1 para evitar división por cero
        int cols = Mathf.Max(1, gridColumns);
        int rows = Mathf.CeilToInt((float)total / cols);

        for (int i = 0; i < total; i++)
        {
            GameObject prefab = prefabsToSpawn[i];
            if (prefab == null) continue;

            // Calcula fila y columna
            int row = i / cols;
            int col = i % cols;

            // Offset en X y Y, manteniendo Z = 0 (mismo plano)
            Vector3 localOffset = new Vector3(
                (col - (cols - 1) * 0.5f) * xSpacing,
                -row * ySpacing,
                0f
            );

            // Posición final en el mundo
            Vector3 spawnPos = spawnPoint.position
                             + spawnPoint.TransformVector(localOffset);

            // Rotación para que queden mirando igual que spawnPoint
            Quaternion spawnRot = spawnPoint.rotation;

            // Instancia y guarda referencia
            GameObject inst = Instantiate(prefab, spawnPos, spawnRot, transform);
            spawnedInstances.Add(inst);
        }
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        if (meshRenderer != null)
            meshRenderer.material.color = originalColor;
    }
}
