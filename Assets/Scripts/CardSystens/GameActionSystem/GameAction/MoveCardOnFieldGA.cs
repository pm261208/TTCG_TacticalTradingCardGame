using UnityEngine;

public class MoveCardOnFieldGA : GameAction{

    public Card card;
    public Tile tile;

    public MoveCardOnFieldGA(Card card, Tile tile) {
        this.card = card;
        this.tile = tile;
    }
}
