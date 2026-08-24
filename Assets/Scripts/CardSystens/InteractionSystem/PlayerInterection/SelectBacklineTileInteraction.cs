using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class SelectBacklineTileInteraction : PlayerInteraction {

    private ulong Player;
    public Tile SelectedTile { get; private set; }

    public SelectBacklineTileInteraction(ulong player, bool canCancel) {
        Player = player;
        CanCancel = canCancel;    
    }

    public override void TryCancel() {
        if (CanCancel) {
            IsCanceled = true;
            Finish();
        }
    }

    public override void OnClickButton(TempButton button) {
        TryCancel();
    }

    public override void OnClickCard(Card card) {
        TryCancel();
        if (CanCancel) {
            card.TrySelectCard();
        }
    }

    public override void OnClickZone(Tile tile) {
        if (!tile.isSelectable)
            return;

        SelectedTile = tile;

        Finish();
    }

    public override void OnEnter() {
        foreach (Tile tile in CardGameManager.Instance.field) {
            if (tile.IsTileBackline(Player) && tile.IsTileEmpty()) {
                tile.SelectableTile();
            }
        }
    }

    public override void OnExit() {
        foreach (Tile tile in CardGameManager.Instance.field) {
            tile.UnselectableTile();
        }
    }
}
