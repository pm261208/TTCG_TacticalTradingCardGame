using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputSystem : MonoBehaviour{


    public GraphicRaycaster graphicRaycaster;
    public UnityEngine.EventSystems.EventSystem eventSystem;

    private Ray ray;
    private RaycastHit hit;


    void Update(){
        if (Input.GetMouseButtonDown(0)){

            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) {

                GameObject clickedObject = hit.collider.gameObject;
                Component[] components = clickedObject.GetComponents<Component>();


                if (ActionSystem.Instance.IsPerforming) return;

                if(clickedObject.GetComponent<Card>() != null) {
                    Card card = clickedObject.GetComponent<Card>();
                    if (InteractionSystem.Instance.currentInteraction == null) {

                        card.TrySelectCard();
                    } else {
                        InteractionSystem.Instance.ClickCard(card);
                    }
                }
                else if(clickedObject.GetComponent<Tile>() != null) {
                    Tile tile = clickedObject.GetComponent<Tile>();
                    if (InteractionSystem.Instance.currentInteraction == null) {
                        //tile.Onclick();
                    } else {
                        InteractionSystem.Instance.ClickZone(tile);
                    }
                }
                else if (clickedObject.GetComponent<TempButton>() != null) {
                    TempButton button = clickedObject.GetComponent<TempButton>();
                    if (InteractionSystem.Instance.currentInteraction == null) {
                        button.Onclick();
                    } else {
                        InteractionSystem.Instance.ClickButton(button);
                    }
                } else {
                    InteractionSystem.Instance.TryCancel();
                }
            }

            PointerEventData pointerData = new PointerEventData(eventSystem);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();

            graphicRaycaster.Raycast(pointerData, results);


            foreach (RaycastResult result in results) {
                if (result.gameObject.GetComponent<TempButton>() != null) {
                    TempButton button = result.gameObject.GetComponent<TempButton>();
                    if (InteractionSystem.Instance.currentInteraction == null) {
                        button.Onclick();
                    } else {
                        InteractionSystem.Instance.ClickButton(button);
                    }
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space)) {

            //CardGameMultiplayer.Instance.SincDrawServerRpc(CardGameManager.Instance.localPlayer.id);
        }
        
    }
}
