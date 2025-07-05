using System.Collections;
using System.Collections.Generic;
using Ubiq.Rooms;
using UnityEngine;

public class UnirseConOtroPeer : MonoBehaviour
{

    public RoomClient miRoomClient;
    public RoomClient otroPeerDeRoomClient;

    public void Join() 
    {
        if (otroPeerDeRoomClient != null && otroPeerDeRoomClient.Room != null) 
        {
            miRoomClient.Join(otroPeerDeRoomClient.Room.JoinCode);
        }
    }
}
