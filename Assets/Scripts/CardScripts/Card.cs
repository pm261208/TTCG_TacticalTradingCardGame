using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static InputSystem;
using static UnityEngine.UI.GridLayoutGroup;

public class Card : GameObjectBase {

    [SerializeField] private CardSO card;
    [SerializeField] private GameObject cardOptions;
    [SerializeField] private CardInteractionButton targetButton;
    public int cardId;
    public string cardName;
    public string cardDescription;
    public CardType cardType;
    public CardData cardData;
    public List<int> activatedEventsInstance = new();
    public Player Owner;


    private void Start() {
        if (NetworkManager.Singleton.IsServer) {
            EventSystem.Instance.OnProcessTriger += EventSystem_OnProcessTriger;
        }
    }


    private void EventSystem_OnProcessTriger(object sender, EventSystem.OnProcessTrigerEventArgs e) {
        ProcessTrigger(e.trigger, e.ctx);
    }

    public void Define(CardSO cardso) {
        card = cardso;
    }

    public void SetupCard() {

        cardName = card.cardName;
        cardDescription = card.description;
        switch (card.cardType) {
            case CardType.Monster:
                SetupMonsterCard();
                break;
            case CardType.Spell:
                SetupSpellCard();
                break;
            case CardType.Trap:
                SetupTrapCard();
                break;
        }

        cardOptions.transform.GetChild(0).gameObject.SetActive(false);
    }
    public void SetupMonsterCard() {
        MonsterCardSO monsterCard = (MonsterCardSO)card;
        MonsterCardData monsterCardData = new MonsterCardData() {
            cardStarLevel = monsterCard.starLevel,
            cardHp = monsterCard.hp,
            cardAtk = monsterCard.atk,
            movequant = 0,
            atkquant = 0,
        };
        cardData = monsterCardData;
        cardType = monsterCard.cardType;
    }
    public void SetupSpellCard() {
        SpellCardSO spellCard = (SpellCardSO)card;
        cardType = spellCard.cardType;

    }
    public void SetupTrapCard() {
        TrapCardSO trapCard = (TrapCardSO)card;
        cardType = trapCard.cardType;

    }

    public List<CardEvent> GetCardEvents() {
        return card.events;
    }

    private void ProcessTrigger(TriggerType trigger, EffectContext ctx) {
        foreach (var evt in card.events) {
            if (evt.trigger != trigger || evt.trigger == TriggerType.NoTrigger)
                continue;

            bool valid = true;

            foreach (var condition in evt.conditions) {
                if (!condition.Evaluate(this, ctx)) {
                    valid = false;
                    break;
                }
            }

            if (!valid)
                continue;

            ctx.Owner = Owner.id;
            if (evt.effectType == EffectTypes.doesNotStartChain) { 
                StartCoroutine(evt.effects.Execute(ctx));

            }else if (evt.effectType == EffectTypes.ignition) {
                StartCoroutine(ChainSystem.Instance.ActivateIgnition(this, evt, new() { Source = cardId, Owner = Owner.id, eventData = ctx.eventData }, Owner));

            }else if (evt.effectType == EffectTypes.ignitianResponse) {
                if (ChainSystem.Instance.buildingChain) {
                    ChainSystem.Instance.RegisterPendingResponse(
                        new PendingEffect {
                            source = this,
                            context = new() { Source = cardId, Owner = Owner.id ,eventData = ctx.eventData },
                            cardEvent = evt,
                            owner = Owner
                        }
                    );
                } else {
                    StartCoroutine(ChainSystem.Instance.ActivateIgnition(this, evt, ctx, Owner));
                }
            }else if (evt.effectType == EffectTypes.response) {
                if (ChainSystem.Instance.buildingChain) {
                    ChainSystem.Instance.RegisterPendingResponse(
                        new PendingEffect {
                            source = this,
                            context = new() { Source = cardId, Owner = Owner.id, eventData = ctx.eventData },
                            cardEvent = evt,
                            owner = Owner
                        }
                    );
                }

            } else if (evt.effectType == EffectTypes.trigger) {
                ChainSystem.Instance.RegisterPendingEffect(
                    new PendingEffect {
                        source = this,
                        context = new() { Source = cardId, Owner = Owner.id, eventData = ctx.eventData },
                        cardEvent = evt,
                        owner = Owner
                    }
                );
            }
            
            
        }
    }

    public void TrySelectCard() {
        if (CardGameManager.Instance.localPlayer == CardGameManager.Instance.turnPlayer && Owner == CardGameManager.Instance.localPlayer) {
            EffectContext context = new(){ Source = cardId, Owner = Owner.id };

            SelectCard(context);
        }
    }

    private void SelectCard(EffectContext context) {
        switch (card.cardType) {
            case CardType.Monster:
                SelectMonsterCard(context);
                break;
            case CardType.Spell:
                SelectSpellCard(context);
                break;
            case CardType.Trap:
                SelectTrapCard(context);
                break;
        }
    }

    private void SelectMonsterCard(EffectContext context) {
        List<int> interactionIndexs = new();

        context.Owner = Owner.id;
        foreach (CardEvent cardEvent in card.events) {
            bool isAble = true;
            foreach (EventCondition condition in cardEvent.conditions) {
                if (!condition.Evaluate(this, context)) {
                    isAble = false; break;
                }
            }
            if (!isAble) continue;

            // define as açoes
            interactionIndexs.Add(card.events.IndexOf(cardEvent));
        }
        if (CardGameManager.Instance.IsCardInHand(this)) {
            EffectNode selectCardEffectNode = new SelectCardEffectNode {
                eventIndexs = interactionIndexs,
                cardSubject = "Source",
            };
            StartCoroutine(selectCardEffectNode.Execute(context));

        } else if (CardGameManager.Instance.IsCardInField(this)) {
            EffectNode selectCardEffectNode = new SelectMoveAtkEffectNode {
                eventIndexs = interactionIndexs,
                cardSubject = "Source",
            };
            StartCoroutine(selectCardEffectNode.Execute(context));
        }
    }
    private void SelectSpellCard(EffectContext context) {
        List<int> interactionIndexs = new();

        context.Owner = Owner.id;
        foreach (CardEvent cardEvent in card.events) {
            bool isAble = true;
            foreach (EventCondition condition in cardEvent.conditions) {
                if (!condition.Evaluate(this, context)) {
                    isAble = false; break;
                }
            }
            if (!isAble) continue;

            // define as açoes
            interactionIndexs.Add(card.events.IndexOf(cardEvent));
        }
        
        EffectNode selectCardEffectNode = new SelectCardEffectNode {
            eventIndexs = interactionIndexs,
            cardSubject = "Source",
        };
        StartCoroutine(selectCardEffectNode.Execute(context));
        
    }
    private void SelectTrapCard(EffectContext context) {

    }

    public int AvailableEvents() {
        int eventCount = 0;
        foreach (CardEvent cardEvent in card.events) {
            bool isAble = true;
            foreach (EventCondition condition in cardEvent.conditions) {
                
                isAble = EvaluateEvent(cardEvent); 
                if(!isAble) break;
            }
            if (!isAble) continue;
            if (cardEvent.effectType == EffectTypes.ignition && CardGameManager.Instance.turnPlayer != Owner) continue;

            eventCount++;
        }
        return eventCount;
    }

    public bool EvaluateEvent(CardEvent cardEvent) {
        bool isAble = true;
        foreach (EventCondition condition in cardEvent.conditions) {
            if (!condition.Evaluate(this, new EffectContext { Source = cardId, Owner = Owner.id })) {
                isAble = false; break;
            }
        }
        return isAble;
    }

    public int AvailableNormalEvents() {
        int eventCount = 0;
        if (cardType == CardType.Monster) {
            if(CardGameManager.Instance.IsCardInHand(this) && ((MonsterCardData)cardData).cardStarLevel <= CardGameManager.Instance.GetPlayerFromId(Owner.id).starMana && CardGameManager.Instance.turnPlayer == Owner)
                { eventCount++; }
            if(((MonsterCardData)cardData).movequant > 0 && CardGameManager.Instance.IsCardInField(this) && CardGameManager.Instance.turnPlayer == Owner)
                { eventCount++; }
            if (((MonsterCardData)cardData).atkquant > 0 && CardGameManager.Instance.IsThereOponentCardInRange(GetAtkRange())) 
                { eventCount++; }
            if(((MonsterCardData)cardData).atkquant > 0 && GetAtkRange().Contains(99))
                { eventCount++; }
        }
        if (cardType == CardType.Spell) {
            if(CardGameManager.Instance.IsCardInHand(this) && CardGameManager.Instance.turnPlayer == Owner)
                { eventCount++; }

        }
        if (cardType == CardType.Trap) {
            
        }
        return eventCount;
    }

    public void ShowInteractions(List<int> interactions) {
        GameObject button;
        CardInteractionButton newbutton;

        if (CardGameManager.Instance.IsCardInHand(this) && card.cardType == CardType.Monster) {
            button = Instantiate(cardOptions.transform.GetChild(0).gameObject);
            button.transform.SetParent(cardOptions.transform);
            button.transform.localScale = new Vector3(1, 1, 1);
            button.transform.localPosition = Vector3.zero;
            button.transform.localRotation = Quaternion.Euler(Vector3.zero);

            button.GetComponent<Image>().color = new Color32(100, 255, 255, 255);

            if (EvaluateEvent(CardGameManager.Instance.normalSummonCardEvent)) {
                newbutton = button.GetComponent<CardInteractionButton>();
                button.gameObject.SetActive(true);
                newbutton.DefineAction(CardGameManager.Instance.normalSummonCardEvent);
            }
            
        }

        foreach (int interaction in interactions){
            button = Instantiate(cardOptions.transform.GetChild(0).gameObject);
            button.transform.SetParent(cardOptions.transform);
            button.transform.localScale = new Vector3(1, 1, 1);
            button.transform.localPosition = Vector3.zero;
            button.transform.localRotation = Quaternion.Euler(Vector3.zero);
            
            newbutton = button.GetComponent<CardInteractionButton>();
            button.gameObject.SetActive(true);
            newbutton.DefineAction(card.events[interaction]);
        }
    }

    public void HideInteractions() {
        foreach (Transform button in cardOptions.transform) {
            if (button.gameObject.activeSelf) {
                Destroy(button.gameObject);
            }
        }
    }

    public void ShowTarget(CardEvent cardEvent) {
        targetButton.DefineAction(cardEvent);
        if (cardEvent == CardGameManager.Instance.atkCardEvent) {
            Debug.Log(cardEvent);
            targetButton.GetComponent<Image>().color = new Color32(255, 35, 35, 255);
        } else {
            targetButton.GetComponent<Image>().color = new Color32(255, 255, 100, 255);
        }
        targetButton.gameObject.SetActive(true);
    }
    public void HideTarget() {
        targetButton.gameObject.SetActive(false);
    }

    public List<int> GetMoveRange() {
        List<int> moveRange = new();

        Tile tile = CardGameManager.Instance.GetTileWCard(this);
        foreach (int n in ((MonsterCardSO)card).moveRange) {
            moveRange.Add(tile.tileId + n);
        }
        return moveRange;
    }
    public List<int> GetAtkRange() {
        List<int> atkRange = new();

        Tile tile = CardGameManager.Instance.GetTileWCard(this);
        foreach (int n in ((MonsterCardSO)card).atkRange) {
            int tileId = tile.tileId + n;
            atkRange.Add(tileId);
            if (Owner == CardGameManager.Instance.player1) {
                if (tileId == 16 || tileId == 26 || tileId == 36 || tileId == 46 || tileId == 56) {
                    atkRange.Add(99);
                }
            }else {
                if (tileId == 10 || tileId == 20 || tileId == 30 || tileId == 40 || tileId == 50) {
                    atkRange.Add(99);
                }
            }
        }
        return atkRange;
    }

    public CardSO GetCardSO() {
        return card;
    }
}
