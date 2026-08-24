using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using Unity.Netcode;
using UnityEngine;

[SRName("Attack Card Node")]
public class AttackCardNode : EffectNode {
    public override string HeaderText => "No requirement";


    public override IEnumerator Execute(EffectContext context) {
        AttackCardGA attackCardGA = new(CardGameManager.Instance.GetCardFromLocalId(context.Source), CardGameManager.Instance.GetCardFromLocalId(context.TargetCard));
        yield return ActionSystem.Instance.Perform(attackCardGA);

        if (NetworkManager.Singleton.IsServer) {
            EffectContext newContext = new();
            newContext.Source = context.Source;
            newContext.eventData = new OnAttackedEventData {
                attackedCard = CardGameManager.Instance.GetIdListFromCardList(new List<Card>{attackCardGA.attackedCard }).ToArray()
            };

            EventSystem.Instance.RaiseEvent(TriggerType.OnAttacked, newContext);
        }

        if (nextEffect != null) {
            yield return nextEffect.Execute(context);
        }
    }
}
