using SerializeReferenceEditor;
using UnityEngine;

[SRName("Is On Field Condition")]
public class IsOnFieldCondition : EventCondition {
    public override bool Evaluate(Card self, EffectContext ctx) {
        return CardGameManager.Instance.IsCardInField(self);
    }
}
