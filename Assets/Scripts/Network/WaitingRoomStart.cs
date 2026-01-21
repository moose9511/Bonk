using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class WaitingRoomStart : NetworkBehaviour
{
    private Button beginBtn;

    override public void OnNetworkSpawn()
    {
        if(!IsHost) {
            gameObject.SetActive(false);
            return;
        }

        beginBtn = GetComponent<Button>(); 
    }

    private void Update()
    {
        if(!IsHost) return;
        if(Input.GetKeyDown(KeyCode.B))
        {
            StartGame();
        }
    }
    public void StartGame()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        //foreach (var player in players)
        //{
        //    player.DestroyPlayerServerRpc();
        //    Debug.Log("destory player");
        //}
        LobbyManager.Instance._lobby.Data["Waiting"] = new DataObject(
            value: "false",
            visibility: DataObject.VisibilityOptions.Public,
            index: DataObject.IndexOptions.S1
        );
        NetworkManager.Singleton.ConnectedClients[0].PlayerObject.GetComponent<Player>().StartAnimationsClientRpc();
        NetworkManager.Singleton.SceneManager.LoadScene("map1", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    
}
