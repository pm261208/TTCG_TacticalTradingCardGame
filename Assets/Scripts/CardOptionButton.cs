using TMPro;
using UnityEngine;

public class CardOptionButton : TempButton {

    public Card card;
    public GameObject SelectedVisual;
    [SerializeField] private CardVisual cardVisual;

    public override void Onclick() {
        InteractionSystem.Instance.ClickButton(this);
    }

    public void Setup() {
        isClickable = true;
        cardVisual.SetUp(card);
    }
}
