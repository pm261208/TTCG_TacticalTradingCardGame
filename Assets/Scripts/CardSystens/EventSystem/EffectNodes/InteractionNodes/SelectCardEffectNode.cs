using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using SerializeReferenceEditor;
using UnityEngine;

[SRName("Select Card Effect Node")]
public class SelectCardEffectNode : EffectNode {
    public override string HeaderText => "Requires: Card";

    public List<int> eventIndexs;

    public override IEnumerator Execute(EffectContext context) {
        if (CardGameManager.Instance.localPlayer.id == context.Owner) {
            var interaction = new SelectCardEffectInteraction(CardGameManager.Instance.GetCardFromLocalId(GetCardSubject(context)[0]), eventIndexs, true);

            InteractionSystem.Instance.StartInteraction(interaction);

            yield return interaction.WaitForFinish();

            if (interaction.IsCanceled) yield break;

            
            CardGameMultiplayer.Instance.SincCardEvent(CardGameManager.Instance.GetEventIndex(context.Source, interaction.SelectedEvent), context);    
        
        } else {

            Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

            yield return new WaitUntil(() => task.IsCompleted);

            EffectContext newContext = task.Result;

            yield return nextEffect?.Execute(newContext);
        }
    }

}
