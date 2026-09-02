using UnityEngine;

public class SetCardGA : GameAction {

    public Card card;
    public Tile tile;

    public SetCardGA(Card card, Tile tile) {
        this.card = card;
        this.tile = tile;
    }
}
