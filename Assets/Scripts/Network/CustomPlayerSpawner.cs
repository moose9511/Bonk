using System;
using Unity.Netcode;
using UnityEngine;

public class CustomPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    [SerializeField] public Quaternion spawnQuaternion;
    [SerializeField] private Transform spawnTransform;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Only the server should handle spawning
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnect;
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (NetworkManager.Singleton.ConnectedClients[id].PlayerObject == null)
                Debug.LogError($"Client {id} has NO PlayerObject!");
        };


        // Spawn for already connected clients (host included)
    }

    private void ClientConnect(ulong obj)
    {
    }
}
