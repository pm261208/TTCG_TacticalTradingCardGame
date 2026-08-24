using TMPro;
using UnityEngine;

public class CardOptionButton : TempButton {

    public Card card;
    public GameObject SelectedVisual;
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI cardStarLevel;
    [SerializeField] private TextMeshProUGUI cardPower;
    [SerializeField] private TextMeshProUGUI cardHp;
    [SerializeField] private TextMeshProUGUI cardDescription;
    public override void Onclick() {
        InteractionSystem.Instance.ClickButton(this);
    }

    public void Setup() {
        cardName.text = card.cardName;
        cardStarLevel.text = ((MonsterCardData)card.cardData).cardStarLevel.ToString();
        cardPower.text = ((MonsterCardData)card.cardData).cardAtk.ToString();
        cardHp.text = ((MonsterCardData)card.cardData).cardHp.ToString();
        cardDescription.text = card.cardDescription;
        isClickable = true;
    }
}
