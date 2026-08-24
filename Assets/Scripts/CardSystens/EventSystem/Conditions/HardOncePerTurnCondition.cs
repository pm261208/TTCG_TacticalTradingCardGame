using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;


[SRName("Hard Once Per Turn Condition")]
public class HardOncePerTurnCondition : EventCondition {

    public int currentEventId;

    public override bool Evaluate(Card self, EffectContext ctx) {
        foreach (EventLog eventLog in CardGameManager.Instance.cardEventLogs) {
            if (eventLog.turn == CardGameManager.Instance.turnCount && 
                CardGameManager.Instance.GetCardFromLocalId(eventLog.sourceCardId).GetCardSO().id == self.GetCardSO().id &&
                eventLog.eventId == currentEventId) {
                return false;
            }
        }
        return true;
    }
}
