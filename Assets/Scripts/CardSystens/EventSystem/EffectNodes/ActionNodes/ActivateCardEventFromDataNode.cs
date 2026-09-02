using System.Collections;
using SerializeReferenceEditor;
using UnityEngine;
using WebSocketSharp;
using static UnityEngine.UI.GridLayoutGroup;

[SRName("Activate Card Event From Data Node")]
public class ActivateCardEventFromDataNode : EffectNode {
    public override string HeaderText => "eventIndex";

    public override IEnumerator Execute(EffectContext context) {
        Card card = CardGameManager.Instance.GetCardFromLocalId(context.Source);

        yield return ChainSystem.Instance.ActivateIgnition(
            card,
            CardGameManager.Instance.GetEventById(GetCardSubject(context)[0], card),
            new EffectContext() { Source = card.cardId, Owner = context.Owner },
            card.Owner
        );
    }
}