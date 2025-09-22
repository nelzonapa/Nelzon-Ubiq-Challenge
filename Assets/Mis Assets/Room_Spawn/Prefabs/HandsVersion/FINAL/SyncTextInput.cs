using UnityEngine;
using TMPro;
using Ubiq.Messaging;
using Ubiq.Rooms;
using System;

public class SyncTextInput : MonoBehaviour
{
    private NetworkContext context;
    private TMP_InputField inputField;
    private bool isUpdatingFromNetwork = false;
    private string lastText = "";
    private RoomClient roomClient;

    private struct Message
    {
        public string text;
        public NetworkId senderId;
    }

    void Start()
    {
        context = NetworkScene.Register(this);

        // Obtener la referencia al RoomClient
        roomClient = NetworkScene.Find(this).GetComponent<RoomClient>();
        if (roomClient == null)
        {
            roomClient = FindObjectOfType<RoomClient>();
        }

        // Buscar el TMP_InputField en los hijos (en el objeto "InputField (TMP)")
        inputField = GetComponentInChildren<TMP_InputField>();

        if (inputField != null)
        {
            // Suscribirse a eventos de cambio local de texto
            inputField.onValueChanged.AddListener(OnLocalTextChanged);
            inputField.onEndEdit.AddListener(OnLocalTextSubmitted);

            // Inicializar con el texto actual
            lastText = inputField.text;
            Debug.Log($"SyncTextInput inicializado correctamente con texto: {lastText}");
        }
        else
        {
            Debug.LogError("No se encontró componente TMP_InputField en este GameObject o sus hijos");

            // Intentar encontrar el componente de manera más específica
            Transform inputFieldChild = transform.Find("InputField (TMP)");
            if (inputFieldChild != null)
            {
                inputField = inputFieldChild.GetComponent<TMP_InputField>();
                if (inputField != null)
                {
                    inputField.onValueChanged.AddListener(OnLocalTextChanged);
                    inputField.onEndEdit.AddListener(OnLocalTextSubmitted);
                    lastText = inputField.text;
                    Debug.Log("TMP_InputField encontrado mediante búsqueda específica");
                }
            }
        }
    }

    private void OnLocalTextChanged(string newText)
    {
        // Solo enviar cambios si no provienen de la red
        if (!isUpdatingFromNetwork && newText != lastText)
        {
            lastText = newText;

            // Enviar cambio a través de la red
            context.SendJson(new Message()
            {
                text = newText,
                senderId = roomClient.Me.networkId
            });

            Debug.Log($"Texto cambiado localmente: {newText}");
        }
    }

    private void OnLocalTextSubmitted(string finalText)
    {
        // Opcional: enviar texto final cuando se presiona Enter
        if (!isUpdatingFromNetwork)
        {
            context.SendJson(new Message()
            {
                text = finalText,
                senderId = roomClient.Me.networkId
            });

            Debug.Log($"Texto enviado (Enter): {finalText}");
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();

        // Actualizar texto desde la red
        isUpdatingFromNetwork = true;

        if (inputField != null)
        {
            inputField.text = msg.text;
            lastText = msg.text;

            // Mantener el cursor en posición correcta
            inputField.caretPosition = msg.text.Length;

            Debug.Log($"Texto actualizado desde red: {msg.text}");
        }

        isUpdatingFromNetwork = false;
    }

    void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnLocalTextChanged);
            inputField.onEndEdit.RemoveListener(OnLocalTextSubmitted);
        }
    }
}