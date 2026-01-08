using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class OpenLobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject createLobbyPanel;
	[SerializeField] private GameObject serverListPanel;
	[SerializeField] private TextField serverNameField;

    [SerializeField] private UnityEngine.UI.Button openLobbyButton;
	[SerializeField] private UnityEngine.UI.Button lobbyBackButton;
	[SerializeField] private UnityEngine.UI.Button createLobbyButton;

	private void Start()
    {
        openLobbyButton.onClick.AddListener(OnOpenLobbyButtonClicked);
		lobbyBackButton.onClick.AddListener(OnBackLobbyButtonClicked);
		createLobbyButton.onClick.AddListener(OnCreateLobbyButtonClicked);
	}

	private void OnOpenLobbyButtonClicked()
	{
		createLobbyPanel.SetActive(true);
		serverListPanel.SetActive(false);
	}
	private void OnBackLobbyButtonClicked() 
	{
		createLobbyPanel.SetActive(false);
		serverListPanel.SetActive(true);
	}
	
	private void OnCreateLobbyButtonClicked()
	{
		GameLobbyManager.Instance.CreateLobby(serverNameField.value, 16);
	}


}
