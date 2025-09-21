using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ConectionWithSpawnObjects : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public bool owner;
    private NetworkContext context;
    private bool isNetworkIdInitialized = false;

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
        // No registrar inmediatamente. Esperar a que NetworkId esté inicializado.
        CheckAndRegister();
    }

    private void Update()
    {
        // Si NetworkId no se ha inicializado, intentar registrar en cada frame hasta que esté listo.
        if (!isNetworkIdInitialized)
        {
            CheckAndRegister();
        }
    }

    private void CheckAndRegister()
    {
        // Verificar si NetworkId está inicializado (no es nulo o vacío)
        if (NetworkId != NetworkId.Null && !isNetworkIdInitialized)
        {
            context = NetworkScene.Register(this);
            isNetworkIdInitialized = true;

            var grab = GetComponent<XRGrabInteractable>();
            if (grab != null)
            {
                grab.selectEntered.AddListener(_ => owner = true);
                grab.selectExited.AddListener(_ => owner = false);
            }
        }
    }

    void FixedUpdate()
    {
        if (owner && isNetworkIdInitialized)
        {
            context.SendJson(new Message(transform));
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
        transform.localScale = data.scale;
    }

    void OnDestroy()
    {
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(_ => owner = true);
            grab.selectExited.RemoveListener(_ => owner = false);
        }
    }
}