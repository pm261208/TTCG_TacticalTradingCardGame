using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using Unity.Netcode;
using UnityEngine;

[SRName("Attack Player Node")]
public class AttackPlayerNode : EffectNode {
    public override string HeaderText => "No requirement";


    public override IEnumerator Execute(EffectContext context) {
        Player player;
        if (CardGameManager.Instance.GetCardFromLocalId(context.Source).Owner == CardGameManager.Instance.player1) {
            player = CardGameManager.Instance.player2;
        } else {
            player = CardGameManager.Instance.player1;
        }
        AttackPlayerGA attackPlayerGA = new(player, CardGameManager.Instance.GetCardFromLocalId(context.Source).cardId);
        yield return ActionSystem.Instance.Perform(attackPlayerGA);

        if (NetworkManager.Singleton.IsServer) {
            /*
            EffectContext newContext = new();
            newContext.Source = context.Source;
            newContext.eventData = new OnAttackedEventData {
                attackedCard = CardGameManager.Instance.GetIdListFromCardList(new List<Card> { attackPlayerGA.attackedCard }).ToArray()
            };

            EventSystem.Instance.RaiseEvent(TriggerType.OnAttacked, newContext);
            */
        }

        if (nextEffect != null) {
            yield return nextEffect.Execute(context);
        }
    }
}
