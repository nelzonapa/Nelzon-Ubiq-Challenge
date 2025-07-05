using System;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Rooms;
using UnityEngine;

public class UnirseRandomPeer : MonoBehaviour
{

    public RoomClient roomClient;

    public void JoinRandom()
    {
        if (roomClient)
        {
            roomClient.Join(Guid.NewGuid());
        }
    }
}
