using UnityEngine;

public class SummonCardGA : GameAction {

    public Card card;
    public Tile tile;

    public SummonCardGA(Card card, Tile tile) {
        this.card = card;
        this.tile = tile;
    }

}
