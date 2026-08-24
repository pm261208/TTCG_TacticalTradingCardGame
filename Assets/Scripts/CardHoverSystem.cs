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
    [SerializeField] private TextMeshProUGUI cardName;
    [SerializeField] private TextMeshProUGUI cardStarLevel;
    [SerializeField] private TextMeshProUGUI cardPower;
    [SerializeField] private TextMeshProUGUI cardHp;
    [SerializeField] private TextMeshProUGUI cardDescription;
    [SerializeField] private Image cardBackground;
    [SerializeField] private GameObject cardStarLevelContainer;
    [SerializeField] private GameObject cardAtkContainer;
    [SerializeField] private GameObject cardHpContainer;
    [SerializeField] private GameObject starLevelContainer;
    [SerializeField] private GameObject atkContainer;
    [SerializeField] private GameObject hpContainer;

    public static CardHoverSystem instance;

    private Card hoveredCard;
    private bool isCardHovered;
    private float timer = 0;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        visual.gameObject.SetActive(false);
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

        visual.gameObject.SetActive(true);
        name.text = card.cardName;
        cardName.text = card.GetCardSO().cardName;
        description.text = card.cardDescription;
        cardDescription.text = card.GetCardSO().description;
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
        cardStarLevel.text = ((MonsterCardSO)hoveredCard.GetCardSO()).starLevel.ToString();
        power.text = monsterHoveredCardData.cardAtk.ToString();
        cardPower.text = ((MonsterCardSO)hoveredCard.GetCardSO()).atk.ToString();
        hp.text = monsterHoveredCardData.cardHp.ToString();
        cardHp.text = ((MonsterCardSO)hoveredCard.GetCardSO()).hp.ToString();

        starLevelContainer.SetActive(true);
        atkContainer.SetActive(true);
        hpContainer.SetActive(true);
        cardStarLevelContainer.SetActive(true);
        cardAtkContainer.SetActive(true);
        cardHpContainer.SetActive(true);

        cardBackground.color = new Color32(255, 210, 110, 255);
    }
    public void ShowSpell() {
        SpellCardData spellHoveredCardData = (SpellCardData)hoveredCard.cardData;

        starLevelContainer.SetActive(false);
        atkContainer.SetActive(false);
        hpContainer.SetActive(false);
        cardStarLevelContainer.SetActive(false);
        cardAtkContainer.SetActive(false);
        cardHpContainer.SetActive(false);

        cardBackground.color = new Color32(100, 220, 180, 255);
    }
    public void ShowTrap() {
        TrapCardData trapHoveredCardData = (TrapCardData)hoveredCard.cardData;

        starLevelContainer.SetActive(false);
        atkContainer.SetActive(false);
        hpContainer.SetActive(false);
        cardStarLevelContainer.SetActive(false);
        cardAtkContainer.SetActive(false);
        cardHpContainer.SetActive(false);

        cardBackground.color = new Color32(220, 110, 220, 255);
    }
    public void Hide() {
        hoveredCard = null;
        visual.gameObject.SetActive(false);
    }

    public void PoiterOnCardEnter() {
        isCardHovered = true;
    }
    public void PoiterOnCardLeave() {
        isCardHovered = false;
    }

}
