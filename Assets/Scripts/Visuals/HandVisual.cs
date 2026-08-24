using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class HandVisual : MonoBehaviour{

    [SerializeField] private Transform showPosition;
    [SerializeField] private SplineContainer splineContiner;
    public int player;


    public void CardActionSystem_UpdateCards(List<Card> newCards) {
        ShowDrawedCard(newCards);
        StartCoroutine(WaitForUpdateCardsPosition(1.5f));
    }

    public IEnumerator WaitForUpdateCardsPosition(float s) {
        yield return new WaitForSeconds(s);
        UpdateCardsPosition();
    }

    public void ShowDrawedCard(List<Card> cards) {
        float firstCardPosition = showPosition.position.x - (cards.Count - 1) * 1.2f/2;
        for (int i = 0; i < cards.Count; i++) {
            Vector3 position = new Vector3(firstCardPosition + 1.2f * i, showPosition.position.y, showPosition.position.z);
            cards[i].transform.DOMove(position, 0.15f);
            cards[i].transform.DORotate(showPosition.rotation.eulerAngles, 0.15f);
        }
        
    }

    public void UpdateCardsPosition() {
        List<Card> handCards;
        bool isOponent = false;
        if (player == 1) {
           handCards = CardGameManager.Instance.hand1;

            if (CardGameManager.Instance.localPlayer != CardGameManager.Instance.player1) {
                isOponent = true;
            }
        } else {
           handCards = CardGameManager.Instance.hand2;

            if (CardGameManager.Instance.localPlayer != CardGameManager.Instance.player2) {
                isOponent = true;
            }
        }

        float duration = 0.25f;

        if (handCards.Count == 0) return;
        float cardSpacing = 0.1f;
        float firstCardPosition = 0.5f - (handCards.Count - 1) * cardSpacing / 2;
        Spline spline = splineContiner.Spline;
        for (int i = 0; i < handCards.Count; i++) {
            float p = firstCardPosition + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(p);
            Vector3 forward = spline.EvaluateTangent(p);
            Vector3 up = spline.EvaluateUpVector(p);
            Quaternion rotation;
            if (!isOponent) {
                rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            } else {
                rotation = Quaternion.LookRotation(up, Vector3.Cross(-up, forward).normalized);
            }


            handCards[i].transform.parent = transform;
            handCards[i].transform.DOMove(splinePosition + transform.position + 0.01f * i * Vector3.back, duration);
            handCards[i].transform.DORotate(rotation.eulerAngles, duration);
        }
    }
    public void PulUpCard(Card card = null) {
        
        List<Card> handCards;
        if (player == 1) {
           handCards = CardGameManager.Instance.hand1;
        } else {
            handCards = CardGameManager.Instance.hand2;
        }


        float duration = 0.25f;
        UpdateCardsPosition();
        int index = handCards.IndexOf(card);
        if(index != -1) {
            float cardSpacing = 0.1f;
            float firstCardPosition = 0.5f - (handCards.Count - 1) * cardSpacing / 2;
            float p = firstCardPosition + index * cardSpacing;
            Spline spline = splineContiner.Spline;
            Vector3 splinePosition = spline.EvaluatePosition(p);
            handCards[index].transform.DOMove((splinePosition + transform.position + 0.01f * index * Vector3.back) + new Vector3(0, 0.3f, 0.3f), duration);
            handCards[index].transform.DORotate(new Vector3(90, 0, 0), duration);
        }

    }


}
