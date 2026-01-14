using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : Singleton<LobbyManager>
{
    public Lobby _lobby;
    private Coroutine _heartbeatCoroutine;
    private Coroutine _refreshLobbyCoroutine;

	[SerializeField] public ScrollRect lobbyScrollView;
	public async Task<bool> CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, Dictionary<string, string> data)
    {
        var options = new InitializationOptions();
        options.SetEnvironmentName("production");

        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
        var player = new Unity.Services.Lobbies.Models.Player(
            AuthenticationService.Instance.PlayerId,
            null,
            playerData
        );

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
        {
            IsPrivate = isPrivate,

            Player = player,
        };

        try
        {
            _lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, lobbyOptions);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create lobby: {e}");
            return false;
        }

        Debug.Log($"Lobby created: {_lobby.Name} with ID: {_lobby.Id}");

        _heartbeatCoroutine = StartCoroutine(LobbyHeartbeat(_lobby.Id, 6f));
        _refreshLobbyCoroutine = StartCoroutine(RefreshLobby(_lobby.Id, 1f));

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
        if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
        }
    }

    private IEnumerator LobbyHeartbeat(object id, float interval)
    {
        while (true)
        {
            Debug.Log("Sending lobby heartbeat ping...");
            LobbyService.Instance.SendHeartbeatPingAsync((string)id);
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator RefreshLobby(object id, float interval)
    {
        while (true)
        {
            Task<Lobby> task = LobbyService.Instance.GetLobbyAsync((string)id);
            yield return new WaitUntil(() => task.IsCompleted);
            Lobby newLobby = task.Result;
            if (newLobby.LastUpdated > _lobby.LastUpdated)
            {
                _lobby = newLobby;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(_lobby.MaxPlayers);

            string joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
				allocation.AllocationIdBytes,
                allocation.Key,
				allocation.ConnectionData
				);

			NetworkManager.Singleton.StartHost();
			NetworkManager.Singleton.SceneManager.LoadScene("WaitingRoom", LoadSceneMode.Single);

			Debug.Log($"Relay created. Join code: {joinCode}");
		}
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to create relay: {e}");
		}
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
			JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
				);

            NetworkManager.Singleton.StartClient();

		}
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to join relay: {e}");
		}
    }
}