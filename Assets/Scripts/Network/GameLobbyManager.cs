using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;

public class GameLobbyManager : Singleton<GameLobbyManager>
{

	public async Task<bool> CreateLobby(string lobbyName, int maxPlayers)
	{

		Dictionary<string, string> playerData = new Dictionary<string, string>()
		{
			{ "JoinCode", null }
		};

		bool success = await LobbyManager.Instance.CreateLobby(lobbyName, maxPlayers, false, playerData);
		return success;
	}
}
