using UnityEngine;
using Unity.Netcode;
using System;

public class MapSpawner : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
	}

    private void OnClientConnected(ulong obj)
    {

	}

    [ClientRpc]
	public void SpawnInClientRpc()
    { 
        
    }

}
