using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Rooms;
using System.Collections.Generic;

public class SpawnObject : MonoBehaviour
{
    [Header("Configuraci�n de Ubiq")]
    public NetworkSpawnManager spawnManager;
    public PrefabCatalogue catalogue;

    [Header("Prefab a Spawnear")]
    public GameObject objectToSpawn;

    [Header("Configuraci�n de Spawn")]
    public float distanceFromPlayer = 1.0f;

    //punto de spawneo, ubicaci�n del usuario y sumado los ajustes para qeu siempre aparezca frente al usuario

    private Camera playerCamera;
    private int prefabIndex = -1;

    void Awake()
    {
        // Intentar encontrar el NetworkSpawnManager si no est� asignado
        if (spawnManager == null)
        {
            spawnManager = NetworkSpawnManager.Find(this);
        }

        // Intentar encontrar el cat�logo si no est� asignado
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

        // Encontrar el �ndice del prefab en el cat�logo
        FindPrefabIndex();
    }

    void OnValidate()
    {
        // Actualizar el �ndice cuando se cambie el prefab en el inspector
        FindPrefabIndex();
    }

    // M�todo para encontrar el �ndice del prefab en el cat�logo
    private void FindPrefabIndex()
    {
        prefabIndex = -1;

        if (catalogue == null || objectToSpawn == null)
        {
            return;
        }

        // Buscar el prefab en el cat�logo
        for (int i = 0; i < catalogue.prefabs.Count; i++)
        {
            if (catalogue.prefabs[i] == objectToSpawn)
            {
                prefabIndex = i;
                Debug.Log($"Prefab encontrado en el cat�logo en la posici�n: {prefabIndex}");
                return;
            }
        }

        Debug.LogWarning($"Prefab {objectToSpawn.name} no encontrado en el cat�logo");
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
            Debug.LogError("No se encontr� la c�mara principal!");
            return;
        }

        if (spawnManager == null)
        {
            Debug.LogError("No se encontr� NetworkSpawnManager!");
            return;
        }

        if (prefabIndex == -1)
        {
            Debug.LogError("El prefab no est� en el cat�logo de Ubiq. No se puede spawnear.");
            return;
        }

        // Spawnear el objeto usando el �ndice del cat�logo
        SpawnWithRoomScopeByIndex(prefabIndex);
        Debug.Log("Solicitado spawn de objeto en red.");
    }

    // M�todo para spawnear por �ndice usando reflection
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
                // Obtener el m�todo SpawnWithRoomScope que acepta un �ndice
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
        Debug.LogWarning("No se pudo acceder al m�todo interno. Intentando spawn directo.");
        spawnManager.SpawnWithRoomScope(objectToSpawn);
    }

    // Manejador para el evento OnSpawned
    private void HandleSpawnedObject(GameObject spawnedObject, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
    {
        // Verificar si es el objeto que queremos configurar
        if (spawnedObject.name.StartsWith(objectToSpawn.name))
        {
            // Calcular posici�n y rotaci�n frente al jugador
            Vector3 spawnPosition = playerCamera.transform.position +
                                   playerCamera.transform.forward * distanceFromPlayer;

            Vector3 directionToCamera = playerCamera.transform.position - spawnPosition;
            Quaternion spawnRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            spawnRotation *= Quaternion.Euler(0, 360f, 0);

            // Configurar la posici�n y rotaci�n
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

    // M�todo para obtener todos los prefabs en el cat�logo (�til para debugging)
    public List<GameObject> GetAllPrefabsInCatalogue()
    {
        if (catalogue != null)
        {
            return catalogue.prefabs;
        }
        return new List<GameObject>();
    }
}