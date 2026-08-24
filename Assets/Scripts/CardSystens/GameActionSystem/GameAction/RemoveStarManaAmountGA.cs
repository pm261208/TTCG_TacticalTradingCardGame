using UnityEngine;

public class RemoveStarManaAmountGA : GameAction{

    public Player player;
    public int startAmount;

    public RemoveStarManaAmountGA(Player player, int startAmount) {
        this.player = player;
        this.startAmount = startAmount;
    }
}
