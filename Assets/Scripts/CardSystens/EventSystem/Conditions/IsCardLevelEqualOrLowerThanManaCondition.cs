using SerializeReferenceEditor;
using UnityEngine;

[SRName("Is Card Level Equal Or Lower Than Mana Condition")]
public class IsCardLevelEqualOrLowerThanManaCondition : EventCondition {

    public override bool Evaluate(Card self, EffectContext ctx) {
        if (((MonsterCardData)self.cardData).cardStarLevel <= CardGameManager.Instance.GetPlayerFromId(ctx.Owner).starMana) {
            return true;
        }
        return false;
    }
}
