using System;
using UnityEngine;

public class YSWindowInteraction : PlayerInteraction {

    public static event EventHandler YSWindowOpen;
    public static event EventHandler YSWindowClose;

    public TempButton SelectedButton { get; private set; }

    public override void OnClickButton(TempButton button) {
        SelectedButton = button;
        Finish();
    }

    public override void OnClickCard(Card card) {
        
    }

    public override void OnClickZone(Tile tile) {
        
    }

    public override void OnEnter() {
        YSWindowOpen?.Invoke(this, EventArgs.Empty);
    }

    public override void OnExit() {
        YSWindowClose?.Invoke(this, EventArgs.Empty);
    }

    public override void TryCancel() {
        
    }
}
