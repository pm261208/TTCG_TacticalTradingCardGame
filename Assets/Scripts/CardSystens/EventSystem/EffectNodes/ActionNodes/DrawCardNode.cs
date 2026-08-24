using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using SerializeReferenceEditor;
using Unity.Netcode;
using UnityEngine;


[SRName("Draw Card Node")]
public class DrawCardNode : EffectNode {
    public override string HeaderText => "No requirement";

    public int drawNumber;

    public override IEnumerator Execute(EffectContext context) {
        DrawCardGA drawCardGA = new(drawNumber, CardGameManager.Instance.GetPlayerFromId(context.Owner));

        yield return ActionSystem.Instance.Perform(drawCardGA);

        if (NetworkManager.Singleton.IsServer) {
            EffectContext newContext = new EffectContext();
            newContext.Source = context.Source;
            newContext.eventData = new OnDrawEventData {
                drawedCards = CardGameManager.Instance.GetIdListFromCardList(drawCardGA.cardsDrawed).ToArray()
            };

            EventSystem.Instance.RaiseEvent(TriggerType.OnDraw, newContext);
        }

        if (nextEffect != null) {
            yield return nextEffect?.Execute(context);
        } 
    }
}
