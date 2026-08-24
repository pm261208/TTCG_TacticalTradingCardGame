using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSystem : Singleton<InteractionSystem> {

    public PlayerInteraction currentInteraction;

    public void StartInteraction(PlayerInteraction interaction) {
        currentInteraction?.OnExit();
        currentInteraction = interaction;
        if (currentInteraction != null) {
            Debug.Log(interaction.ToString());
        } else {
            Debug.Log("interaction null");
        }
        currentInteraction?.OnEnter();
    }

    public bool TryCancel() {
        if (currentInteraction != null && currentInteraction.CanCancel) {
            currentInteraction.TryCancel();
            return true;
        }
        return false;
    }

    public void ClickCard(Card card) {
        currentInteraction?.OnClickCard(card);
    }

    public void ClickZone(Tile tile) {
        currentInteraction?.OnClickZone(tile);
    }
    public void ClickButton(TempButton button) {
        currentInteraction?.OnClickButton(button);
    }

    
}
