using System;
using System.Collections;
using System.Reflection;
using SerializeReferenceEditor;
using UnityEngine;

[SRName("Summon Card Node")]
public class SummonCardNode : EffectNode {
    public override string HeaderText => "Requires: Card Tile";

    public override IEnumerator Execute(EffectContext context) {

        yield return ActionSystem.Instance.Perform(new SummonCardGA(CardGameManager.Instance.GetCardFromLocalId(GetCardSubject(context)[0]), CardGameManager.Instance.GetTileFromId(GetCardSubject(context)[1])));

        //CardGameMultiplayer.Instance.SincActionNode(ActionNodesEnum.SummonCardNode, cardSubject, context);
        if (nextEffect != null) {
            yield return nextEffect?.Execute(context);
        } 
    }
}
