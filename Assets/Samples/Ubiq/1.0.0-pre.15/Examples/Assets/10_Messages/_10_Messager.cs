using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using System;

namespace Ubiq.Examples
{
    public class _10_Messager : MonoBehaviour
    {
        //Guarda la "conexión de red" del objeto.
        private NetworkContext context;

        private void Start()
        {
            //"Registra" el objeto en la red usando NetworkScene.Register(this).
            context = NetworkScene.Register(this);
        }

        public void SetColor(Color32 color)
        {
            //Cambia el color del objeto localmente
            GetComponentInChildren<Renderer>().material.color = color;

            //Si está conectado a una red (context.Scene != null), envía el color a los demás usando context.SendJson().
            if (context.Scene != null)
            {
                context.SendJson<Color32>(color);
            }
        }

        /*
         Se ejecuta cuando llega un mensaje de otro jugador.
        Extrae el color del mensaje (message.FromJson<Color32>()) y lo aplica al objeto.
         */
        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var color = message.FromJson<Color32>();
            GetComponentInChildren<Renderer>().material.color = color;
        }
    }
}