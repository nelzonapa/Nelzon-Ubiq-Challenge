using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class NodeImageHandler : MonoBehaviour
{
    [Header("UI Prefab (Panel con Content y CloseButton)")]
    public GameObject imagePanelPrefab;

    private DataDeNodo data;
    private GameObject panelInstance;
    private XRGrabInteractable grabInteractable;

    [Header("Ajuste de posición del panel (offset local)")]
    public Vector3 panelLocalOffset = new Vector3(0.2f, 0.2f, 0f);

    void Awake()
    {
        data = GetComponent<DataDeNodo>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
        else
            Debug.LogWarning("NodeImageHandler requiere XRGrabInteractable.");
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (panelInstance != null) return;
        if (imagePanelPrefab == null)
        {
            Debug.LogError("NodeImageHandler: asigna imagePanelPrefab en Inspector.");
            return;
        }

        // Instanciar y parentear al nodo para que siga sus movimientos
        panelInstance = Instantiate(imagePanelPrefab);
        panelInstance.transform.SetParent(transform, false);
        UpdatePanelTransform();

        // Poblar imágenes en "Content"
        var content = panelInstance.transform.Find("Content");
        if (content == null)
        {
            Debug.LogError("NodeImageHandler: prefab debe tener hijo 'Content'.");
            return;
        }

        foreach (var fname in data.imageFiles)
        {
            var go = new GameObject("IMG_" + fname, typeof(Image));
            go.transform.SetParent(content, false);
            var img = go.GetComponent<Image>();
            var spr = Resources.Load<Sprite>("Images/" + System.IO.Path.GetFileNameWithoutExtension(fname));
            if (spr != null)
                img.sprite = spr;
            else
                Debug.LogWarning($"NodeImageHandler: no encontré Resources/Images/{fname}");
        }

        // Botón cerrar
        var btn = panelInstance.transform.Find("CloseButton")?.GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(() => { Destroy(panelInstance); panelInstance = null; });
        else
            Debug.LogWarning("NodeImageHandler: falta botón 'CloseButton'.");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (panelInstance != null)
        {
            Destroy(panelInstance);
            panelInstance = null;
        }
    }

    void LateUpdate()
    {
        // Mantener el panel siguiendo al nodo con offset
        if (panelInstance != null)
            UpdatePanelTransform();
    }

    private void UpdatePanelTransform()
    {
        panelInstance.transform.localPosition = panelLocalOffset;
        panelInstance.transform.localRotation = Quaternion.identity;
        // Ajustar escala si es necesario (opcional)
        panelInstance.transform.localScale = Vector3.one * 0.002f;
    }
}
