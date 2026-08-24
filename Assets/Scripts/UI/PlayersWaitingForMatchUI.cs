using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayersWaitingForMatchUI : MonoBehaviour{

    [SerializeField] private Transform playersWaintingContainer;
    [SerializeField] private Transform playerTemplate;


    private void Awake() {
        playerTemplate.gameObject.SetActive(false);
    }

    private void Start() {
        CardGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += CardGameMultiplayer_OnPlayerDataListChanged;
    }

    private void CardGameMultiplayer_OnPlayerDataListChanged(object sender, System.EventArgs e) {
        UpdatePlayerList();
    }

    private void UpdatePlayerList() {
        foreach(Transform child in playersWaintingContainer) {
            if (child == playerTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < CardGameMultiplayer.Instance.GetPlayerAmount(); i++) {
            Transform playerTransform = Instantiate(playerTemplate);
            PlayerData playerData = CardGameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(i);
            playerTransform.gameObject.SetActive(true);
            playerTransform.SetParent(playersWaintingContainer);
            playerTransform.GetComponent<PlayerTemplateUI>().SetPlayerName(playerData.playerName.ToString());

            if (playerData.playerName == CardGameMultiplayer.Instance.GetPlayerData().playerName) {
               playerTransform.GetComponent<PlayerTemplateUI>().SetColor(43, 43, 255);
            }
        }
    }
}
