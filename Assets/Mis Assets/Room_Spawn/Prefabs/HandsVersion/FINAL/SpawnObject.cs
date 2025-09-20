using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    [Header("Prefab a Spawnear")]
    public GameObject objectToSpawn;

    [Header("Configuraci�n de Spawn")]
    public float distanceFromPlayer = 1.0f;   // Distancia frente al jugador

    // Referencia a la c�mara principal
    private Camera playerCamera;

    void Start()
    {
        // Obtener la c�mara principal al inicio
        playerCamera = Camera.main;
    }

    // Funci�n p�blica que puede ser llamada por eventos (como un bot�n)
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

        // Calcular la posici�n frente a la c�mara
        Vector3 spawnPosition = playerCamera.transform.position +
                               playerCamera.transform.forward * distanceFromPlayer;

        // Calcular la rotaci�n para que mire hacia la c�mara y luego aplicar 180 grados
        Vector3 directionToCamera = playerCamera.transform.position - spawnPosition;
        Quaternion spawnRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);

        // Aplicar rotaci�n adicional de 180 grados en el eje Y
        spawnRotation *= Quaternion.Euler(0, 180f, 0);

        // Instanciar el objeto
        GameObject spawnedObject = Instantiate(objectToSpawn, spawnPosition, spawnRotation);

        Debug.Log($"Objeto spawneado: {spawnedObject.name} en posici�n: {spawnPosition}");
    }
}