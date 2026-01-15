using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UpdatePlayerNumber : NetworkBehaviour
{
    private TextMeshProUGUI playerNumberText;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerNumberText = GetComponent<TextMeshProUGUI>();

    }
    private void FixedUpdate()
    {
        playerNumberText.text = $"{NetworkManager.Singleton.ConnectedClients.Count}/{LobbyManager.Instance._lobby.MaxPlayers}";
    }
}
