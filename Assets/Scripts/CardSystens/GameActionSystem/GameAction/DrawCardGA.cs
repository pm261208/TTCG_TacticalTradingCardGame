using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DrawCardGA : GameAction{

    public int drawNumber;
    public Player player;
    public List<Card> cardsDrawed;

    public DrawCardGA(int drawNumber, Player player) {
        this.drawNumber = drawNumber;
        this.player = player;
    }

}
