using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class CardHoverSystem : MonoBehaviour{

    [SerializeField] private GameObject visual;
    [SerializeField] private new TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI starLevel;
    [SerializeField] private TextMeshProUGUI power;
    [SerializeField] private TextMeshProUGUI hp;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private GameObject starLevelContainer;
    [SerializeField] private GameObject atkContainer;
    [SerializeField] private GameObject hpContainer;
    [SerializeField] private CardVisual cardVisual;

    public static CardHoverSystem instance;

    private Card hoveredCard;
    private bool isCardHovered;
    private float timer = 0;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        visual.SetActive(false);
    }

    private void Update() {
        if (hoveredCard != null && !isCardHovered) {
            timer += Time.deltaTime;
            if (timer >= 5f) {
                Hide();
            }
        }
    }

    public void Show(Card card) {
        hoveredCard = card;

        visual.SetActive(true);
        name.text = card.cardName;
        description.text = card.cardDescription;
        cardVisual.SetUp(card);
        switch (card.GetCardSO().cardType) {
            case CardType.Monster:
                ShowMonster();
                break;
            case CardType.Spell:
                ShowSpell();
                break;
            case CardType.Trap:
                ShowTrap();
                break;
        }

        timer = 0;
    }

    public void ShowMonster() {
        MonsterCardData monsterHoveredCardData = (MonsterCardData)hoveredCard.cardData;

        starLevel.text = monsterHoveredCardData.cardStarLevel.ToString();
        power.text = monsterHoveredCardData.cardAtk.ToString();
        hp.text = monsterHoveredCardData.cardHp.ToString();

        starLevelContainer.SetActive(true);
        atkContainer.SetActive(true);
        hpContainer.SetActive(true);
    }
    public void ShowSpell() {
        SpellCardData spellHoveredCardData = (SpellCardData)hoveredCard.cardData;

        starLevelContainer.SetActive(false);
        atkContainer.SetActive(false);
        hpContainer.SetActive(false);
    }
    public void ShowTrap() {
        TrapCardData trapHoveredCardData = (TrapCardData)hoveredCard.cardData;

        starLevelContainer.SetActive(false);
        atkContainer.SetActive(false);
        hpContainer.SetActive(false);
    }
    public void Hide() {
        hoveredCard = null;
        visual.SetActive(false);
    }

    public void PoiterOnCardEnter() {
        isCardHovered = true;
    }
    public void PoiterOnCardLeave() {
        isCardHovered = false;
    }

}
