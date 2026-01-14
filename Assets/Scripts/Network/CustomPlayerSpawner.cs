using Unity.Netcode;
using UnityEngine;

public class CustomPlayerSpawner : MonoBehaviour
{
	[SerializeField] private GameObject playerPrefab;
	private void Awake()
	{
		if(NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

		foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
		{
			OnClientConnected(clientId);
        }
    }

	private void OnClientConnected(ulong clientId)
	{
		Debug.Log($"Client connected: {clientId}");
        Vector3 spawnPos = Vector3.zero;

		// Spawn the player manually
		var playerInstance = Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);

		playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
	}

	public void ConnectClient(ulong clientId, GameObject playerPrefab)
	{
		Debug.Log("im i doing anyting");
        var playerInstance = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}

