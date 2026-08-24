using System.Collections;
using SerializeReferenceEditor;
using UnityEngine;

[SRName("Remove StarMana Normal Summon Node")]
public class RemoveStarManaNormalSummonNode : EffectNode {
    public override string HeaderText => "Requires: No requirement";

    public override IEnumerator Execute(EffectContext context) {
        RemoveStarManaAmountGA RemoveManaGA = new(CardGameManager.Instance.GetPlayerFromId(context.Owner), ((MonsterCardData)CardGameManager.Instance.GetCardFromLocalId(context.Source).cardData).cardStarLevel);
        yield return ActionSystem.Instance.Perform(RemoveManaGA);


        if (nextEffect != null) {
            yield return nextEffect.Execute(context);
        }
    }
}
