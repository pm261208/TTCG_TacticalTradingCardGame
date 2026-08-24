using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DeckVisual : MonoBehaviour{
    
    public int deckId;
    [SerializeField] private GameObject deckSpot;
    private float cardSpacing = 0.02f;
    private Vector3 cardUpsideDown = new Vector3(-90,0,0);
    private Vector3 cardUpsideUp = new Vector3(90,0,0);

    private void Awake() {
        CardGameManager.Instance.OnMatchStart += StateManager_OnMatchStart;
    }

    private void StateManager_OnMatchStart(object sender, System.EventArgs e) {
        UpdateCardsPosition();
    }

    public IEnumerator WaitForUpdateCardsPosition(float s) {
        yield return new WaitForSeconds(s);
        UpdateCardsPosition();
    }

    public void UpdateCardsPosition() {
        if (deckId == 1) {
            foreach (Card card in CardGameManager.Instance.deck1) {
                int index = CardGameManager.Instance.deck1.IndexOf(card);
                Vector3 cardPosition = new Vector3(0, index * cardSpacing, 0);
                card.transform.parent = deckSpot.transform;
                card.transform.localRotation = Quaternion.Euler(cardUpsideDown);
                card.transform.localPosition = cardPosition;
            }

        } else {
            foreach (Card card in CardGameManager.Instance.deck2) {
                int index = CardGameManager.Instance.deck2.IndexOf(card);
                Vector3 cardPosition = new Vector3(0, index * cardSpacing, 0);
                card.transform.parent = deckSpot.transform;
                card.transform.localRotation = Quaternion.Euler(cardUpsideDown);
                card.transform.localPosition = cardPosition;
            }
        }
    }

}
