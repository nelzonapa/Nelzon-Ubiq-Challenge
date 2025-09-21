using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Rooms;

namespace Ubiq.Examples
{
    public class _20_Spawner : MonoBehaviour
    {
        //Genera objetos (prefabs) en una posición específica (spawnPoint) y los sincroniza en la red.
        /*
         spawnPoint: Lugar donde aparecerán los objetos.
        manager: Gestiona el spawn de objetos en red (NetworkSpawnManager).
         */
        public Transform spawnPoint;
        private NetworkSpawnManager manager;

        //Guarda referencia de los objetos creados localmente para poder borrarlos después.
        /*
         locallySpawned: Lista de objetos creados por este jugador (para borrarlos luego).
         */
        private List<GameObject> locallySpawned = new List<GameObject>();

        private void Start()
        {
            //Obtiene el NetworkSpawnManager (padre) y se suscribe al evento OnSpawned.
            manager = GetComponentInParent<NetworkSpawnManager>();
            manager.OnSpawned.AddListener(Manager_OnSpawned);
        }

        private void Manager_OnSpawned(GameObject go, IRoom room, IPeer peer, NetworkSpawnOrigin origin)
        {
            //Si el objeto fue creado por este jugador (peer == RoomClient.Me), lo guarda en locallySpawned.
            if (peer == GetComponent<RoomClient>().Me)
            {
                locallySpawned.Add(go);
            }
            spawnPoint.GetPositionAndRotation(out var pos, out var rot);

            // Add a tiny bit of jitter so the spawned objects don't balance perfectly!
            //Añade variación aleatoria (jitter) en la posición para evitar que los objetos aparezcan perfectamente alineados.
            pos += UnityEngine.Random.Range(0.0f,0.05f) * Vector3.one;

            //Posiciona el objeto en spawnPoint, con un pequeño desplazamiento aleatorio (para que no se amontonen).
            go.transform.SetPositionAndRotation(pos,rot);
        }

        //Permite limpiar todos los objetos creados por el jugador local.
        public void Clear()
        {
            foreach(var spawned in locallySpawned)
            {
                manager.Despawn(spawned);
            }
            locallySpawned.Clear();
        }

        //Instancia un prefab desde el catálogo del manager y lo sincroniza en la red.
        /*
         Ejemplo: Si prefabIndex = 0, spawneará el primer prefab de la lista.
         */
        public void Spawn(int prefabIndex)
        {
            var prefab = manager.catalogue.prefabs[prefabIndex];
            manager.SpawnWithPeerScope(prefab);
        }
    }
}