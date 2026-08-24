using System;
using SerializeReferenceEditor;
using UnityEngine;

[SRName("Is Self Condition")]
public class IsSelfCondition : EventCondition {
    public override bool Evaluate(Card self, EffectContext ctx) {
        return ctx.Source == self.cardId;
    }
}