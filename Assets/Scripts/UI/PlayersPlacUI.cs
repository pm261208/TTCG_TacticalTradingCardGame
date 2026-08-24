using System;
using TMPro;
using UnityEngine;

public class PlayersPlacUI : MonoBehaviour{

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI opponentNameText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI opponentManaText;
    [SerializeField] private TextMeshProUGUI lpText;
    [SerializeField] private TextMeshProUGUI opponentLpText;

    private void Start() {
        CardGameManager.Instance.OnMatchStart += CardGameManager_OnMatchStart;
        CardGameManager.Instance.OnManaChanged += CardGameManager_OnManaChaged;
        CardGameManager.Instance.OnLifePointsChanged += CardGameManager_OnLifePointsChanged;
    }

    private void CardGameManager_OnMatchStart(object sender, EventArgs e) {
        if (CardGameManager.Instance.localPlayer == CardGameManager.Instance.player1) {
            playerNameText.text = CardGameManager.Instance.player1.playerName;
            opponentNameText.text = CardGameManager.Instance.player2.playerName;

        } else {
            playerNameText.text = CardGameManager.Instance.player2.playerName;
            opponentNameText.text = CardGameManager.Instance.player1.playerName;

        }
        UpdateMana();
        UpdateLifePoints();
    }

    private void CardGameManager_OnLifePointsChanged(object sender, EventArgs e) {
        UpdateLifePoints();
    }

    private void CardGameManager_OnManaChaged(object sender, System.EventArgs e) {
        UpdateMana();
    }

    public void UpdateMana() {
        if (CardGameManager.Instance.localPlayer == CardGameManager.Instance.player1) {
            manaText.text = CardGameManager.Instance.player1.starMana.ToString();
            opponentManaText.text = CardGameManager.Instance.player2.starMana.ToString();
        } else {
            manaText.text = CardGameManager.Instance.player2.starMana.ToString();
            opponentManaText.text = CardGameManager.Instance.player1.starMana.ToString();
        }
    }
    public void UpdateLifePoints() {
        if (CardGameManager.Instance.localPlayer == CardGameManager.Instance.player1) {
            lpText.text = CardGameManager.Instance.player1.lp.ToString();
            opponentLpText.text = CardGameManager.Instance.player2.lp.ToString();
        } else {
            lpText.text = CardGameManager.Instance.player2.lp.ToString();
            opponentLpText.text = CardGameManager.Instance.player1.lp.ToString();
        }
    }
}
