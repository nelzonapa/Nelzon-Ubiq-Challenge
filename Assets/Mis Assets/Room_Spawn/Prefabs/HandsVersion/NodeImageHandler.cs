using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class NodeImageHandler : MonoBehaviour
{
    [Header("UI Prefab (Panel con Content y CloseButton)")]
    public GameObject imagePanelPrefab;

    [Header("Ajuste de posición del panel")]
    public Vector3 panelLocalOffset = new Vector3(2.0f, 0f, 0f);
    public float panelScale = 0.002f;

    private DataDeNodo data;
    private GameObject panelInstance;
    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        data = GetComponent<DataDeNodo>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            // ¡OJO! Ya no nos suscribimos a selectExited
        }
        else
        {
            Debug.LogWarning("NodeImageHandler requiere un XRGrabInteractable.");
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (panelInstance != null) return;
        if (imagePanelPrefab == null)
        {
            Debug.LogError("Asignar imagePanelPrefab en el Inspector.");
            return;
        }

        // 1) Instanciar y parentear al nodo
        panelInstance = Instantiate(imagePanelPrefab);
        panelInstance.transform.SetParent(transform, false);
        panelInstance.transform.localPosition = panelLocalOffset;
        panelInstance.transform.localRotation = Quaternion.identity;
        panelInstance.transform.localScale = Vector3.one * panelScale;

        // 2) Poblar imágenes
        var content = panelInstance.transform.Find("Content");
        if (content == null)
        {
            Debug.LogError("Prefab debe tener un hijo llamado 'Content'.");
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
                Debug.LogWarning($"No encontré Resources/Images/{fname}");
        }

        // 3) Botón cerrar — destruye el panel
        var btn = panelInstance.transform.Find("CloseButton")?.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() =>
            {
                Destroy(panelInstance);
                panelInstance = null;
            });
        }
        else
        {
            Debug.LogWarning("Prefab debe tener un Button hijo llamado 'CloseButton'.");
        }
    }
}
