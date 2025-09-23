using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Spawning;
using Ubiq.Samples;

[RequireComponent(typeof(XRGrabInteractable))]
public class NodeImageHandler : MonoBehaviour
{
    [Header("UI Prefab (Panel con Content y CloseButton)")]
    public GameObject imagePanelPrefab;

    [Header("Ajuste de posición del panel")]
    public Vector3 panelLocalOffset = new Vector3(2.0f, 0f, 0f);
    public float panelScale = 0.002f;

    private DataDeNodo data;
    private XRGrabInteractable grabInteractable;
    private NetworkSpawnManager spawnManager;

    void Awake()
    {
        spawnManager = NetworkSpawnManager.Find(this);
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
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (imagePanelPrefab == null)
        {
            Debug.LogError("Asignar imagePanelPrefab en el Inspector.");
            return;
        }

        var go = spawnManager.SpawnWithPeerScope(imagePanelPrefab);
        var imageCanvas = go.GetComponent<ImageCanvas>();
        imageCanvas.transform.SetParent(transform, false);
        imageCanvas.transform.localPosition = panelLocalOffset;
        imageCanvas.transform.localRotation = Quaternion.identity;
        imageCanvas.transform.localScale = Vector3.one * panelScale;
        imageCanvas.owner = true;

        // 2) Poblar imágenes
        var content = imageCanvas.transform.Find("Content");
        if (content == null)
        {
            Debug.LogError("Prefab debe tener un hijo llamado 'Content'.");
            return;
        }

        StartCoroutine(LoadImagesAndAdjustCollider(content, imageCanvas));
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

            // Configurar tamaño inicial del RectTransform
            rectTransform.sizeDelta = new Vector2(400f, 300f); // Tamaño por defecto

            var img = go2.GetComponent<Image>();
            var spr = Resources.Load<Sprite>("Images/" + System.IO.Path.GetFileNameWithoutExtension(fname));

            if (spr != null)
            {
                img.sprite = spr;
                // Ajustar el tamaño del RectTransform al de la sprite
                rectTransform.sizeDelta = new Vector2(spr.rect.width, spr.rect.height);
            }
            else
            {
                Debug.LogWarning($"No encontré Resources/Images/{fname}");
            }

            // Esperar un frame entre cada imagen para que el layout se actualice
            yield return null;
        }

        // Forzar el ajuste del collider después de cargar todas las imágenes
        var colliderAdjuster = imageCanvas.GetComponent<DynamicColliderAdjuster>();
        if (colliderAdjuster != null)
        {
            colliderAdjuster.ForceAdjustment();
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
        }
    }
}