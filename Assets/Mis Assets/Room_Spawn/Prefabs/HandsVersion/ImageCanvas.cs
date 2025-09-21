using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Spawning;

public class ImageCanvas : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public bool owner;
    private NetworkContext context;

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

        var grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(_ => owner = true);
        grab.selectExited.AddListener(_ => owner = false);
    }
    void FixedUpdate()
    {
        // Solo el propietario envía actualizaciones de posición y rotación
        if (owner)
        {
            context.SendJson(new Message(transform));//envía mensaje
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        // Llega un mensaje desde otro cliente
        var data = msg.FromJson<Message>();
        transform.position = data.position;
        transform.rotation = data.rotation;
        transform.localScale = data.scale;
    }

    void OnDestroy()
    {
        // Limpieza de listeners si fuera necesario
        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(_ => owner = true);
            grab.selectExited.RemoveListener(_ => owner = false);
        }
    }
}
