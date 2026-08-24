using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[SRName("Select Opponent Card On Field Node")]
public class SelectOpponentCardOnFieldNode : EffectNode {
    public override string HeaderText => "Requires: Card";

    public List<int> eventIndexs;

    public override IEnumerator Execute(EffectContext context) {
        /*
        if (CardGameManager.Instance?.localPlayer.id == context.Owner) {
            Card card = CardGameManager.Instance.GetCardFromLocalId(GetCardSubject(context)[0]);

            var interaction = new SelectMoveAtkInteraction(card, eventIndexs, CardGameManager.Instance.GetCardFromLocalId(GetCardSubject(context)[0]).GetMoveRange(), null, true);

            InteractionSystem.Instance.StartInteraction(interaction);

            yield return interaction.WaitForFinish();

            if (interaction.IsCanceled) yield break;

            if (interaction.SelectedEvent != null) {
                CardGameMultiplayer.Instance.SincCardEvent(CardGameManager.Instance.GetEventIndex(context.Source, interaction.SelectedEvent), context);

            }
            if (interaction.SelectedTile != null) {
                context.TargetTile = interaction.SelectedTile.tileId;
                CardGameMultiplayer.Instance.SincCardEvent(CardGameManager.Instance.GetEventIndex(context.Source, CardGameManager.Instance.moveCardEvent), context);
            }

        }
        */
        yield break;
    }

}
