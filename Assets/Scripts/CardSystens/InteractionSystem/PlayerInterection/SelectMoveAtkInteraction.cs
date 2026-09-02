using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SelectMoveAtkInteraction : PlayerInteraction {

    public CardEvent SelectedEvent { get; private set; }
    public Tile SelectedTile { get; private set; }
    public Card SelectedCard { get; private set; }

    private Card selectedCard;
    private List<int> tileRange;
    private List<int> atkRange;
    public List<int> cardEvents;

    public SelectMoveAtkInteraction(Card card, List<int> effects, List<int> tile, List<int> atk, bool canCancel) {
        selectedCard = card;
        CanCancel = canCancel;
        cardEvents = effects;
        tileRange = tile;
        atkRange = atk;
    }

public override void OnEnter() {
    selectedCard.ShowInteractions(cardEvents);

    if(((MonsterCardData)selectedCard.cardData).movequant >= 1) { 
        foreach (Tile tile in CardGameManager.Instance.field) {
            if (tileRange.Contains(tile.tileId) && !tile.HasMonster()) {
                tile.SelectableTile();
            }
            
        } 
    }
    if(((MonsterCardData)selectedCard.cardData).atkquant >= 1) { 
        foreach (Tile tile in CardGameManager.Instance.field) {
            if (tile.HasMonster()) {
                if (atkRange.Contains(tile.tileId) && tile.monsterOnTile.Owner != CardGameManager.Instance.localPlayer) {
                    tile.monsterOnTile.ShowTarget(CardGameManager.Instance.atkCardEvent);
                }
                if (atkRange.Contains(99)) {
                    ObjectManager.Instance.attackPlayerButton.GetComponent<CardInteractionButton>().cardEvent = CardGameManager.Instance.atkPlayerCardEvent;
                    ObjectManager.Instance.attackPlayerButton.gameObject.SetActive(true);
                }
            }
            
        } 
    }
}

public override void OnClickZone(Tile tile) {
    if (!tile.isSelectable) TryCancel();

    SelectedTile = tile;
    Finish();
}

public override void TryCancel() {
        if (CanCancel) {
            IsCanceled = true;
            Finish();
        }
    }

public override void OnExit() {
    foreach (Tile tile in CardGameManager.Instance.field) {
        tile.UnselectableTile();
        if (tile.monsterOnTile != null) {
            tile.monsterOnTile.HideTarget();
        }
    }
    ObjectManager.Instance.attackPlayerButton.gameObject.SetActive(false);
    selectedCard.HideInteractions();
}

public override void OnClickCard(Card card) {
        TryCancel();
        if (CanCancel) {
            card.TrySelectCard();
        }
    }

public override void OnClickButton(TempButton button) {
        CardInteractionButton cardInteractionButton = button.GetComponent<CardInteractionButton>();
        if (cardInteractionButton.cardEvent != null) {
            if (cardInteractionButton.cardEvent == CardGameManager.Instance.atkCardEvent) SelectedCard = cardInteractionButton.card;
            SelectedEvent = cardInteractionButton.cardEvent;
            Finish();
        }
        
    }
}