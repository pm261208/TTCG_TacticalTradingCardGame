using SerializeReferenceEditor;
using UnityEngine;

[SRName("Is On Hand Condition")]
public class IsOnHandCondition : EventCondition {
    public override bool Evaluate(Card self, EffectContext ctx) {
        return CardGameManager.Instance.IsCardInHand(self);
    }
}
