using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTemplateUI : MonoBehaviour{

    [SerializeField] private TextMeshProUGUI playerName;
    
    public void SetPlayerName(string playerName) {
        this.playerName.text = playerName;
    }

    public void SetColor(byte r, byte g, byte b) {
        Color32 templateColor = new Color32 (r, g, b, 255);
        GetComponent<Image>().color = templateColor;
    }
}
