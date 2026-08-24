using UnityEngine;

public class AttackPlayerGA : GameAction {

    public Player player;
    public int cardId;

    public AttackPlayerGA(Player player, int cardId) {
        this.player = player;
        this.cardId = cardId;
    }
}
