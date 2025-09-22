using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Rooms;
using System.Collections.Generic;

public class SpawnObject : MonoBehaviour
{
    [Header("Configuración de Ubiq")]
    public NetworkSpawnManager spawnManager;
    public PrefabCatalogue catalogue;

    [Header("Prefab a Spawnear")]
    public GameObject objectToSpawn;

    [Header("Configuración de Spawn")]
    public float distanceFromPlayer = 1.0f;

    //punto de spawneo, ubicación del usuario y sumado los ajustes para qeu siempre aparezca frente al usuario

    private Camera playerCamera;
    private int prefabIndex = -1;

    void Awake()
    {
        // Intentar encontrar el NetworkSpawnManager si no está asignado
        if (spawnManager == null)
        {
            spawnManager = NetworkSpawnManager.Find(this);
        }

        // Intentar encontrar el catálogo si no está asignado
        if (catalogue == null && spawnManager != null)
        {
            catalogue = spawnManager.catalogue;
        }

        playerCamera = Camera.main;

        // Suscribirse al evento de spawn
        if (spawnManager != null)
        {
            spawnManager.OnSpawned.AddListener(HandleSpawnedObject);
        }

        // Encontrar el índice del prefab en el catálogo
        FindPrefabIndex();
    }

    void OnValidate()
    {
        // Actualizar el índice cuando se cambie el prefab en el inspector
        FindPrefabIndex();
    }

    // Método para encontrar el índice del prefab en el catálogo
    private void FindPrefabIndex()
    {
        prefabIndex = -1;

        if (catalogue == null || objectToSpawn == null)
        {
            return;
        }

        // Buscar el prefab en el catálogo
        for (int i = 0; i < catalogue.prefabs.Count; i++)
        {
            if (catalogue.prefabs[i] == objectToSpawn)
            {
                prefabIndex = i;
                Debug.Log($"Prefab encontrado en el catálogo en la posición: {prefabIndex}");
                return;
            }
        }

        Debug.LogWarning($"Prefab {objectToSpawn.name} no encontrado en el catálogo");
    }

    public void SpawnPrefab()
    {
        if (objectToSpawn == null)
        {
            Debug.LogError("No hay prefab asignado para spawnear!");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogError("No se encontró la cámara principal!");
            return;
        }

        if (spawnManager == null)
        {
            Debug.LogError("No se encontró NetworkSpawnManager!");
            return;
        }

        if (prefabIndex == -1)
        {
            Debug.LogError("El prefab no está en el catálogo de Ubiq. No se puede spawnear.");
            return;
        }

        // Spawnear el objeto usando el índice del catálogo
        SpawnWithRoomScopeByIndex(prefabIndex);
        Debug.Log("Solicitado spawn de objeto en red.");
    }

    // Método para spawnear por índice usando reflection
    private void SpawnWithRoomScopeByIndex(int index)
    {
        // Obtener el campo privado 'spawner' de NetworkSpawnManager mediante reflection
        var spawnerField = typeof(NetworkSpawnManager).GetField("spawner",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (spawnerField != null)
        {
            var internalSpawner = spawnerField.GetValue(spawnManager) as NetworkSpawner;
            if (internalSpawner != null)
            {
                // Obtener el método SpawnWithRoomScope que acepta un índice
                var method = typeof(NetworkSpawner).GetMethod("SpawnWithRoomScope",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null,
                    new System.Type[] { typeof(int) },
                    null);

                if (method != null)
                {
                    method.Invoke(internalSpawner, new object[] { index });
                    return;
                }
            }
        }

        // Fallback: intentar spawnear directamente (puede no funcionar para room scope)
        Debug.LogWarning("No se pudo acceder al método interno. Intentando spawn directo.");
        spawnManager.SpawnWithRoomScope(objectToSpawn);
    }

    // Manejador para el evento OnSpawned
    private void HandleSpawnedObject(GameObject spawnedObject, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
    {
        // Verificar si es el objeto que queremos configurar
        if (spawnedObject.name.StartsWith(objectToSpawn.name))
        {
            // Calcular posición y rotación frente al jugador
            Vector3 spawnPosition = playerCamera.transform.position +
                                   playerCamera.transform.forward * distanceFromPlayer;

            Vector3 directionToCamera = playerCamera.transform.position - spawnPosition;
            Quaternion spawnRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            spawnRotation *= Quaternion.Euler(0, 360f, 0);

            // Configurar la posición y rotación
            spawnedObject.transform.position = spawnPosition;
            spawnedObject.transform.rotation = spawnRotation;

            Debug.Log($"Objeto spawneado y configurado: {spawnedObject.name}");
        }
    }

    void OnDestroy()
    {
        // Limpiar el listener cuando el objeto sea destruido
        if (spawnManager != null)
        {
            spawnManager.OnSpawned.RemoveListener(HandleSpawnedObject);
        }
    }

    // Método para obtener todos los prefabs en el catálogo (útil para debugging)
    public List<GameObject> GetAllPrefabsInCatalogue()
    {
        if (catalogue != null)
        {
            return catalogue.prefabs;
        }
        return new List<GameObject>();
    }
}