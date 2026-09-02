using System.Collections.Generic;
using UnityEngine;

public class SelectCardEffectInteraction : PlayerInteraction{

    private Card selectedCard;
    private List<int> EffectIndexs;
    public CardEvent SelectedEvent {  get; private set; }
    public int? SelectedMainSpellTrapEvent {  get; private set; }

    public SelectCardEffectInteraction(Card card, List<int> effects, bool canCancel) {
        selectedCard = card;
        CanCancel = canCancel;
        EffectIndexs = effects;
    }
    public override void TryCancel() {
        if (CanCancel) {
            IsCanceled = true;
            Finish();
        }
    }

    public override void OnClickButton(TempButton button) {
        CardInteractionButton cardInteractionButton = button.GetComponent<CardInteractionButton>();
        if (cardInteractionButton.cardEvent != null) {
            SelectedEvent = cardInteractionButton.cardEvent;

            if (selectedCard.cardType == CardType.Spell)
                if (((SpellCardSO)selectedCard.GetCardSO()).mainSpellEvents.Contains(CardGameManager.Instance.GetEventIndex(selectedCard.cardId, cardInteractionButton.cardEvent)) &&
                    CardGameManager.Instance.IsCardInHand(selectedCard))
                    SelectedMainSpellTrapEvent = CardGameManager.Instance.GetEventIndex(selectedCard.cardId, cardInteractionButton.cardEvent);
        }
        Finish();
    }

    public override void OnClickCard(Card card) {
        if (card == selectedCard) return;
        TryCancel();
        if (CanCancel) {
            card.TrySelectCard();
        }
    }

    public override void OnClickZone(Tile tile) {
        TryCancel();
    }

    public override void OnEnter() {
        selectedCard.ShowInteractions(EffectIndexs);
    }

    public override void OnExit() {
        selectedCard.HideInteractions();
    }
}
