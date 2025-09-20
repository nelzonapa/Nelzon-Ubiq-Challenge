using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Spawning;
public class ConectionNoSpawnObject : MonoBehaviour
{
    private NetworkContext context;
    private bool owner;

    // Mensaje simplificado que solo contiene transform
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

    void Start()
    {

        context = NetworkScene.Register(this);

        // Configuración de propiedad mediante XRGrabInteractable
        var grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(_ => owner = true);
        grab.selectExited.AddListener(_ => owner = false);
    }

    void FixedUpdate()
    {
        // Solo el propietario envía actualizaciones de posición y rotación
        if (owner)
        {
            context.SendJson(new Message(transform));
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
