using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SerializeReferenceEditor;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp;
using static Unity.VisualScripting.Member;
using static UnityEngine.UI.GridLayoutGroup;

[SRName("Select Tile Atk Effect Node")]
public class SelectMoveAtkEffectNode : EffectNode {
    public override string HeaderText => "Requires: Card";

    public List<int> eventIndexs;

    public override IEnumerator Execute(EffectContext context) {
        if (CardGameManager.Instance?.localPlayer.id == context.Owner) {
            Card card = CardGameManager.Instance.GetCardFromLocalId(GetCardSubject(context)[0]);

            var interaction = new SelectMoveAtkInteraction(card, eventIndexs, card.GetMoveRange(), card.GetAtkRange(), true);

            InteractionSystem.Instance.StartInteraction(interaction);

            yield return interaction.WaitForFinish();

            if (interaction.IsCanceled) yield break;

            if (interaction.SelectedEvent != null) {
                if (interaction.SelectedCard != null) {
                    context.TargetCard = interaction.SelectedCard.cardId;
                }
                CardGameMultiplayer.Instance.SincCardEvent(CardGameManager.Instance.GetEventIndex(context.Source, interaction.SelectedEvent), context);
                
            }
            if (interaction.SelectedTile != null) {
                context.TargetTile = interaction.SelectedTile.tileId;
                CardGameMultiplayer.Instance.SincCardEvent(CardGameManager.Instance.GetEventIndex(context.Source, CardGameManager.Instance.moveCardEvent), context);
            }

        }
    }
}