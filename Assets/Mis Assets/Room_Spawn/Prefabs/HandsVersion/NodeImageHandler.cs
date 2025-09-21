using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Spawning;
using Ubiq.Samples;
using Ubiq.Rooms;
using System.Collections;
using System.Reflection;

[RequireComponent(typeof(XRGrabInteractable))]
public class NodeImageHandler : MonoBehaviour
{
    [Header("Configuraci�n de Ubiq")]
    public NetworkSpawnManager spawnManager;
    public PrefabCatalogue catalogue;

    [Header("UI Prefab (Panel con Content y CloseButton)")]
    public GameObject imagePanelPrefab;

    [Header("Ajuste de posici�n del panel")]
    public Vector3 panelLocalOffset = new Vector3(2.0f, 0f, 0f);
    public float panelScale = 0.002f;

    private DataDeNodo data;
    private XRGrabInteractable grabInteractable;
    private int prefabIndex = -1;

    void Awake()
    {
        if (spawnManager == null)
        {
            spawnManager = NetworkSpawnManager.Find(this);
        }

        if (catalogue == null && spawnManager != null)
        {
            catalogue = spawnManager.catalogue;
        }

        data = GetComponent<DataDeNodo>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
        else
        {
            Debug.LogWarning("NodeImageHandler requiere un XRGrabInteractable.");
        }

        FindPrefabIndex();

        if (spawnManager != null)
        {
            spawnManager.OnSpawned.AddListener(HandleSpawnedObject);
        }
    }

    void OnValidate()
    {
        FindPrefabIndex();
    }

    private void FindPrefabIndex()
    {
        prefabIndex = -1;

        if (catalogue == null || imagePanelPrefab == null)
        {
            return;
        }

        for (int i = 0; i < catalogue.prefabs.Count; i++)
        {
            if (catalogue.prefabs[i] == imagePanelPrefab)
            {
                prefabIndex = i;
                Debug.Log($"Prefab encontrado en el cat�logo en la posici�n: {prefabIndex}");
                return;
            }
        }

        Debug.LogWarning($"Prefab {imagePanelPrefab.name} no encontrado en el cat�logo");
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (imagePanelPrefab == null)
        {
            Debug.LogError("Asignar imagePanelPrefab en el Inspector.");
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

        SpawnWithRoomScopeByIndex(prefabIndex);
        Debug.Log("Solicitado spawn de panel de im�genes en red.");
    }

    private void SpawnWithRoomScopeByIndex(int index)
    {
        var spawnerField = typeof(NetworkSpawnManager).GetField("spawner",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (spawnerField != null)
        {
            var internalSpawner = spawnerField.GetValue(spawnManager) as NetworkSpawner;
            if (internalSpawner != null)
            {
                var method = typeof(NetworkSpawner).GetMethod("SpawnWithRoomScope",
                    BindingFlags.NonPublic | BindingFlags.Instance,
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

        Debug.LogWarning("No se pudo acceder al m�todo interno. Intentando spawn directo.");
        spawnManager.SpawnWithRoomScope(imagePanelPrefab);
    }

    private void HandleSpawnedObject(GameObject spawnedObject, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
    {
        if (spawnedObject.name.StartsWith(imagePanelPrefab.name))
        {
            var imageCanvas = spawnedObject.GetComponent<ImageCanvas>();
            if (imageCanvas != null)
            {
                // Preparar mensaje de configuraci�n
                ImageCanvas.ConfigurationMessage configMessage = new ImageCanvas.ConfigurationMessage
                {
                    parentNodeId = data.nodeId,
                    imageFiles = data.imageFiles,
                    localOffset = panelLocalOffset,
                    scaleFactor = panelScale
                };

                // Enviar configuraci�n
                StartCoroutine(SendConfigurationAfterDelay(imageCanvas, configMessage));
            }
        }
    }

    private IEnumerator SendConfigurationAfterDelay(ImageCanvas imageCanvas, ImageCanvas.ConfigurationMessage configMessage)
    {
        // Esperar un frame para asegurar que el ImageCanvas est� inicializado
        yield return null;

        // Enviar configuraci�n
        imageCanvas.SendConfiguration(configMessage);
        Debug.Log("Configuraci�n enviada para el panel del nodo: " + data.nodeId);
    }

    void OnDestroy()
    {
        if (spawnManager != null)
        {
            spawnManager.OnSpawned.RemoveListener(HandleSpawnedObject);
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }
}