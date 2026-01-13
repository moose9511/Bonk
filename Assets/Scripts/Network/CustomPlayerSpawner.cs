using Unity.Netcode;
using UnityEngine;

public class CustomPlayerSpawner : MonoBehaviour
{
	private void Start()
	{
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
	}

	private void OnClientConnected(ulong clientId)
	{
		if (!NetworkManager.Singleton.IsServer)
			return;

		// Pick a spawn point based on join order
		int index = NetworkManager.Singleton.ConnectedClients.Count - 1;

		Vector3 spawnPos = Vector3.zero;

		// Spawn the player manually
		var playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;
		var playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

		playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
	}
}

