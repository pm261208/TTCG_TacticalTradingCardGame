using SerializeReferenceEditor;
using UnityEngine;

[SRName("Is Set For A Turn Condition")]
public class IsSetForATurnCondition : EventCondition {
    public override bool Evaluate(Card self, EffectContext ctx) {
        EventLog eventLog = CardGameManager.Instance.cardEventLogs.Find(x => x.eventId == 95 && x.turn != CardGameManager.Instance.turnCount && x.instanceId + 1 == self.instanceId);
        return CardGameManager.Instance.IsCardInField(self) && self.isSet && eventLog != null;
    }
}
