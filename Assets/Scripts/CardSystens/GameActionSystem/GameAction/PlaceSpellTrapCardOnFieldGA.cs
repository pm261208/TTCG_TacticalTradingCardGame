using UnityEngine;

public class PlaceSpellTrapCardOnFieldGA : GameAction {

    public Card card;
    public Tile tile;

    public PlaceSpellTrapCardOnFieldGA(Card card, Tile tile) {
        this.card = card;
        this.tile = tile;
    }

}
