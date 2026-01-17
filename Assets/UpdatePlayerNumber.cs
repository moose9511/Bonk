using System.Net;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UpdatePlayerNumber : NetworkBehaviour
{
    private TextMeshProUGUI playerNumberText;

    public int maxPlayers;
    public override void OnNetworkSpawn()
    {
        playerNumberText = GetComponent<TextMeshProUGUI>();
        base.OnNetworkSpawn();

        if (!IsHost) return;
        
        maxPlayers = LobbyManager.Instance._lobby.MaxPlayers;
    }
    private void FixedUpdate()
    {
        if (!IsHost) return;
        if (NetworkManager.Singleton == null)
            return;

        int currentPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
        UpdatePlayerCountRpc(currentPlayers, this.maxPlayers);
    }

    [Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Everyone)]
    public void UpdatePlayerCountRpc(int num1, int num2)
    {
        playerNumberText.text = num1 + " / " + num2;
    }

}
