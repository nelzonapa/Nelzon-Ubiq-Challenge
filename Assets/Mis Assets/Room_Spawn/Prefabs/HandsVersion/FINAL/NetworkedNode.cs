using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class NetworkedNode : MonoBehaviour
{
    private NetworkContext context;
    private bool isOwner = false;
    private XRGrabInteractable grabInteractable;

    private struct TransformMessage
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    void Start()
    {
        // Obtener el ID del nodo desde DataDeNodo
        var data = GetComponent<DataDeNodo>();
        if (data == null)
        {
            Debug.LogError("NetworkedNode requiere un componente DataDeNodo");
            return;
        }

        // Crear un NetworkId único basado en el ID del nodo
        // Usamos un offset para evitar conflictos con otros objetos de red
        uint networkIdValue = (uint)data.nodeId + 1000000; // Offset para nodos
        NetworkId networkId = new NetworkId(networkIdValue);

        // Registrar manualmente con un NetworkId único
        context = NetworkScene.Register(this, networkId);

        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
        else
        {
            Debug.LogWarning("NetworkedNode: No se encontró XRGrabInteractable en " + gameObject.name);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isOwner = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isOwner = false;
    }

    void FixedUpdate()
    {
        if (isOwner)
        {
            context.SendJson(new TransformMessage
            {
                position = transform.position,
                rotation = transform.rotation,
                scale = transform.localScale
            });
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<TransformMessage>();
        transform.position = data.position;
        transform.rotation = data.rotation;
        transform.localScale = data.scale;
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }
}