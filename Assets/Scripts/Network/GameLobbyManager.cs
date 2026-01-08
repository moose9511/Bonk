using UnityEngine;

public class GameLobbyManager : Singleton<GameLobbyManager>
{

	public void CreateLobby(string lobbyName, int maxPlayers)
	{
		LobbyManager.Instance.CreateLobby(lobbyName, maxPlayers, false, null);
	}
}
