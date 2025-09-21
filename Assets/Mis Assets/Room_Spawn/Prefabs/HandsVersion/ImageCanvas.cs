using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Spawning;
using UnityEngine.UI;

public class ImageCanvas : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public bool owner;
    private NetworkContext context;

    // Variables para configuración
    private int parentNodeId = -1;
    private string[] imageFiles;
    private Vector3 localOffset;
    private float scaleFactor;
    private bool isConfigured = false;

    private struct TransformMessage
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    // Nuevo struct para mensajes de configuración
    public struct ConfigurationMessage
    {
        public int parentNodeId;
        public string[] imageFiles;
        public Vector3 localOffset;
        public float scaleFactor;
    }

    private void Start()
    {
        context = NetworkScene.Register(this);

        var grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.selectEntered.AddListener(_ => owner = true);
            grab.selectExited.AddListener(_ => owner = false);
        }

        // Inicialmente desactivado hasta estar configurado
        gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (owner)
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
        // Intentar procesar como mensaje de configuración
        try
        {
            var configMsg = msg.FromJson<ConfigurationMessage>();
            ConfigurePanel(configMsg);
            return;
        }
        catch (System.Exception e)
        {
            Debug.Log("No es mensaje de configuración: " + e.Message);
        }

        // Si no es configuración, procesar como mensaje de transformación
        try
        {
            var transformMsg = msg.FromJson<TransformMessage>();
            transform.position = transformMsg.position;
            transform.rotation = transformMsg.rotation;
            transform.localScale = transformMsg.scale;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Error procesando mensaje de transformación: " + e.Message);
        }
    }

    private void ConfigurePanel(ConfigurationMessage config)
    {
        parentNodeId = config.parentNodeId;
        imageFiles = config.imageFiles;
        localOffset = config.localOffset;
        scaleFactor = config.scaleFactor;

        // Buscar el nodo padre
        if (GeneradorNodos.nodosPorId != null && GeneradorNodos.nodosPorId.ContainsKey(parentNodeId))
        {
            GameObject parentNode = GeneradorNodos.nodosPorId[parentNodeId];
            transform.SetParent(parentNode.transform, false);
            transform.localPosition = localOffset;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one * scaleFactor;

            // Configurar imágenes
            SetupImages();

            isConfigured = true;
            gameObject.SetActive(true);
            Debug.Log("Panel configurado correctamente para el nodo: " + parentNodeId);
        }
        else
        {
            Debug.LogError("No se encontró el nodo padre con ID: " + parentNodeId);
        }
    }

    private void SetupImages()
    {
        var content = transform.Find("Content");
        if (content == null)
        {
            Debug.LogError("No se encontró el objeto 'Content' en el panel");
            return;
        }

        // Limpiar contenido existente
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Crear nuevas imágenes
        foreach (var filename in imageFiles)
        {
            var imageGO = new GameObject("IMG_" + filename, typeof(Image));
            imageGO.transform.SetParent(content, false);

            var image = imageGO.GetComponent<Image>();
            var sprite = Resources.Load<Sprite>("Images/" + System.IO.Path.GetFileNameWithoutExtension(filename));

            if (sprite != null)
            {
                image.sprite = sprite;
            }
            else
            {
                Debug.LogWarning("No se encontró la imagen: " + filename);
            }
        }
    }

    // Método público para enviar configuración
    public void SendConfiguration(ConfigurationMessage config)
    {
        context.SendJson(config);
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