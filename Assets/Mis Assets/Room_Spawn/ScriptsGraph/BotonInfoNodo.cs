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

    //Para spawneo:

    [Tooltip("Punto desde donde se generará el prefab")]
    public Transform spawnPoint;
    private GameObject spawnedInstance;

    [Tooltip("Lista de prefabs a instanciar.")]
    public GameObject[] prefabsToSpawn;

    // Lista para almacenar instancias creadas
    private readonly System.Collections.Generic.List<GameObject> spawnedInstances =
        new System.Collections.Generic.List<GameObject>();



    //FUNCIONES
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
        {
            meshRenderer.material.color = clickedColor;
        }

        // Crear todos los prefabs de la lista (si aún no lo hiciste)
        if (spawnedInstances.Count == 0 && prefabsToSpawn != null && spawnPoint != null)
        {
            foreach (var prefab in prefabsToSpawn)
            {
                if (prefab != null)
                {
                    GameObject inst = Instantiate(
                        prefab,
                        spawnPoint.position,
                        spawnPoint.rotation,
                        transform
                    );
                    spawnedInstances.Add(inst);
                }
            }
        }
    }

    public void OnSelectExited(SelectExitEventArgs args)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = originalColor;
        }
    }
}
