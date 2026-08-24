using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class SelectCardWindowInteraction : PlayerInteraction {

    public static event EventHandler<SelectCardWindowOpenEventArgs> SelectCardWindowOpen;
    public static event EventHandler SelectCardWindowClose;
    public class SelectCardWindowOpenEventArgs : EventArgs {
        public Card selectedCard;
        public List<Card> cards;
        public bool canCancel;
    }

    public TempButton selectedButton;
    public Card selectedCard;
    public List<Card> cards;

    public SelectCardWindowInteraction(List<Card> cardList, bool canCancel) {
        cards = cardList;
        CanCancel = canCancel;
    }

    public override void OnClickButton(TempButton button) {
        if (button.isClickable) {
            if(button is CardOptionButton opitionButton) {
                if (selectedCard == opitionButton.card) {
                    selectedCard = null;
                } else {
                    selectedCard = opitionButton.card;
                }
                SelectCardWindowOpen?.Invoke(this, new SelectCardWindowOpenEventArgs {
                    selectedCard = selectedCard,
                    cards = cards,
                    canCancel = CanCancel,
                });
            } else {
                selectedButton = button;
                if (selectedButton is ActivateButton activateButton) {
                    
                }
                if (selectedButton is CancelButton cancelButton) {
                    selectedCard = null;
                }
                Finish();
            }
                
        }
    }

    public override void OnClickCard(Card card) {

    }

    public override void OnClickZone(Tile tile) {
        
    }

    public override void OnEnter() {
        SelectCardWindowOpen?.Invoke(this, new SelectCardWindowOpenEventArgs {
            selectedCard = selectedCard,
            cards = cards,
            canCancel = CanCancel,
        });
    }

    public override void OnExit() {
        SelectCardWindowClose?.Invoke(this, EventArgs.Empty);
    }

    public override void TryCancel() {
        
    }
}
