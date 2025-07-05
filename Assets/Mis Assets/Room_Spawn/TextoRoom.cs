using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Rooms;

public class TextoRoom : MonoBehaviour
{

    public RoomClient roomClient;

    private UnityEngine.UI.Text texto;
    private string textoOriginal;

    // Start is called before the first frame update
    private void Start()
    {
        roomClient.OnJoinedRoom.AddListener(MiRoomConectadoAOtro);

        texto=GetComponent<UnityEngine.UI.Text>();
        textoOriginal=texto.text;
    }

    private void MiRoomConectadoAOtro(IRoom otroRoom)
    {
        if (otroRoom != null && otroRoom.UUID != null && otroRoom.UUID.Length > 0)
        {
            texto.text = $"{textoOriginal} #{otroRoom.UUID.Substring(0,4)}";
        }
    }

}
