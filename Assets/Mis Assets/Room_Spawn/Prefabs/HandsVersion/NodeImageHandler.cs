using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Spawning;
using Ubiq.Samples;
using Ubiq.Rooms;
using System.Collections.Generic;
using System.Reflection;

[RequireComponent(typeof(XRGrabInteractable))]
public class NodeImageHandler : MonoBehaviour
{
    [Header("Configuración de Ubiq")]
    public NetworkSpawnManager spawnManager;
    public PrefabCatalogue catalogue;

    [Header("UI Prefab (Panel con Content y CloseButton)")]
    public GameObject imagePanelPrefab;

    [Header("Ajuste de posición del panel")]
    public Vector3 panelLocalOffset = new Vector3(2.0f, 0f, 0f);
    public float panelScale = 0.002f;

    private DataDeNodo data;
    private XRGrabInteractable grabInteractable;
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

        // Encontrar el índice del prefab en el catálogo
        FindPrefabIndex();

        // Suscribirse al evento de spawn
        if (spawnManager != null)
        {
            spawnManager.OnSpawned.AddListener(HandleSpawnedObject);
        }
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

        if (catalogue == null || imagePanelPrefab == null)
        {
            return;
        }

        // Buscar el prefab en el catálogo
        for (int i = 0; i < catalogue.prefabs.Count; i++)
        {
            if (catalogue.prefabs[i] == imagePanelPrefab)
            {
                prefabIndex = i;
                Debug.Log($"Prefab encontrado en el catálogo en la posición: {prefabIndex}");
                return;
            }
        }

        Debug.LogWarning($"Prefab {imagePanelPrefab.name} no encontrado en el catálogo");
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
        Debug.Log("Solicitado spawn de panel de imágenes en red.");
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

        // Fallback: intentar spawnear directamente
        Debug.LogWarning("No se pudo acceder al método interno. Intentando spawn directo.");
        spawnManager.SpawnWithRoomScope(imagePanelPrefab);
    }

    // Manejador para el evento OnSpawned
    private void HandleSpawnedObject(GameObject spawnedObject, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
    {
        // Verificar si es el panel de imágenes que queremos configurar
        if (spawnedObject.name.StartsWith(imagePanelPrefab.name))
        {
            // Configurar el panel
            var imageCanvas = spawnedObject.GetComponent<ImageCanvas>();
            if (imageCanvas != null)
            {
                imageCanvas.transform.SetParent(transform, false);
                imageCanvas.transform.localPosition = panelLocalOffset;
                imageCanvas.transform.localRotation = Quaternion.identity;
                imageCanvas.transform.localScale = Vector3.one * panelScale;
                imageCanvas.owner = true;

                // Poblar imágenes
                var content = imageCanvas.transform.Find("Content");
                if (content == null)
                {
                    Debug.LogError("Prefab debe tener un hijo llamado 'Content'.");
                    return;
                }

                foreach (var fname in data.imageFiles)
                {
                    var go2 = new GameObject("IMG_" + fname, typeof(Image));
                    go2.transform.SetParent(content, false);

                    var img = go2.GetComponent<Image>();
                    var spr = Resources.Load<Sprite>("Images/" + System.IO.Path.GetFileNameWithoutExtension(fname));
                    if (spr != null)
                        img.sprite = spr;
                    else
                        Debug.LogWarning($"No encontré Resources/Images/{fname}");
                }
            }

            Debug.Log($"Panel de imágenes spawneado y configurado: {spawnedObject.name}");
        }
    }

    void OnDestroy()
    {
        // Limpiar el listener cuando el objeto sea destruido
        if (spawnManager != null)
        {
            spawnManager.OnSpawned.RemoveListener(HandleSpawnedObject);
        }

        // Limpiar listeners del grab interactable
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }
}