using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[SRName("Is Drawed Condition")]
public class IsDrawedCondition : EventCondition {
    public override bool Evaluate(Card self, EffectContext ctx) {
        if (ctx.eventData is OnDrawEventData drawData) {
            return new List<int>(drawData.drawedCards).Contains(self.cardId);
        }
        return false;
    }
}
