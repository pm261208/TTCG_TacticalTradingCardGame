using System.Collections;
using System.Threading.Tasks;
using SerializeReferenceEditor;
using UnityEngine;

[SRName("Select Tile On Your Field Node")]
public class SelectTileOnYourFieldNode : EffectNode {
    public override string HeaderText => "No requirement";

    public override IEnumerator Execute(EffectContext context) {
        if (CardGameManager.Instance.localPlayer.id == context.Owner) {
            var interaction = new SelectTileOnYourFieldInteraction(context.Owner, false);

            InteractionSystem.Instance.StartInteraction(interaction);

            yield return interaction.WaitForFinish();

            if (interaction.IsCanceled) yield break;

            context.TargetTile = interaction.SelectedTile.tileId;

            yield return nextEffect?.Execute(context);
            CardGameMultiplayer.Instance.SendInteractionResultServerRpc(context);

        } else {

            Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

            yield return new WaitUntil(() => task.IsCompleted);

            EffectContext newContext = task.Result;

            yield return nextEffect?.Execute(newContext);
        }

    }
}
