using UnityEngine;

public class AttackCardGA : GameAction {

    public Card attackingCard;
    public Card attackedCard;

    public AttackCardGA(Card attackingCard, Card attackedCard) {
        this.attackingCard = attackingCard;
        this.attackedCard = attackedCard;
    }
}
