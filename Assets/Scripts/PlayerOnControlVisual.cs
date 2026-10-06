using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PlayerOnControlVisual : Singleton<PlayerOnControlVisual> {

    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI isWaitingForResponseText;


    private void Start() {
        CardGameManager.Instance.OnTurnChage += StateManager_OnTurnChage;
        CardGameManager.Instance.OnMatchStart += StateManager_OnMatchStart;
        CardGameManager.Instance.OnWaintingForResponse += CardGameManager_OnWaintingForResponse;
        isWaitingForResponseText.gameObject.SetActive(false);
    }

    private void CardGameManager_OnWaintingForResponse(object sender, EventArgs e) {
        StartCoroutine(WaitingForResponse());
    }

    private void StateManager_OnTurnChage(object sender, EventArgs e) {
        PlayerOnControl();
    }

    private void StateManager_OnMatchStart(object sender, System.EventArgs e) {
        PlayerOnControl();
    }

    public void PlayerOnControl() {
        if(CardGameManager.Instance.localPlayer == CardGameManager.Instance.turnPlayer) {
            playerText.text = "PlayerTurn";
            playerText.color = Color.blue;

        } else {
            playerText.text = "OpponentTurn";
            playerText.color = Color.red;
        }

    }

    public IEnumerator WaitingForResponse() {
        isWaitingForResponseText.gameObject.SetActive(true);
        Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

        yield return new WaitUntil(() => task.IsCompleted);
        isWaitingForResponseText.gameObject.SetActive(false);
    }
}
