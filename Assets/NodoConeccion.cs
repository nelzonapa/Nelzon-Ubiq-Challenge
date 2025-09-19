using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Spawning;

[RequireComponent(typeof(XRGrabInteractable))]
public class NodoConeccion : MonoBehaviour
{
    public NetworkId NetworkId { get; set; }
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
        if (NetworkId == NetworkId.Null)
        {
            // Genera un NetworkId �nico usando el hash del nombre o una l�gica custom
            NetworkId = new NetworkId((uint)transform.GetInstanceID());
        }

        context = NetworkScene.Register(this, NetworkId); // Registra con el NetworkId expl�cito:cite[10]

        // Configuraci�n de propiedad mediante XRGrabInteractable
        var grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(_ => owner = true);
        grab.selectExited.AddListener(_ => owner = false);
    }

    void FixedUpdate()
    {
        // Solo el propietario env�a actualizaciones de posici�n y rotaci�n
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
