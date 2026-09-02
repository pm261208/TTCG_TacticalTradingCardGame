using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

[SRName("Activate CardEvent Node")]
public class ActivateCardEventNode : EffectNode {
    public override string HeaderText => "No requirement";
    
    public int cardEventIndex;

    public override IEnumerator Execute(EffectContext context) {

        yield return ActionSystem.Instance.Perform(new DeclareEffectGA(CardGameManager.Instance.GetCardFromLocalId(context.Source)));
    }
}
