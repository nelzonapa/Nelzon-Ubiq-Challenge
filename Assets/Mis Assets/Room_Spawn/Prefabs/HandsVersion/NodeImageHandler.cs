using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Spawning;
using Ubiq.Samples;
using Ubiq.Rooms;

[RequireComponent(typeof(XRGrabInteractable))]
public class NodeImageHandler : MonoBehaviour
{
    [Header("UI Prefab (Panel con Content y CloseButton)")]
    public GameObject imagePanelPrefab;

    [Header("Ajuste de posici�n del panel")]
    public Vector3 panelLocalOffset = new Vector3(2.0f, 0f, 0f);
    public float panelScale = 0.002f;

    private DataDeNodo data;
    private XRGrabInteractable grabInteractable;
    private NetworkSpawnManager spawnManager;
    private int imagePanelPrefabIndex = -1;
    public PrefabCatalogue catalogue;

    void Awake()
    {
        spawnManager = NetworkSpawnManager.Find(this);
        data = GetComponent<DataDeNodo>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        // Intentar encontrar el cat�logo si no est� asignado
        if (catalogue == null && spawnManager != null)
        {
            catalogue = spawnManager.catalogue;
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
        else
        {
            Debug.LogWarning("NodeImageHandler requiere un XRGrabInteractable.");
        }

        // Encontrar el �ndice del prefab en el cat�logo
        FindImagePanelPrefabIndex();
    }

    void OnValidate()
    {
        // Actualizar el �ndice cuando se cambie el prefab en el inspector
        FindImagePanelPrefabIndex();
    }

    private void FindImagePanelPrefabIndex()
    {
        imagePanelPrefabIndex = -1;

        //if (spawnManager == null || spawnManager.catalogue == null || imagePanelPrefab == null)
        //{
        //    return;
        //}

        if (spawnManager == null || catalogue == null || imagePanelPrefab == null)
        {
            return;
        }

        // Buscar el prefab en el cat�logo
        for (int i = 0; i < spawnManager.catalogue.prefabs.Count; i++)
        {
            if (spawnManager.catalogue.prefabs[i] == imagePanelPrefab)
            {
                imagePanelPrefabIndex = i;
                Debug.Log($"ImagePanel prefab encontrado en el cat�logo en la posici�n: {imagePanelPrefabIndex}");
                return;
            }
        }

        Debug.LogWarning($"ImagePanel prefab {imagePanelPrefab.name} no encontrado en el cat�logo");
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (imagePanelPrefab == null)
        {
            Debug.LogError("Asignar imagePanelPrefab en el Inspector.");
            return;
        }

        if (imagePanelPrefabIndex == -1)
        {
            Debug.LogError("El ImagePanel prefab no est� en el cat�logo de Ubiq. No se puede spawnear.");
            return;
        }

        // Spawnear el panel usando Room Scope para que todos lo vean
        SpawnImagePanelWithRoomScope();
    }

    private void SpawnImagePanelWithRoomScope()
    {
        // Usar reflection para acceder al m�todo SpawnWithRoomScope interno, similar a tu script SpawnObject
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
                    method.Invoke(internalSpawner, new object[] { imagePanelPrefabIndex });
                    Debug.Log("Solicitado spawn de ImagePanel en red con Room Scope.");

                    // Suscribirse al evento para configurar el panel cuando sea spawneado
                    if (spawnManager != null)
                    {
                        spawnManager.OnSpawned.AddListener(HandleSpawnedImagePanel);
                    }
                    return;
                }
            }
        }

        // Fallback: intentar spawnear directamente
        Debug.LogWarning("No se pudo acceder al m�todo interno. Intentando spawn directo.");
        spawnManager.SpawnWithRoomScope(imagePanelPrefab);

        // Suscribirse al evento para configurar el panel cuando sea spawneado
        if (spawnManager != null)
        {
            spawnManager.OnSpawned.AddListener(HandleSpawnedImagePanel);
        }
    }

    // Manejador para el evento OnSpawned del ImagePanel
    private void HandleSpawnedImagePanel(GameObject spawnedObject, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
    {
        // Verificar si es el ImagePanel que queremos configurar
        if (spawnedObject.name.StartsWith(imagePanelPrefab.name))
        {
            // Configurar la posici�n relativa al nodo
            spawnedObject.transform.SetParent(transform, false);
            spawnedObject.transform.localPosition = panelLocalOffset;
            spawnedObject.transform.localRotation = Quaternion.identity;
            spawnedObject.transform.localScale = Vector3.one * panelScale;

            // Obtener el componente ImageCanvas y configurarlo
            var imageCanvas = spawnedObject.GetComponent<ImageCanvas>();
            if (imageCanvas != null)
            {
                imageCanvas.owner = true;
            }

            // Poblar las im�genes en el panel
            var content = spawnedObject.transform.Find("Content");
            if (content == null)
            {
                Debug.LogError("Prefab debe tener un hijo llamado 'Content'.");
                return;
            }

            StartCoroutine(LoadImagesAndAdjustCollider(content, imageCanvas));

            Debug.Log($"ImagePanel spawneado y configurado: {spawnedObject.name}");

            // Remover el listener despu�s de configurar el panel
            if (spawnManager != null)
            {
                spawnManager.OnSpawned.RemoveListener(HandleSpawnedImagePanel);
            }
        }
    }

    private System.Collections.IEnumerator LoadImagesAndAdjustCollider(Transform content, ImageCanvas imageCanvas)
    {
        // Esperar un frame para que el layout se actualice
        yield return null;

        foreach (var fname in data.imageFiles)
        {
            var go2 = new GameObject("IMG_" + fname, typeof(RectTransform), typeof(Image));
            var rectTransform = go2.GetComponent<RectTransform>();
            rectTransform.SetParent(content, false);

            // Configurar tama�o inicial del RectTransform
            rectTransform.sizeDelta = new Vector2(400f, 300f);

            var img = go2.GetComponent<Image>();
            var spr = Resources.Load<Sprite>("Images/" + System.IO.Path.GetFileNameWithoutExtension(fname));

            if (spr != null)
            {
                img.sprite = spr;
                rectTransform.sizeDelta = new Vector2(spr.rect.width, spr.rect.height);
            }
            else
            {
                Debug.LogWarning($"No encontr� Resources/Images/{fname}");
            }

            yield return null;
        }

        // Forzar el ajuste del collider despu�s de cargar todas las im�genes
        var colliderAdjuster = imageCanvas.GetComponent<DynamicColliderAdjuster>();
        if (colliderAdjuster != null)
        {
            colliderAdjuster.ForceAdjustment();
        }

        // Asegurar que el bot�n de cerrar est� por encima del contenido
        Transform closeButton = imageCanvas.transform.Find("CloseButton");
        if (closeButton != null)
        {
            closeButton.SetAsLastSibling();
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }

        // Limpiar el listener cuando el objeto sea destruido
        if (spawnManager != null)
        {
            spawnManager.OnSpawned.RemoveListener(HandleSpawnedImagePanel);
        }
    }
}