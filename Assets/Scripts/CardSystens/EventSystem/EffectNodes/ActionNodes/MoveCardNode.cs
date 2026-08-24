using System.Collections;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

public class MoveCardNode :  EffectNode {
    public override string HeaderText => "Requires: Card Tile";

    public override IEnumerator Execute(EffectContext context) {
        MoveCardOnFieldGA moveCardGA = new(CardGameManager.Instance.GetCardFromLocalId(GetCardSubject(context)[0]), CardGameManager.Instance.GetTileFromId(GetCardSubject(context)[1]));
        yield return ActionSystem.Instance.Perform(moveCardGA);

        
        if (nextEffect != null) {
            yield return nextEffect?.Execute(context);
        } 
    }
}
