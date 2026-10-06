using System;
using UnityEngine;
using UnityEngine.UI;

public class BootMenuUI : MonoBehaviour {

    [SerializeField] private Button createServerButton;
    [SerializeField] private Button createClientButton;

    private void Awake() {
        createServerButton.onClick.AddListener(CreateLobbyButton);
        createClientButton.onClick.AddListener(CreateClientButton);
    }

    private async void CreateClientButton() {
        await CardGameLobby.Instance.InitializeUnityAuthenticationAsync();
        CardGameLobby.Instance.QuickJoin();
    }
    private async void CreateLobbyButton() {
        await CardGameLobby.Instance.InitializeUnityAuthenticationAsync();
        string lobbyName = "LobbyName" + UnityEngine.Random.Range(100, 1000);
        await CardGameLobby.Instance.CreateLobby(lobbyName, false);
    }


}
