using System;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Rooms;
using UnityEngine;

public class UnirseRandomSoloAlComenzar : MonoBehaviour
{
    private void Start()
    {
        GetComponent<RoomClient>().Join(Guid.NewGuid());
    }
}
