using System;
using UnityEngine;

[Serializable]
public abstract class EventCondition{

    public abstract bool Evaluate(Card self, EffectContext ctx);
}
