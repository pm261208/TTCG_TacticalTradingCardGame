using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[SRName("Disjunction Condition")]
public class DisjunctionCondition : EventCondition {

    [SerializeReference]
    [SR]
    public List<EventCondition> conditionList;

    public override bool Evaluate(Card self, EffectContext ctx) {
        foreach (EventCondition condition in conditionList) {
            if(condition.Evaluate(self, ctx)) return true;
        }
        return false;
    }
}
