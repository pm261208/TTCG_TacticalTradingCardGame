using System;
using UnityEngine;
using UnityEngine.UI;

public class BootMenuUI : MonoBehaviour {

    [SerializeField] private Button createServerButton;
    [SerializeField] private Button createClientButton;
    [SerializeField] private Button lookForMatchButton;
    [SerializeField] private MatchmakingClient matchmakingClient;

    private void Awake() {
        createServerButton.onClick.AddListener(CreateLobbyButton);
        createClientButton.onClick.AddListener(CreateClientButton);
        lookForMatchButton.onClick.AddListener(() => {
            matchmakingClient.RequestMatchmaking();
            //CardGameLobby.Instance.QuickJoin();
        });
    }

    private async void CreateClientButton() {
        await CardGameLobby.Instance.InitializeUnityAuthenticationAsync();
        CardGameLobby.Instance.QuickJoin();
    }
    private async void CreateLobbyButton() {
        await CardGameLobby.Instance.InitializeUnityAuthenticationAsync();
        string lobbyName = "LobbyName" + UnityEngine.Random.Range(100, 1000);
        CardGameLobby.Instance.CreateLobby(lobbyName, false);
    }


}
