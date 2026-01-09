using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UIElements;

public class ShowLobbyList : MonoBehaviour
{
    [SerializeField] private ScrollView lobbyScrollView;
	public async void ShowLobbies()
    {
        while(Application.isPlaying) {
            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync();

            foreach (Lobby lobby in lobbies.Results)
            {
                lobbyScrollView.contentContainer.Add(new Label($"Lobby Name: {lobby.Name}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}"));
				Debug.Log($"Lobby Name: {lobby.Name}, Lobby ID: {lobby.Id}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}");
			}
		}
	}
}
