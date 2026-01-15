using Unity.Netcode;
using UnityEngine;

public class CustomPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private Quaternion spawnQuaternion;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Only the server should handle spawning
        if (!IsServer) return;

        Debug.Log("CustomPlayerSpawner: OnNetworkSpawn - Subscribing to client connected callback");
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Spawn for already connected clients (host included)
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;
        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        Debug.Log($"Spawning player for client {clientId}");

        var playerInstance = Instantiate(playerPrefab, Vector3.zero, spawnQuaternion);

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        playerInstance.GetComponent<Player>().EnableCanvas(false);


    }
}
