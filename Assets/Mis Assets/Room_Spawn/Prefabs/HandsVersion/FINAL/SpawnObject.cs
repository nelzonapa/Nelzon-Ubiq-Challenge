using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    [Header("Prefab a Spawnear")]
    public GameObject objectToSpawn;

    [Header("Configuración de Spawn")]
    public float distanceFromPlayer = 1.0f;   // Distancia frente al jugador

    // Referencia a la cámara principal
    private Camera playerCamera;

    void Start()
    {
        // Obtener la cámara principal al inicio
        playerCamera = Camera.main;
    }

    // Función pública que puede ser llamada por eventos (como un botón)
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

        // Calcular la posición frente a la cámara
        Vector3 spawnPosition = playerCamera.transform.position +
                               playerCamera.transform.forward * distanceFromPlayer;

        // Calcular la rotación para que mire hacia la cámara y luego aplicar 180 grados
        Vector3 directionToCamera = playerCamera.transform.position - spawnPosition;
        Quaternion spawnRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);

        // Aplicar rotación adicional de 180 grados en el eje Y
        spawnRotation *= Quaternion.Euler(0, 180f, 0);

        // Instanciar el objeto
        GameObject spawnedObject = Instantiate(objectToSpawn, spawnPosition, spawnRotation);

        Debug.Log($"Objeto spawneado: {spawnedObject.name} en posición: {spawnPosition}");
    }
}