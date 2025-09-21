using System;
using System.Collections;
using System.Collections.Generic;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ubiq.Examples
{
    /*
     Cuando empieza el juego, el código genera un ID único (como un código de sala).
    Busca todos los "clientes de red" (personas o cosas que se quieren conectar).
    Los une a la misma sala usando ese ID, para que puedan interactuar entre ellos.
     */
    public class _10_JoinRoomClients : MonoBehaviour
    {
        private void Start()
        {
            //Cuando empieza el juego, el código genera un ID único (como un código de sala).
            var guid = Guid.NewGuid();
            //Busca todos los "clientes de red" (personas o cosas que se quieren conectar).
            foreach (var roomClient in GetComponentsInChildren<RoomClient>())
            {
                //Los une a la misma sala usando ese ID, para que puedan interactuar entre ellos.
                roomClient.Join(guid);
            }
        }
    }
}