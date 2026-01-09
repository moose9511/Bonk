using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Core;

public class LobbyManager : Singleton<LobbyManager>
{
    private Lobby _lobby;

    public async Task<bool> CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, Dictionary<string, string> data)
    {
        if(UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if(!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
        var player = new Unity.Services.Lobbies.Models.Player(
            AuthenticationService.Instance.PlayerId, 
            null, 
            playerData
        );

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,

            Player = player,
        };
    	_lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
        
        Debug.Log($"Lobby created: {_lobby.Name} with ID: {_lobby.Id}");
		return true;
    }

    private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
    {
        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();
        foreach (var (key, value) in data)
        {
            playerData.Add(key, new PlayerDataObject(
                visibility: PlayerDataObject.VisibilityOptions.Member,
                value: value));
        }

        return playerData;
    }

    public void OnApplicationQuit()
    {
        if(_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
		}
	}

}
