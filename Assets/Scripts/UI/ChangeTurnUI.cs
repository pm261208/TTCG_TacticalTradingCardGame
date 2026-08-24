using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTurnUI : MonoBehaviour{

    [SerializeField] private Button turnButton;

    private void Start() {
        CardGameManager.Instance.OnTurnChage += CardGameManager_OnTurnChage;
        turnButton.onClick.AddListener(() => {
            if (InteractionSystem.Instance.currentInteraction == null || InteractionSystem.Instance.currentInteraction.CanCancel) {

                InteractionSystem.Instance.TryCancel();

                CardGameMultiplayer.Instance.ChangeTurnPlayerServerRpc();
            }
            
        });

    }

    private void CardGameManager_OnTurnChage(object sender, System.EventArgs e) {
        if (CardGameManager.Instance.turnPlayer == CardGameManager.Instance.localPlayer) {
            turnButton.gameObject.SetActive(true);
        } else {
            turnButton.gameObject.SetActive(false);
        }
    }
}
