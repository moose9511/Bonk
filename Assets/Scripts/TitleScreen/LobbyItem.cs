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
using Unity.Netcode;

public class LobbyItem : MonoBehaviour
{
    public Lobby lobby;

	bool isJoining = false;

	[SerializeField] public TextMeshProUGUI lobbyNameText;
    [SerializeField] public TextMeshProUGUI playerCountText;

	public string joinCode;

	public async void JoinAsync()
	{
		if (isJoining)
			return;
		isJoining = true;

		try
		{
			Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
			bool worked = await LobbyManager.Instance.JoinRelay(joinCode);

			if (!worked) { 
				Debug.LogError("Failed to join relay after joining lobby.");
				isJoining = false;
                return;
            }

			Debug.Log($"Joined lobby: {joinedLobby.Name} with ID: {joinedLobby.Id}");
		}
		catch (System.Exception e)
		{
			Debug.LogError($"Failed to join lobby: {e}");
		}

		isJoining = false;
	}
}
