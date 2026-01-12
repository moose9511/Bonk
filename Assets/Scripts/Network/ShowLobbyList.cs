using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class ShowLobbyList : MonoBehaviour
{
    [SerializeField] public ScrollRect lobbyScrollView;
    [SerializeField] private GameObject lobbyEntryPrefab;

    public async void Start()
    {
        await InitializeUGS();
        ShowLobbies();
    }
    private async Task InitializeUGS()
    {
        try
        {
            await UnityServices.InitializeAsync();

            await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (System.Exception e)
        { 
            Debug.LogError($"Failed to initialize Unity Gaming Services: {e}");
        }
    }
    public async void ShowLobbies()
    {
        while (Application.isPlaying) {
            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync();
            if (lobbies != null)
            {
                Debug.Log($"Found {lobbies.Results.Count} lobbies.");
            } 

            foreach (Transform child in lobbyScrollView.content)
                {
                    Destroy(child.gameObject);
                }

            foreach (Lobby lobby in lobbies.Results)
            {
                Instantiate(lobbyEntryPrefab, lobbyScrollView.content);
                Debug.Log($"Lobby Name: {lobby.Name}, Lobby ID: {lobby.Id}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}");
			}

            await Task.Delay(5000); // Refresh every 5 seconds
        }
	}
}
