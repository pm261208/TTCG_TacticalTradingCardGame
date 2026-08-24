using UnityEngine;

public class CardInteractionButton : TempButton {

    public CardEvent cardEvent;
    public Card card;


    public override void Onclick() {
        
    }

    public void DefineAction(CardEvent cardEffect) {
        cardEvent = cardEffect;
    }

}
