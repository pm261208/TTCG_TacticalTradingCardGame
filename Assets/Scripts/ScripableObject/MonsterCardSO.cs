using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Monster")]
public class MonsterCardSO : CardSO {
    public int starLevel;
    public int atk;
    public int hp;

    public string moveRange;
    public string atkRange;

}
