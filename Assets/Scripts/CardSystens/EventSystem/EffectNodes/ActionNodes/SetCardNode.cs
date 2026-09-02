using System.Collections;
using UnityEngine;

public class SetCardNode : EffectNode {
    public override string HeaderText => "Requires: Card Tile";

    public override IEnumerator Execute(EffectContext context) {

        yield return ActionSystem.Instance.Perform(new SetCardGA(CardGameManager.Instance.GetCardFromLocalId(GetCardSubject(context)[0]), CardGameManager.Instance.GetTileFromId(GetCardSubject(context)[1])));

        if (nextEffect != null) {
            yield return nextEffect?.Execute(context);
        }
    }
}
