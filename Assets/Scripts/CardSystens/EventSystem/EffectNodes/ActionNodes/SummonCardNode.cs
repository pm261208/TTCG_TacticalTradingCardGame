using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using SerializeReferenceEditor;
using Unity.Netcode;
using UnityEngine;

[SRName("Summon Card Node")]
public class SummonCardNode : EffectNode {
    public override string HeaderText => "Requires: Card Tile";

    public override IEnumerator Execute(EffectContext context) {
        SummonCardGA summonCardGA = new(CardGameManager.Instance.GetCardFromLocalId(GetCardSubject(context)[0]), CardGameManager.Instance.GetTileFromId(GetCardSubject(context)[1]));
        yield return ActionSystem.Instance.Perform(summonCardGA);

        if (NetworkManager.Singleton.IsServer) {
            EffectContext newContext = new() { 
                Source = context.Source,
            };
            newContext.eventData = new OnSummonEventData {
                summonedCards = new int[] { summonCardGA.card.cardId } 
            };

            EventSystem.Instance.RaiseEvent(TriggerType.OnSummon, newContext);
        }

        if (nextEffect != null) {
            yield return nextEffect?.Execute(context);
        } 
    }
}
