using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class OpenLobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject createLobbyPanel;
	[SerializeField] private GameObject serverListPanel;

	[SerializeField] private TMP_InputField serverNameField;
	[SerializeField] private TMP_InputField maxPlayerField;

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
	
	private async void OnCreateLobbyButtonClicked()
	{
		Int32.TryParse(maxPlayerField.text, out int j);
		if(j < 2 || j > 32)
			j = 16;

		bool success = await GameLobbyManager.Instance.CreateLobby(serverNameField.text, j);

		//if(success)
		//{
		//	SceneManager.LoadScene("map1");
		//}
	}


}
