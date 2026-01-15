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

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("map1", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
