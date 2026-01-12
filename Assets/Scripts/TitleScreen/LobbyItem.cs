using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Core;
using System.Collections;
using Unity.Services.Core.Environments;
using TMPro;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    public Lobby lobby;

	bool isJoining = false;

	[SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button joinBtn;

	public async void JoinAsync(Lobby lobby)
	{
		if (isJoining)
			return;
		isJoining = true;

		try
		{
			Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);

			Debug.Log($"Joined lobby: {joinedLobby.Name} with ID: {joinedLobby.Id}");
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Failed to join lobby: {e}");
		}

		isJoining = false;
	}
}
