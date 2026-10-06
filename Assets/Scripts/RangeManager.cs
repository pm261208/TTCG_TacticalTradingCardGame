using UnityEngine;
using System.Collections.Generic;

public class RangeManager{
    
    private static readonly Dictionary<string, List<int>> rangeDicionary = new(){
        { "R1", new (){ 1, -1, 10, -10 } },
        { "R2", new (){ 1, -1, 9, -9, 10, -10, 11, -11 } },
        { "R3", new (){ 1, -1, 2, -2, 9, -9, 10, -10, 11, -11, 20, -20 } },
        { "C1", new (){10, -10 } },
        { "C2", new (){10, -10, 20, -20 } },
        { "C3", new (){10, -10, 20, -20, 30, -30 } },
        { "C4", new (){10, -10, 20, -20, 30, -30, 40, -40 } },
        { "C5", new (){10, -10, 20, -20, 30, -30, 40, -40, 50, -50 } },
    };

    public static List<int> GetRange(string rangeName) {
        return rangeDicionary[rangeName];
    }

}
