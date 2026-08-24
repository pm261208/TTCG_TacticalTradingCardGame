using System.Collections.Generic;
using UnityEngine;

public class GameState{

    public int player1;
    public int player2;
    public int turnPlayer;
    public int[,,] field = new int[5, 5, 2];
    public List<int> hand1;
    public List<int> hand2;
    public List<int> deck1;
    public List<int> deck2;
    public List<int> gy1;
    public List<int> gy2;
}
