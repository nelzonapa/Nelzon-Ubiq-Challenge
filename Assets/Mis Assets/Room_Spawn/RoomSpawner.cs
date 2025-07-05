using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;
using Ubiq.Rooms;

public class RoomSpawner : MonoBehaviour
{

    public Transform puntoSpawneo;
    private NetworkSpawnManager managerDeSpawneo;

    // Start is called before the first frame update
    private void Start()
    {
        managerDeSpawneo = GetComponentInParent<NetworkSpawnManager>();//busca el spawn manager en el inspector
        managerDeSpawneo.OnSpawned.AddListener(funcionDeManagerDeEspawneo);
    }

    private void funcionDeManagerDeEspawneo(GameObject _gameObjetc,IRoom _room, IPeer _peer, NetworkSpawnOrigin _origin) 
    {
        puntoSpawneo.GetPositionAndRotation(out var posicion, out var rotacion);
        
        posicion += UnityEngine.Random.Range(0.0f, 0.05f)*Vector3.one;
        //para hcerlo orgánico o evitar colisiones exactas

        _gameObjetc.transform.SetPositionAndRotation(posicion,rotacion);
    }

    public void SpawneoConUnSoloPeer(int index) 
    {
        if (managerDeSpawneo)
        {
            managerDeSpawneo.SpawnWithPeerScope(managerDeSpawneo.catalogue.prefabs[index]);
        }
    }

    public void SpawneoConTodaLaSala(int index) 
    {
        if (managerDeSpawneo) 
        {
            managerDeSpawneo.SpawnWithRoomScope(managerDeSpawneo.catalogue.prefabs[index]);
        }
    }
}
