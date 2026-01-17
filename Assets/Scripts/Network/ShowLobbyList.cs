using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShowLobbyList : MonoBehaviour
{
    [SerializeField] public ScrollRect lobbyScrollView;

    [SerializeField] private GameObject lobbyEntryPrefab;

    QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
    {
        Count = 25,
        Filters = new System.Collections.Generic.List<QueryFilter>()
    };
	public void Start()
    {
        var options = new InitializationOptions();
		options.SetEnvironmentName("production");
        ShowLobbies();
    }
    public async void ShowLobbies()
    {
        while (true) {
            if(!LobbyManager.Instance.imReadyForYou)
            {
                await Task.Delay(1000);
                continue;
            }

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

            if (lobbyScrollView == null) return;

            foreach (Transform child in lobbyScrollView.content)
            {
                Destroy(child.gameObject);
            }

            foreach (Lobby lobby in lobbies.Results)
            {
                if (lobbyScrollView == null) return;
                GameObject entry = Instantiate(lobbyEntryPrefab, lobbyScrollView.content);
				LobbyItem lobbyItem = entry.GetComponent<LobbyItem>();

                lobbyItem.lobby = lobby;
                lobbyItem.lobbyNameText.text = lobby.Name; 
                lobbyItem.playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
                lobby.Data.TryGetValue("JoinCode", out var joinCodeData);

                if(joinCodeData != null && !joinCodeData.Equals(""))
                    lobbyItem.joinCode = joinCodeData.Value;
                else
                    Debug.Log("Join code not found in lobby data.");

			}

            await Task.Delay(5000); // Refresh every 5 seconds
        }
	}
}
