using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour{

    [SerializeField] private Button lookForMatchButton;
    [SerializeField] private Button DecksButton;
    [SerializeField] private TMP_InputField playerInputField;
    //[SerializeField] private MatchmakingClient matchmakingClient;
    [SerializeField] private GameObject waitingForMatchBlocker;

    private void Start() {
        playerInputField.text = PlayerPrefs.GetString(CardGameMultiplayer.PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER);
        waitingForMatchBlocker.SetActive(false);

        playerInputField.onEndEdit.AddListener(UpdateName);

        lookForMatchButton.onClick.AddListener(() => { 
            waitingForMatchBlocker.SetActive(true);
            Loader.Load(Loader.Scene.TestingConnectionScene);
        });

        DecksButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.DeckSelectScene);
        });
    }

    private void UpdateName(string value) {
        if (playerInputField.text.Length <= 3) {
            playerInputField.text = PlayerPrefs.GetString(CardGameMultiplayer.PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER);
        } else {
            PlayerPrefs.SetString(CardGameMultiplayer.PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerInputField.text);
        }
    }


}
