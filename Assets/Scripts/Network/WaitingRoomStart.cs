using Unity.Netcode;
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
        NetworkManager.Singleton.SceneManager.LoadScene("map1", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
