using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChoseCardsResponseWindow : MonoBehaviour{

    [SerializeField] private GameObject Layout;
    [SerializeField] private GameObject Content;
    [SerializeField] private TempButton cardOptionTemplate;
    [SerializeField] private TempButton activateButton;
    [SerializeField] private TempButton cancelButton;

    void Start(){
        SelectCardWindowInteraction.SelectCardWindowOpen += SelectCardWindowInteraction_SelectCardWindowOpen;
        SelectCardWindowInteraction.SelectCardWindowClose += SelectCardWindowInteraction_SelectCardWindowClose;
        cardOptionTemplate.gameObject.SetActive(false);
        Layout.SetActive(false);
    }

    private void SelectCardWindowInteraction_SelectCardWindowClose(object sender, System.EventArgs e) {
        foreach (Transform filho in Content.transform) {
            if (filho.gameObject.activeSelf) {
                Destroy(filho.gameObject);
            }
        }
        Layout.SetActive(false);
    }

    private void SelectCardWindowInteraction_SelectCardWindowOpen(object sender, SelectCardWindowInteraction.SelectCardWindowOpenEventArgs e) {
        Layout.SetActive(true);
        if (e.selectedCard == null) {
            activateButton.isClickable = false;
            activateButton.GetComponent<Image>().color = new Color32(60, 132, 36, 255);
        } else {
            activateButton.isClickable = true;
            activateButton.GetComponent<Image>().color = new Color32(122, 255, 78, 255);
        }
        if (e.canCancel) {
            cancelButton.isClickable = true;
            cancelButton.GetComponent<Image>().color = new Color32(255, 78, 78, 255);
        } else {
            cancelButton.isClickable = false;
            cancelButton.GetComponent<Image>().color = new Color32(150, 68, 68, 255);
        }
        foreach (Transform filho in Content.transform) {
            if (filho.gameObject.activeSelf) {
                Destroy(filho.gameObject);
            }
        }

        foreach (Card card in e.cards) {
            TempButton cardOp = Instantiate(cardOptionTemplate);
            cardOp.gameObject.SetActive(true);
            ((CardOptionButton)cardOp).card = card;
            ((CardOptionButton)cardOp).Setup();
            cardOp.transform.SetParent(Content.transform);
        }
        
        foreach (Transform filho in Content.transform) {
            CardOptionButton opitionButton = filho.GetComponent<CardOptionButton>();
            if (e.selectedCard == opitionButton.card) {
                opitionButton.SelectedVisual.SetActive(true);
            } else {
                opitionButton.SelectedVisual.SetActive(false);
            }
        }
        

    }
}
