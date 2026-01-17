using Unity.Netcode;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
        // Start host or client here
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        var instance = Instantiate(playerPrefab);
        var netObj = instance.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);
    }
}
