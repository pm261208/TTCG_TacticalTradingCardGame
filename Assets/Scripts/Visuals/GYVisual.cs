using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class GYVisual : MonoBehaviour{

    public int player;
    [SerializeField] private GameObject gySpot;
    private float cardSpacing = 0.02f;
    private Vector3 cardUpsideDown = new Vector3(-90, 0, 0);
    private Vector3 cardUpsideUp = new Vector3(90, 0, 0);

    public void UpdateCardPositions() {
        float duration = 0.25f;

        if (player == 1) {
            foreach (Card card in CardGameManager.Instance.gy1) {
                int index = CardGameManager.Instance.gy1.IndexOf(card);
                Vector3 cardPosition = gySpot.transform.position + new Vector3(0, index * cardSpacing, 0);
                card.transform.parent = gySpot.transform;
                card.transform.localRotation = Quaternion.Euler(cardUpsideUp);
                card.transform.DOMove(cardPosition, duration);
            }
        } else {
            foreach (Card card in CardGameManager.Instance.gy2) {
                int index = CardGameManager.Instance.gy2.IndexOf(card);
                Vector3 cardPosition = gySpot.transform.position + new Vector3(0, index * cardSpacing, 0);
                card.transform.parent = gySpot.transform;
                card.transform.localRotation = Quaternion.Euler(cardUpsideUp);
                card.transform.DOMove(cardPosition, duration);
            }
        }
    }
    public IEnumerator WaitForUpdateCardsPosition(float s) {
        yield return new WaitForSeconds(s);
        UpdateCardPositions();
    }

    public void ShowGYCard(Card card) {

        float duration = 0.25f;

        Vector3 position = gySpot.transform.position + new Vector3(0, 0.02f * (transform.childCount+1), 0);
        card.transform.DOMove(position, duration);
        card.transform.DORotate(new Vector3(90, 0, 0), duration);
    }
}
