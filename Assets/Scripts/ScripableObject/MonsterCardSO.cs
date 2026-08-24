using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Monster")]
public class MonsterCardSO : CardSO {
    public int starLevel;
    public int atk;
    public int hp;

    public List<int> moveRange;
    public List<int> atkRange;

}
