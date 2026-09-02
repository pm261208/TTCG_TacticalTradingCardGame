using UnityEngine;

public class SelectTileOnYourFieldInteraction : PlayerInteraction {

    private ulong Player;
    public Tile SelectedTile { get; private set; }

    public SelectTileOnYourFieldInteraction(ulong player, bool canCancel) {
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
        Tile tile = CardGameManager.Instance.GetTileWCard(card);
        if (tile != null) {
            if (tile.spellTrapOnTile == null && tile.isSelectable) {
                SelectedTile = tile;

                Finish();
                return;
            }
        }
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
            if (tile.IsTileInYourField(Player) && !tile.HasSpellTrap()) {
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
