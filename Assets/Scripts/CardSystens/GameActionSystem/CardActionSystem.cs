using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class CardActionSystem : MonoBehaviour{


    private void OnEnable() {
        ActionSystem.AttachPerformer<DrawCardGA>(DrawCardPerformer);
        ActionSystem.SubscribeReaction<DrawCardGA>(DrawCardPostReaction, ReactionTiming.POST);
        ActionSystem.AttachPerformer<SummonCardGA>(SummonCardPerformer);
        ActionSystem.AttachPerformer<MoveCardOnFieldGA>(MoveCardOnFieldPerformer);
        ActionSystem.AttachPerformer<SendCardToGYGA>(SendCardToGYPerformer);
        ActionSystem.AttachPerformer<DeclareEffectGA>(DeclareEffectPerformer);
        ActionSystem.AttachPerformer<AttackCardGA>(AttackCardPerformer);
        ActionSystem.AttachPerformer<RemoveStarManaAmountGA>(RemoveStarManaAmountPerformer);
        ActionSystem.AttachPerformer<AttackPlayerGA>(AttackPlayerPerformer);
    }
    private void OnDisable() {
        ActionSystem.DetachPerformer<DrawCardGA>();
        ActionSystem.UnsubscribeReaction<DrawCardGA>(DrawCardPostReaction, ReactionTiming.POST);
        ActionSystem.DetachPerformer<SummonCardGA>();
        ActionSystem.DetachPerformer<MoveCardOnFieldGA>();
        ActionSystem.DetachPerformer<SendCardToGYGA>();
        ActionSystem.DetachPerformer<DeclareEffectGA>();
        ActionSystem.DetachPerformer<AttackCardGA>();
        ActionSystem.DetachPerformer<RemoveStarManaAmountGA>();
        ActionSystem.DetachPerformer<AttackPlayerGA>();
    }

    private IEnumerator DrawCardPerformer(DrawCardGA drawCardGA) {
        List<Card> newListCards = new();

        if (drawCardGA.player == CardGameManager.Instance.player1) {
            for (int i = 0; i < drawCardGA.drawNumber; i++) {
                Card card = CardGameManager.Instance.deck1[0];
                CardGameManager.Instance.deck1.RemoveAt(0);
                CardGameManager.Instance.hand1.Add(card);
                newListCards.Add(card);
            }

        } else {
            for (int i = 0; i < drawCardGA.drawNumber; i++) {
                Card card = CardGameManager.Instance.deck2[0];
                CardGameManager.Instance.deck2.RemoveAt(0);
                CardGameManager.Instance.hand2.Add(card);
                newListCards.Add(card);   
            }
        }
        drawCardGA.cardsDrawed = newListCards;

        if (newListCards[0].Owner == CardGameManager.Instance.localPlayer) {
            ObjectManager.Instance.handPlayer.GetComponent<HandVisual>().ShowDrawedCard(newListCards);
        }

        StartCoroutine(ObjectManager.Instance.handPlayer.GetComponent<HandVisual>().WaitForUpdateCardsPosition(1.5f)); 
        yield return StartCoroutine(ObjectManager.Instance.handOpponent.GetComponent<HandVisual>().WaitForUpdateCardsPosition(1.5f)); 
    }

    private void DrawCardPostReaction(DrawCardGA drawCardGA) {
        
    }

    private IEnumerator SummonCardPerformer(SummonCardGA summonCardGA) {
        Card card = summonCardGA.card;
        Tile tile = summonCardGA.tile;
        if (CardGameManager.Instance.IsCardInHand(card)) {
            StartCoroutine(ObjectManager.Instance.handPlayer.GetComponent<HandVisual>().WaitForUpdateCardsPosition(0.25f));
            StartCoroutine(ObjectManager.Instance.handOpponent.GetComponent<HandVisual>().WaitForUpdateCardsPosition(0.25f));
        }
        if (CardGameManager.Instance.IsCardInDeck(card)) {
            StartCoroutine(ObjectManager.Instance.deckPlayer.GetComponent<DeckVisual>().WaitForUpdateCardsPosition(0.25f));
            StartCoroutine(ObjectManager.Instance.deckOpponent.GetComponent<DeckVisual>().WaitForUpdateCardsPosition(0.25f));
        }
        if (CardGameManager.Instance.IsCardInGy(card)) {
            StartCoroutine(ObjectManager.Instance.gyPlayer.GetComponent<GYVisual>().WaitForUpdateCardsPosition(0.25f));
            StartCoroutine(ObjectManager.Instance.gyOpponent.GetComponent<GYVisual>().WaitForUpdateCardsPosition(0.25f));
        }
        CardGameManager.Instance.RemoveCard(card);
        card.activatedEventsInstance.Clear();
        MonsterCardData monsterData = (MonsterCardData)card.cardData;
        monsterData.movequant = 1;
        monsterData.atkquant = 1;
        card.transform.parent = tile.transform;
        tile.cardOnTile = card;

        ObjectManager.Instance.field.GetComponent<FieldVisual>().ShowSummonedCard(card, tile);
        yield return StartCoroutine(ObjectManager.Instance.field.GetComponent<FieldVisual>().WaitForUpdateCardsPosition(1.0f));
    }

    private IEnumerator MoveCardOnFieldPerformer(MoveCardOnFieldGA moveCardOnFieldGA) {
        Card card = moveCardOnFieldGA.card;
        Tile tile = moveCardOnFieldGA.tile;
        Tile previosTile = CardGameManager.Instance.GetTileWCard(card);
        card.transform.parent = tile.transform;
        MonsterCardData monsterData = (MonsterCardData)card.cardData;
        monsterData.movequant -= 1;
        tile.cardOnTile = card;
        previosTile.cardOnTile = null;

        yield return StartCoroutine(ObjectManager.Instance.field.GetComponent<FieldVisual>().WaitForUpdateCardsPosition(0.1f)); ;
    }

    private IEnumerator SendCardToGYPerformer(SendCardToGYGA sendCardToGYGA) {
        Card card = sendCardToGYGA.card;
        if (CardGameManager.Instance.IsCardInHand(card)) {
            StartCoroutine(ObjectManager.Instance.handPlayer.GetComponent<HandVisual>().WaitForUpdateCardsPosition(0.25f));
            StartCoroutine(ObjectManager.Instance.handOpponent.GetComponent<HandVisual>().WaitForUpdateCardsPosition(0.25f));
        }
        if (CardGameManager.Instance.IsCardInDeck(card)) {
            StartCoroutine(ObjectManager.Instance.deckPlayer.GetComponent<DeckVisual>().WaitForUpdateCardsPosition(0.25f));
            StartCoroutine(ObjectManager.Instance.deckOpponent.GetComponent<DeckVisual>().WaitForUpdateCardsPosition(0.25f));
        }

        CardGameManager.Instance.RemoveCard(card);
        
        if (card.Owner == CardGameManager.Instance.player1) {
            CardGameManager.Instance.gy1.Add(card);
        } else {
            CardGameManager.Instance.gy2.Add(card);
        }

        if (CardGameManager.Instance.localPlayer == card.Owner) {
            ObjectManager.Instance.gyPlayer.GetComponent<GYVisual>().ShowGYCard(card);
        } else {
            ObjectManager.Instance.gyOpponent.GetComponent<GYVisual>().ShowGYCard(card);
        }

        StartCoroutine(ObjectManager.Instance.gyPlayer.GetComponent<GYVisual>().WaitForUpdateCardsPosition(1f));
        yield return StartCoroutine(ObjectManager.Instance.gyOpponent.GetComponent<GYVisual>().WaitForUpdateCardsPosition(1f));

    }

    private IEnumerator AttackCardPerformer(AttackCardGA attackCardGA) {
        Card attackingCard = attackCardGA.attackingCard;
        Card attackedCard = attackCardGA.attackedCard;

        if (CardGameManager.Instance.IsCardInField(attackingCard) && CardGameManager.Instance.IsCardInField(attackedCard)) {
            MonsterCardData attackingCardData = (MonsterCardData)attackingCard.cardData;
            MonsterCardData attackedCardData = (MonsterCardData)attackedCard.cardData;
            int attack = attackingCardData.cardAtk;
            attackedCardData.cardHp -= attack;
            attackedCardData.atkquant -= 1;

            attackedCard.GetComponent<CardVisual>().SetUp(attackedCard);

            if (attackedCardData.cardHp <= 0) {
                CardGameManager.Instance.RemoveCard(attackedCard);
                if (attackedCard.Owner == CardGameManager.Instance.player1) {
                    CardGameManager.Instance.gy1.Add(attackedCard);
                } else {
                    CardGameManager.Instance.gy2.Add(attackedCard);
                }

                if (CardGameManager.Instance.localPlayer == attackedCard.Owner) {
                    ObjectManager.Instance.gyPlayer.GetComponent<GYVisual>().ShowGYCard(attackedCard);
                    StartCoroutine(ObjectManager.Instance.gyPlayer.GetComponent<GYVisual>().WaitForUpdateCardsPosition(1f));
                } else {
                    ObjectManager.Instance.gyOpponent.GetComponent<GYVisual>().ShowGYCard(attackedCard);
                    StartCoroutine(ObjectManager.Instance.gyOpponent.GetComponent<GYVisual>().WaitForUpdateCardsPosition(1f));
                }
                attackedCardData.cardHp = attackedCard.GetCardSO().GetComponent<MonsterCardSO>().hp;
            }
        }

        yield return StartCoroutine(ObjectManager.Instance.field.GetComponent<FieldVisual>().WaitForUpdateCardsPosition(0.1f));

    }

    private IEnumerator RemoveStarManaAmountPerformer(RemoveStarManaAmountGA removeStarManaAmountGA) {
        if (removeStarManaAmountGA.player == CardGameManager.Instance.player1) {
            CardGameManager.Instance.AddPlayerMana(CardGameManager.Instance.player1.id, -removeStarManaAmountGA.startAmount);
        } else {
            CardGameManager.Instance.AddPlayerMana(CardGameManager.Instance.player2.id, -removeStarManaAmountGA.startAmount);
        }
        
        yield return new WaitForSeconds(0.01f);
    }

    private IEnumerator AttackPlayerPerformer(AttackPlayerGA attackPlayerGA) {
        Card card = CardGameManager.Instance.GetCardFromLocalId(attackPlayerGA.cardId);
        MonsterCardData attackingCardData = (MonsterCardData)card.cardData;
        if (attackPlayerGA.player == CardGameManager.Instance.player1) {
            CardGameManager.Instance.AddPlayerLifePoint(CardGameManager.Instance.player1.id, -attackingCardData.cardAtk);
        } else {
            CardGameManager.Instance.AddPlayerLifePoint(CardGameManager.Instance.player2.id, -attackingCardData.cardAtk);
        }
        attackingCardData.atkquant -= 1;

        yield return new WaitForSeconds(0.01f);
    }

    private IEnumerator DeclareEffectPerformer(DeclareEffectGA declareEffectGA) {
        Card card = declareEffectGA.card;
        card.transform.DOMove(card.transform.position + new Vector3(0, 1, 0), 0.25f);

        yield return new WaitForSeconds(0.75f);
        card.transform.DOMove(card.transform.position + new Vector3(0, -1, 0), 0.25f);
    }


    }
