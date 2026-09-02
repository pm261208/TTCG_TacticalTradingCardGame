using System.Collections;
using UnityEngine;

public class PlaceSpellTrapCardOnFieldNode : EffectNode{

    public override string HeaderText => "Requires: Card Tile";

    public override IEnumerator Execute(EffectContext context) {

        yield return ActionSystem.Instance.Perform(new PlaceSpellTrapCardOnFieldGA(CardGameManager.Instance.GetCardFromLocalId(GetCardSubject(context)[0]), CardGameManager.Instance.GetTileFromId(GetCardSubject(context)[1])));

        if (nextEffect != null) {
            yield return nextEffect?.Execute(context);
        }
    }
}
