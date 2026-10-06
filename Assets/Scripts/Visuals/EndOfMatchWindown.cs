using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndOfMatchWindown : MonoBehaviour{

    [SerializeField] private GameObject visual;
    [SerializeField] private TextMeshProUGUI result;
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private Button goToMenuButton;

    private void Start() {
        visual.SetActive(false);
        CardGameManager.Instance.OnMatchEnd += CardGameManager_OnMatchEnd;

        goToMenuButton.onClick.AddListener(() => {
            StartCoroutine(CardGameManager.Instance.LeaveMatch());
        });
    }

    private void CardGameManager_OnMatchEnd(object sender, CardGameManager.OnMatchEndEventArgs e) {
        visual.SetActive(true);
        if (e.winner == CardGameManager.Instance.localPlayer) {
            result.text = "Você Venceu!";
            message.text = "Reduziu a vida do oponente a 0";
        } else {
            result.text = "Você Perdeu!";
            message.text = "Sua vida foi reduzida a 0";
        }
    }
}
