using SerializeReferenceEditor;
using UnityEngine;

[SRName("Soft Once Per Turn Condition")]
public class SoftOncePerTurnCondition : EventCondition {

    public int currentEventId;

    public override bool Evaluate(Card self, EffectContext ctx) {
        foreach (EventLog eventLog in CardGameManager.Instance.cardEventLogs) {
            Card card = CardGameManager.Instance.GetCardFromLocalId(eventLog.sourceCardId);
            if (eventLog.turn == CardGameManager.Instance.turnCount &&
                CardGameManager.Instance.GetCardFromLocalId(eventLog.sourceCardId).GetCardSO().id == self.GetCardSO().id &&
                eventLog.eventId == currentEventId &&
                eventLog.instanceId == card.instanceId) {
                return false;
            }
        }
        return true;
    }
}
