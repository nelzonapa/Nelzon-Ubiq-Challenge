using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class BotonInfoNodo : MonoBehaviour
{
    //Para spawneo:

    [Tooltip("Lista de prefabs a instanciar.")]
    public GameObject[] prefabsToSpawn;

    [Tooltip("Punto desde donde se generarán los prefabs.")]
    public Transform spawnPoint;

    // Lista para almacenar instancias creadas
    private readonly System.Collections.Generic.List<GameObject> spawnedInstances =
        new System.Collections.Generic.List<GameObject>();

    //FUNCIONES
    public void OnSelectEntered()
    {
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

    // (Opcional) Método para limpiar los objetos instanciados
    public void ResetSpawned()
    {
        foreach (var inst in spawnedInstances)
        {
            if (inst != null) Destroy(inst);
        }
        spawnedInstances.Clear();
    }
}
