using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public Card card;

    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI starLevelText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image background;
    [SerializeField] private Image border;

    [SerializeField] private GameObject starLevelContainer;
    [SerializeField] private GameObject atkContainer;
    [SerializeField] private GameObject hpContainer;



    public void SetUp(Card card) {
        this.card = card;
        ShowCard();
    }

    private void ShowCard() {
        cardNameText.text = card.cardName;
        descriptionText.text = card.cardDescription;

        switch (card.cardType) {
            case CardType.Monster:
                ShowMonsterCard();
                break;
            case CardType.Spell:
                ShowSpellCard();
                break;
            case CardType.Trap:
                ShowTrapCard();
                break;

        }
    }

    private void ShowMonsterCard() {
        MonsterCardData monsterCardData = (MonsterCardData)card.cardData;
        starLevelText.text = monsterCardData.cardStarLevel.ToString();
        hpText.text = monsterCardData.cardHp.ToString();
        atkText.text = monsterCardData.cardAtk.ToString();

        background.color = new Color32(255, 210, 110, 255);
    }
    private void ShowSpellCard() {
        background.color = new Color32(100, 220, 180, 255);
        starLevelContainer.SetActive(false);
        atkContainer.SetActive(false);
        hpContainer.SetActive(false);
    }
    private void ShowTrapCard() {
        background.color = new Color32(220, 110, 220, 255);
        starLevelContainer.SetActive(false);
        atkContainer.SetActive(false);
        hpContainer.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if(transform.localPosition.y == 0) {
            ObjectManager.Instance.handPlayer.GetComponent<HandVisual>().PulUpCard(GetComponent<Card>());
        }

        CardHoverSystem.instance.Show(card);
        CardHoverSystem.instance.PoiterOnCardEnter();
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (transform.localPosition.y <= 0.3f) {
            ObjectManager.Instance.handPlayer.GetComponent<HandVisual>().PulUpCard();
        }
        CardHoverSystem.instance.PoiterOnCardLeave();

    }

    public void UpdateBorder() {
        if (card.Owner != CardGameManager.Instance.localPlayer) {
            border.color = Color.red;
        } else if (card.AvailableEvents() > 0) {
            border.color = Color.yellow;
        } else if (card.AvailableNormalEvents() > 0) {
            border.color = Color.blue;
        } else {
            border.color = Color.black;
        }
    }

}
