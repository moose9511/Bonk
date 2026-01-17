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

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (NetworkManager.Singleton.ConnectedClients[id].PlayerObject == null)
                Debug.LogError($"Client {id} has NO PlayerObject!");
        };


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
        if(gameObject.scene.name == "WaitingRoom")
        {
            //var playerInstance = Instantiate(playerPrefab, spawnTransform.position, spawnQuaternion);

            //playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        } else
        {

        }
           
	}

}
