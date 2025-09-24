using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI; // Necesario para Button

public class ImageCanvas : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public bool owner;
    private NetworkContext context;
    private XRGrabInteractable grabInteractable;
    private Button closeButton;

    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public Message(Transform t)
        {
            position = t.position;
            rotation = t.rotation;
            scale = t.localScale;
        }
    }

    private void Start()
    {
        context = NetworkScene.Register(this);
        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(OnSelectEntered);
        grabInteractable.selectExited.AddListener(OnSelectExited);

        // Configuración para múltiples agarres
        grabInteractable.selectMode = InteractableSelectMode.Multiple;

        // Configurar el botón de cerrar
        SetupCloseButton();
    }

    public void SetupCloseButton()
    {
        // Buscar el botón de cerrar en los hijos
        closeButton = transform.Find("CloseButton")?.GetComponent<Button>();

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);

            // Asegurarse de que el botón esté visible y funcional
            closeButton.gameObject.SetActive(true);

            // Configurar la posición y tamaño del botón para que sea visible
            RectTransform buttonRect = closeButton.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                // Posicionar en la esquina superior derecha
                buttonRect.anchorMin = new Vector2(1, 1);
                buttonRect.anchorMax = new Vector2(1, 1);
                buttonRect.pivot = new Vector2(1, 1);
                buttonRect.anchoredPosition = new Vector2(-10, -10); // Margen desde la esquina
                buttonRect.sizeDelta = new Vector2(50, 50); // Tamaño del botón
            }
        }
        else
        {
            Debug.LogWarning("No se encontró el botón de cerrar en los hijos del ImageCanvas");
        }
    }

    public void OnCloseButtonClicked()
    {
        // Destruir este panel de imagen
        Destroy(gameObject);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Solo se convierte en owner si es el primer interactor
        if (grabInteractable.interactorsSelecting.Count == 1)
        {
            owner = true;
        }
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        // Deja de ser owner cuando no hay interactores
        if (grabInteractable.interactorsSelecting.Count == 0)
        {
            owner = false;
        }
    }

    void FixedUpdate()
    {
        if (owner)
        {
            context.SendJson(new Message(transform));
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        if (!owner) // Solo procesa mensajes si no es el owner
        {
            var data = msg.FromJson<Message>();
            transform.position = data.position;
            transform.rotation = data.rotation;
            transform.localScale = data.scale;
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }

        // Limpiar el listener del botón
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
    }
}