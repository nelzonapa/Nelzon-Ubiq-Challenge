using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Rooms;

public class SpawnObject : MonoBehaviour
{
    [Header("Prefab a Spawnear")]
    public GameObject objectToSpawn;

    [Header("Configuración de Spawn")]
    public float distanceFromPlayer = 1.0f;

    private Camera playerCamera;
    private NetworkSpawnManager spawnManager;

    void Awake()
    {
        spawnManager = NetworkSpawnManager.Find(this);
        playerCamera = Camera.main;

        // Suscribirse al evento de spawn con la firma correcta
        spawnManager.OnSpawned.AddListener(HandleSpawnedObject);
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

        // Spawnear el objeto
        spawnManager.SpawnWithRoomScope(objectToSpawn);
        Debug.Log("Solicitado spawn de objeto en red.");
    }

    // Manejador con la firma correcta para el evento OnSpawned
    private void HandleSpawnedObject(GameObject spawnedObject, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
    {
        // Verificar si es el tipo de objeto que queremos configurar
        if (spawnedObject.name.StartsWith(objectToSpawn.name))
        {
            // Calcular posición y rotación
            Vector3 spawnPosition = playerCamera.transform.position +
                                   playerCamera.transform.forward * distanceFromPlayer;

            Vector3 directionToCamera = playerCamera.transform.position - spawnPosition;
            Quaternion spawnRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
            spawnRotation *= Quaternion.Euler(0, 180f, 0);

            // Configurar la posición y rotación del objeto spawnedo
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
}