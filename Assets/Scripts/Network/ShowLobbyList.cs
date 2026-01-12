using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class ShowLobbyList : MonoBehaviour
{
    [SerializeField] public ScrollRect lobbyScrollView;

    [SerializeField] private GameObject lobbyEntryPrefab;
    [SerializeField] private TextMeshProUGUI lobbyName;
    [SerializeField] private TextMeshProUGUI playerCount;
    [SerializeField] private Button joinButton;

	QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
    {
        Count = 25,
        Filters = new System.Collections.Generic.List<QueryFilter>()
    };
	public async void Start()
    {
        var options = new InitializationOptions();
		options.SetEnvironmentName("production");

		await InitializeUGS();
		ShowLobbies();
    }
    private async Task InitializeUGS()
    {
        try
        {
            await UnityServices.InitializeAsync();

            await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
        catch (System.Exception e)
        { 
            Debug.LogError($"Failed to initialize Unity Gaming Services: {e}");
        }
    }
    public async void ShowLobbies()
    {
		while (Application.isPlaying) {
			QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);

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
                GameObject entry = Instantiate(lobbyEntryPrefab, lobbyScrollView.content);
				TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();

                texts[0].text = $"({lobby.Players.Count}/{lobby.MaxPlayers})";
                texts[1].text = lobby.Name;

				Debug.Log($"Lobby Name: {lobby.Name}, Lobby ID: {lobby.Id}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}");
			}

            await Task.Delay(5000); // Refresh every 5 seconds
        }
	}
}
