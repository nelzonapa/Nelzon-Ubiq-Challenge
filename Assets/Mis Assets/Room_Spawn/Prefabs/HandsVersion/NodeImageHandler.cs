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

        // Encontrar el �ndice del prefab en el cat�logo
        FindPrefabIndex();

        // Suscribirse al evento de spawn
        if (spawnManager != null)
        {
            spawnManager.OnSpawned.AddListener(HandleSpawnedObject);
        }
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

        if (catalogue == null || imagePanelPrefab == null)
        {
            return;
        }

        // Buscar el prefab en el cat�logo
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

        // Spawnear el objeto usando el �ndice del cat�logo
        SpawnWithRoomScopeByIndex(prefabIndex);
        Debug.Log("Solicitado spawn de panel de im�genes en red.");
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

        // Fallback: intentar spawnear directamente
        Debug.LogWarning("No se pudo acceder al m�todo interno. Intentando spawn directo.");
        spawnManager.SpawnWithRoomScope(imagePanelPrefab);
    }

    // Manejador para el evento OnSpawned
    private void HandleSpawnedObject(GameObject spawnedObject, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
    {
        // Verificar si es el panel de im�genes que queremos configurar
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

                // Poblar im�genes
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
                        Debug.LogWarning($"No encontr� Resources/Images/{fname}");
                }
            }

            Debug.Log($"Panel de im�genes spawneado y configurado: {spawnedObject.name}");
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