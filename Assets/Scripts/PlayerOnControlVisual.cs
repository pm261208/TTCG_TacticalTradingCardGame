using TMPro;
using UnityEngine;

public class PlayerOnControlVisual : Singleton<PlayerOnControlVisual> {

    [SerializeField] private TextMeshProUGUI playerText;


    private void Start() {
        //CardGameManager.Instance.OnTurnChage += StateManager_OnTurnChage;
        CardGameManager.Instance.OnMatchStart += StateManager_OnMatchStart;
    }

    private void StateManager_OnMatchStart(object sender, System.EventArgs e) {
        PlayerOnControl();
    }

    public void PlayerOnControl() {;
        if(CardGameManager.Instance.localPlayer == CardGameManager.Instance.player1) {
            playerText.text = "Player1";
            playerText.color = Color.blue;

        } else {
            playerText.text = "Player2";
            playerText.color = Color.red;
        }


    }
}
