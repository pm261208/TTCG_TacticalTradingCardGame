using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.GridLayoutGroup;

public class CardGameManager : MonoBehaviour{

    public static CardGameManager Instance {  get; private set; }

    public event EventHandler OnMatchStart;
    public event EventHandler OnPlayerSet;
    public event EventHandler OnTurnChage;
    public event EventHandler OnManaChanged;
    public event EventHandler OnLifePointsChanged;

    public Player localPlayer;
    public Player player1;
    public Player player2;
    public Player turnPlayer;
    public Tile[,] field = new Tile[5, 5];
    public List<Card> hand1;
    public List<Card> hand2;
    public List<Card> deck1;
    public List<Card> deck2;
    public List<Card> gy1;
    public List<Card> gy2;
    public DeckSO deck1SO;
    public DeckSO deck2SO;

    public List<EventLog> cardEventLogs = new();
    public int turnCount;


    public CardEvent normalSummonCardEvent;

    public CardEvent moveCardEvent = new() {
        effectType = EffectTypes.doesNotStartChain,
        effects = new MoveCardNode {
            
            cardSubject = "Source TargetTile",
        },
    };

    public CardEvent atkCardEvent = new() {
        effectType = EffectTypes.doesNotStartChain,
        effects = new AttackCardNode {
            cardSubject = "",
        },
    };

    public CardEvent atkPlayerCardEvent = new() {
        effectType = EffectTypes.doesNotStartChain,
        effects = new AttackPlayerNode {
            cardSubject = "",
        },
    };

    public CardEvent setCardEvent = new() {
        effectType = EffectTypes.doesNotStartChain,
        effects = new SelectTileOnYourFieldNode {
            nextEffect = new SetCardNode {
                cardSubject = "Source TargetTile"
            }
        },
        conditions = new List<EventCondition> { new IsOnHandCondition() }
    };

    public CardEvent placeSpellTrapCardEvent = new() {
        effectType = EffectTypes.doesNotStartChain,
        effects = new SelectTileOnYourFieldNode {
            nextEffect = new PlaceSpellTrapCardOnFieldNode {
                cardSubject = "Source TargetTile",
                nextEffect = new ActivateCardEventFromDataNode { cardSubject = "eventIndex" }
            }
        },
    };

    [SerializeField] private GameObject Card;
    [SerializeField] private GameObject Tile;


    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        //TEMP
        normalSummonCardEvent = new() {
            effectType = EffectTypes.doesNotStartChain,
            effects = new SelectBacklineTileNode {
                nextEffect = new SummonCardNode {
                    nextEffect = new RemoveStarManaNormalSummonNode { },

                    cardSubject = "Source TargetTile",
                },
                cardSubject = "Source",
            },
            conditions = new List<EventCondition> { new IsCardLevelEqualOrLowerThanManaCondition(), new IsOnHandCondition() }
        };

        placeSpellTrapCardEvent = new() {
            effectType = EffectTypes.doesNotStartChain,
            effects = new SelectTileOnYourFieldNode {
                nextEffect = new PlaceSpellTrapCardOnFieldNode {
                    cardSubject = "Source TargetTile",
                    nextEffect = new ActivateCardEventFromDataNode { cardSubject = "eventIndex" }
                }
            },
        };

    }

    private void Start() {
        CardGameMultiplayer.Instance.OnLoadEventCompleted += CardGameMultiplayer_OnLoadEventCompleted;
    }

    private void CardGameMultiplayer_OnLoadEventCompleted(object sender, CardGameMultiplayer.OnLoadEventCompletedEventArgs e) {
        if (NetworkManager.Singleton.IsServer) {
            if (e.sceneName == Loader.Scene.DuelScene.ToString()) {
                deck1SO = player1.playerDeck;
                deck2SO = player2.playerDeck;
                SetMatchData();
            }
            if (e.sceneName == Loader.Scene.TestingConnectionScene.ToString()) {

            }
        }
    }



    private void SetMatchData() {
        foreach (CardSO cardSO in deck1SO.Deck) {
            GameObject cardTransform = Instantiate(Card);
            Card newCard = cardTransform.GetComponent<Card>();
            newCard.Define(cardSO);
            newCard.name = cardSO.name + (deck1.Count + deck2.Count);
            newCard.cardId = deck1.Count + deck2.Count + 1;
            newCard.instanceId = (deck1.Count + deck2.Count + 1)*100;
            newCard.Owner = player1;
            newCard.SetupCard();
            newCard.GetComponent<CardVisual>().SetUp(newCard);

            deck1.Add(newCard);
        }

        foreach (CardSO cardSO in deck2SO.Deck) {
            GameObject cardTransform = Instantiate(Card);
            Card newCard = cardTransform.GetComponent<Card>();
            newCard.Define(cardSO);
            newCard.name = cardSO.name + (deck1.Count + deck2.Count + 1);
            newCard.cardId = deck1.Count + deck2.Count + 1;
            newCard.instanceId = (deck1.Count + deck2.Count + 1) * 100;
            newCard.Owner = player2;
            newCard.SetupCard();
            newCard.GetComponent<CardVisual>().SetUp(newCard);

            deck2.Add(newCard);
        }

        for (int i = 0; i < 5; i++) {
            for(int j = 0; j < 5; j++) {
                GameObject tileTransform = Instantiate(Tile);
                tileTransform.name = "Tile " + ((i + 1) * 10 + j + 1);
                Tile newTile = tileTransform.GetComponent<Tile>();
                newTile.tileId = (i+1)*10+j+1;
                field[i,j] = newTile;
            }
        }

        bool sorteio = UnityEngine.Random.Range(0, 2) == 0;

        ShuffleDeck(deck1);
        ShuffleDeck(deck2);

        int player;
        int opponent;
        if (localPlayer == player1) {
            player = 1;
            opponent = 2;
        } else {
            player = 2;
            opponent = 1;
        }

        ObjectManager.Instance.deckPlayer.GetComponent<DeckVisual>().deckId = player;
        ObjectManager.Instance.deckOpponent.GetComponent<DeckVisual>().deckId = opponent;
        ObjectManager.Instance.handPlayer.GetComponent<HandVisual>().player = player;
        ObjectManager.Instance.handOpponent.GetComponent<HandVisual>().player = opponent;
        ObjectManager.Instance.gyPlayer.GetComponent<GYVisual>().player = player;
        ObjectManager.Instance.gyOpponent.GetComponent<GYVisual>().player = opponent;

        player1.lp = 8;
        player2.lp = 8;

        OnPlayerSet?.Invoke(this, EventArgs.Empty);
    }

    public void SetMatchDataClient() {
        if (NetworkManager.Singleton.LocalClientId == player1.id) {
            localPlayer = player1;
        } else {
            localPlayer = player2;
        }
        player1.lp = 8;
        player2.lp = 8;

        foreach (CardSO cardSO in deck1SO.Deck) {
            GameObject cardTransform = Instantiate(Card);
            Card newCard = cardTransform.GetComponent<Card>();
            newCard.Define(cardSO);
            newCard.name = cardSO.name + (deck1.Count + deck2.Count + 1);
            newCard.cardId = deck1.Count + deck2.Count + 1;
            newCard.instanceId = (deck1.Count + deck2.Count + 1) * 100;
            newCard.Owner = player1;
            newCard.SetupCard();
            newCard.GetComponent<CardVisual>().SetUp(newCard);

            deck1.Add(newCard);
        }

        foreach (CardSO cardSO in deck2SO.Deck) {
            GameObject cardTransform = Instantiate(Card);
            Card newCard = cardTransform.GetComponent<Card>();
            newCard.Define(cardSO);
            newCard.name = cardSO.name + (deck1.Count + deck2.Count + 1);
            newCard.cardId = deck1.Count + deck2.Count + 1;
            newCard.instanceId = (deck1.Count + deck2.Count + 1) * 100;
            newCard.Owner = player2;
            newCard.SetupCard();
            newCard.GetComponent<CardVisual>().SetUp(newCard);

            deck2.Add(newCard);
        }
 
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 5; j++) {
                GameObject tileTransform = Instantiate(Tile);
                tileTransform.name = "Tile " + ((i + 1) * 10 + j + 1);
                Tile newTile = tileTransform.GetComponent<Tile>();
                newTile.tileId = (i + 1) * 10 + j + 1;
                field[i, j] = newTile;
            }
        }

        int player;
        int opponent;
        if (localPlayer == player1) {
            player = 1;
            opponent = 2;
        } else {
            player = 2;
            opponent = 1;
        }

        ObjectManager.Instance.deckPlayer.GetComponent<DeckVisual>().deckId = player;
        ObjectManager.Instance.deckOpponent.GetComponent<DeckVisual>().deckId = opponent;
        ObjectManager.Instance.handPlayer.GetComponent<HandVisual>().player = player;
        ObjectManager.Instance.handOpponent.GetComponent<HandVisual>().player = opponent;
        ObjectManager.Instance.gyPlayer.GetComponent<GYVisual>().player = player;
        ObjectManager.Instance.gyOpponent.GetComponent<GYVisual>().player = opponent;

        CardGameMultiplayer.Instance.PlayerMatchDataSincServerRpc();
    }

    public void StartMatchT() {
        DrawCardGA initialdrawCardGA = new(5, player1);
        StartCoroutine(ActionSystem.Instance.Perform(initialdrawCardGA));
        DrawCardGA drawCardGA = new(5, player2);
        StartCoroutine(ActionSystem.Instance.Perform(drawCardGA));
        StartCoroutine(WaitForUpdateCardBorder(2f));

        Debug.Log("START MATCH");
        OnMatchStart?.Invoke(this, EventArgs.Empty);
        OnTurnChage?.Invoke(this, EventArgs.Empty);
    }

    public bool IsCardInDeck(Card card) {
        if (deck1.Contains(card)) {
            return true;
        }
        if (deck2.Contains(card)) {
            return true;
        }
        return false;
    }
    public bool IsCardInHand(Card card) {
        if (hand1.Contains(card)) {
            return true;
        }
        if (hand2.Contains(card)) {
            return true;
        }
        return false;
    }

    public bool IsCardInGy(Card card) {
        if (gy1.Contains(card)) {
            return true;
        }
        if (gy2.Contains(card)) {
            return true;
        }
        return false;
    }
    public bool IsCardInField(Card card) {

        for (int collum = 0; collum < 5; collum++) {
            for (int row = 0; row < 5; row++) {
                Tile tile = field[row, collum];
                if (tile.monsterOnTile == card) {
                    return true;
                }
                if (tile.spellTrapOnTile == card) {
                    return true;
                }
            }
        }
        return false;
    }

    public Tile GetTileWCard(Card card) {

        for (int collum = 0; collum < 5; collum++) {
            for (int row = 0; row < 5; row++) {
                Tile tile = field[row, collum];
                if (tile.monsterOnTile == card) {
                    return tile;
                }
                if (tile.spellTrapOnTile == card) {
                    return tile;
                }
            }
        }
        return null;
    }

    public void ShuffleDeck(List<Card> deck) {
        for (int i = 0; i < deck.Count; i++) {
            Card temp = deck[i];
            int randomIndex = UnityEngine.Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void SincDecks(List<int> deck1Ids, List<int> deck2Ids) {
        deck1 = deck1.OrderBy(card => deck1Ids.IndexOf(card.cardId)).ToList();
        deck2 = deck2.OrderBy(card => deck2Ids.IndexOf(card.cardId)).ToList();

        ObjectManager.Instance.deckPlayer.GetComponent<DeckVisual>().UpdateCardsPosition();
        ObjectManager.Instance.deckOpponent.GetComponent<DeckVisual>().UpdateCardsPosition();
    }

    public Card GetCardFromLocalId(int cardId) {
        foreach(Card card in hand1) {
            if(card.cardId == cardId) return card;
        }
        foreach(Card card in hand2) {
            if(card.cardId == cardId) return card;
        }
        foreach(Card card in deck1) {
            if(card.cardId == cardId) return card;
        }
        foreach(Card card in deck2) {
            if(card.cardId == cardId) return card;
        }
        foreach(Card card in gy1) {
            if(card.cardId == cardId) return card;
        }
        foreach(Card card in gy2) {
            if(card.cardId == cardId) return card;
        }
        foreach(Tile tile in field) {
            if (tile.monsterOnTile != null) { 
                if (tile.monsterOnTile.cardId == cardId) return tile.monsterOnTile;
            }
            if (tile.spellTrapOnTile != null) { 
                if (tile.spellTrapOnTile.cardId == cardId) return tile.spellTrapOnTile;
            }
        }
        return null;
    }

    public List<Card> GetCardListFromLocalIdList(List<int> cardIds) {
        List<Card> cards = new();
        foreach (int cardId in cardIds) {
            cards.Add(GetCardFromLocalId(cardId));
        }
        return cards;
    }

    public List<int> GetIdListFromCardList(List<Card> cardList) {
        List<int> cardIds = new();
        foreach (Card card in cardList) {
            cardIds.Add(card.cardId);
        }
        return cardIds;
    }

    public Player GetPlayerFromId(ulong playerId) {
        if (player1.id == playerId) {
            return player1;
        } else if (player2.id == playerId) {
            return player2;
        }
        return null;
    }

    public Tile GetTileFromId(int tileId) {
        foreach(Tile tile in field) {
            if (tile.tileId == tileId) {
                return tile;
            }
        }
        return null;
    }

    public void ChangeTurn(ulong playerId) {
        if (playerId == player1.id) {
            turnPlayer = player1;

            AddPlayerMana(player1.id, 2 - player1.starMana);
        } else {
            turnPlayer = player2;

            AddPlayerMana(player2.id, 2 - player2.starMana);
        }

        if (NetworkManager.Singleton.IsServer) {
            CardGameMultiplayer.Instance.SincDrawServerRpc(playerId, 1);
        }

        foreach(Tile tile in field) {
            if(tile.monsterOnTile != null) {
                MonsterCardData monsterCardData = (MonsterCardData)tile.monsterOnTile.cardData;
                monsterCardData.movequant = 1;
                monsterCardData.atkquant = 1;
            }
        }

        if(turnCount != 0) StartCoroutine(WaitForUpdateCardBorder(0.5f));
        turnCount += 1;
        Debug.Log("TURN CHANGED!");
        OnTurnChage?.Invoke(this, EventArgs.Empty);
    }

    public void DefineInitialTurnPlayer(ulong playerId) {
        if (playerId == player1.id) {
            turnPlayer = player1;
        } else {
            turnPlayer = player2;
        }
        turnCount = 1;
    }

    public int GetEventIndex(int cardId, CardEvent cardEvent) {

        int eventIndex = GetCardFromLocalId(cardId).GetCardSO().events.IndexOf(cardEvent);
        if (eventIndex == -1) {
            if(cardEvent == normalSummonCardEvent) {
                eventIndex = 99;
            }
            if(cardEvent == moveCardEvent) {
                eventIndex = 98;
            }
            if(cardEvent == atkCardEvent) {
                eventIndex = 97;
            }
            if (cardEvent == atkPlayerCardEvent) {
                eventIndex = 96;
            }
            if (cardEvent == setCardEvent) {
                eventIndex = 95;
            }
            if (cardEvent == placeSpellTrapCardEvent) {
                eventIndex = 94;
            }
        }
        return eventIndex;
    }

    public CardEvent GetEventById(int eventId, Card card) {
        if (eventId == 99) {
            return normalSummonCardEvent;
        } else if (eventId == 98) {
            return moveCardEvent;
        } else if (eventId == 97) {
            return atkCardEvent;
        } else if (eventId == 96) {
            return atkPlayerCardEvent;
        } else if (eventId == 95) {
            return setCardEvent;
        } else if (eventId == 94) {
            return placeSpellTrapCardEvent;
        }else {
            return card.GetCardSO().events[eventId];
        }
    }

    public void RemoveCard(Card card) {
        if (hand1.Contains(card)) {
            hand1.Remove(card);
            return;
        }
        if (hand2.Contains(card)) {
            hand2.Remove(card);
            return;
        }
        if (deck1.Contains(card)) {
            deck1.Remove(card);
            return;
        }
        if (deck2.Contains(card)) {
            deck2.Remove(card);
            return;
        }
        if (gy1.Contains(card)) {
            gy1.Remove(card);
            return;
        }
        if (gy2.Contains(card)) {
            gy2.Remove(card);
            return;
        }
        foreach(Tile tile in field) {
            if (tile.monsterOnTile == card) {
                tile.monsterOnTile = null;
                return;
            }
            if (tile.spellTrapOnTile == card) {
                tile.spellTrapOnTile = null;
                return;
            }
        }
    }

    public void UpdateCardsBorderVisual() {
        foreach (Card card in hand1) {
            card.GetComponent<CardVisual>().UpdateBorder();
        }
        foreach (Card card in hand2) {
            card.GetComponent<CardVisual>().UpdateBorder();
        }
        foreach (Card card in deck1) {
            card.GetComponent<CardVisual>().UpdateBorder();
        }
        foreach (Card card in deck2) {
            card.GetComponent<CardVisual>().UpdateBorder();
        }
        foreach (Card card in gy1) {
            card.GetComponent<CardVisual>().UpdateBorder();
        }
        foreach (Card card in gy2) {
            card.GetComponent<CardVisual>().UpdateBorder();
        }
        foreach (Tile tile in field) {
            if (tile.monsterOnTile != null) {
                tile.monsterOnTile.GetComponent<CardVisual>().UpdateBorder();
            }
            if (tile.spellTrapOnTile != null) {
                tile.spellTrapOnTile.GetComponent<CardVisual>().UpdateBorder();
            }
        }
    }

    public bool IsThereOponentCardInRange(List<int> range) {
        foreach(Tile tile in field) {
            if (range.Contains(tile.tileId) && tile.monsterOnTile != null) {
                if (tile.monsterOnTile.Owner != localPlayer) {
                    return true;
                }
            }
        }
        return false;
    }

    public IEnumerator WaitForUpdateCardBorder(float s) {
        yield return new WaitForSeconds(s);
        UpdateCardsBorderVisual();
    }

    public void AddPlayerMana(ulong playerId, int starMana) {
        if (playerId == player1.id) {
            player1.starMana += starMana;
        } else {
            player2.starMana += starMana;
        }
        OnManaChanged?.Invoke(this, EventArgs.Empty);
    }
    public void AddPlayerLifePoint(ulong playerId, int lifePoints) {
        if (playerId == player1.id) {
            player1.lp += lifePoints;
        } else {
            player2.lp += lifePoints;
        }
        OnLifePointsChanged?.Invoke(this, EventArgs.Empty);
    }
    
    private void OnDisable() {
        CardGameMultiplayer.Instance.OnLoadEventCompleted -= CardGameMultiplayer_OnLoadEventCompleted;
    }
}
