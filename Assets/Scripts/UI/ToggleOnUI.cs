using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleOnUI : MonoBehaviour{

    [SerializeField] private Button toggleOnButton;
    [SerializeField] private TextMeshProUGUI toggleOnText;

    private void Start() {
        toggleOnButton.onClick.AddListener(ToggleButton);
    }

    private void ToggleButton() {
        CardGameMultiplayer.Instance.ChangePlayerToggleOnServerRpc(CardGameManager.Instance.localPlayer.id);
        if (!CardGameManager.Instance.localPlayer.toggleOn) {
            toggleOnButton.image.color = Color.cyan;
            toggleOnText.text = "Toggle On";
        } else {
            toggleOnButton.image.color = Color.red;
            toggleOnText.text = "Toggle Off";
        }
    }
}
