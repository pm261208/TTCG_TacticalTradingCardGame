using System;
using UnityEngine;

[System.Serializable]
public abstract class EventCondition{

    public abstract bool Evaluate(Card self, EffectContext ctx);
}
