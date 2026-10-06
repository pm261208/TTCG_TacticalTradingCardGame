using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;
using static UnityEngine.Rendering.GPUSort;

public class ChainSystem : Singleton<ChainSystem> {

    private List<PendingEffect> pendingEffects = new();
    private List<PendingEffect> pendingResponses = new();
    private List<PendingEffect> pendingPassives = new();

    private Stack<ChainLink> currentChain = new();


    public bool resolvingChain = false;
    public bool buildingChain = false;


    // =========================
    // REGISTRO DE TRIGGERS
    // =========================

    public void RegisterPendingEffect(PendingEffect effect) {

        pendingEffects.Add(effect);

        Debug.Log(
            $"Pending Effect Registered: {effect.source.name}"
        );
    }

    public void RegisterPendingResponse(PendingEffect effect) {

        pendingResponses.Add(effect);

    }

    public void RegisterPendingPassive(PendingEffect effect) {

        pendingPassives.Add(effect);

    }

    // =========================
    // PROCESSA OS EFEITOS PASSIVOS
    // =========================

    public IEnumerator ProcessPendingPassiveEffects() {
        
        if (pendingPassives.Count == 0) 
            yield break;
        yield return ResolvePassives();
    }

    // =========================
    // RESOLVE OS EFEITOS PASSIVOS
    // =========================

    private IEnumerator ResolvePassives() {
        buildingChain = true;
        CardGameMultiplayer.Instance.OnChainStateChanged();

        List<PendingEffect> mandatoryEffects = new();
        List<PendingEffect> mandatoryOponentEffects = new();
        List<PendingEffect> optionalEffects = new();
        List<PendingEffect> optionalOponentEffects = new();

        Player turnPlayer = CardGameManager.Instance.turnPlayer;
        Player oponentPlayer = (turnPlayer == CardGameManager.Instance.player1) ? CardGameManager.Instance.player2 : CardGameManager.Instance.player1;
        foreach (var effect in pendingPassives) {

            if (effect.owner == turnPlayer) {
                if (effect.cardEvent.isOptional) {
                    optionalEffects.Add(effect);
                } else {
                    mandatoryEffects.Add(effect);
                }
            } else {
                if (effect.cardEvent.isOptional) {
                    optionalOponentEffects.Add(effect);
                } else {
                    mandatoryOponentEffects.Add(effect);
                }
            }
        }
        pendingPassives.Clear();

        while (mandatoryEffects.Count != 0) {

            yield return mandatoryEffects[0].cardEvent.effects.Execute(mandatoryEffects[0].context);
            CardGameMultiplayer.Instance.ExecuteEventByIdClientRpc(CardGameManager.Instance.GetEventIndex(mandatoryEffects[0].context.Source, mandatoryEffects[0].cardEvent), mandatoryEffects[0].context);
            mandatoryEffects.RemoveAt(0);
            EvaluatePendingEffects(mandatoryEffects);

        }
        while (mandatoryOponentEffects.Count != 0) {

            yield return mandatoryOponentEffects[0].cardEvent.effects.Execute(mandatoryOponentEffects[0].context);
            CardGameMultiplayer.Instance.ExecuteEventByIdClientRpc(CardGameManager.Instance.GetEventIndex(mandatoryOponentEffects[0].context.Source, mandatoryOponentEffects[0].cardEvent), mandatoryOponentEffects[0].context);
            mandatoryOponentEffects.RemoveAt(0);
            EvaluatePendingEffects(mandatoryOponentEffects);
        }
        while (optionalEffects.Count != 0) {

            yield return optionalEffects[0].cardEvent.effects.Execute(optionalEffects[0].context);
            CardGameMultiplayer.Instance.ExecuteEventByIdClientRpc(CardGameManager.Instance.GetEventIndex(optionalEffects[0].context.Source, optionalEffects[0].cardEvent), optionalEffects[0].context);
            optionalEffects.RemoveAt(0);
            EvaluatePendingEffects(optionalEffects);

        }
        while (optionalOponentEffects.Count != 0) {

            yield return optionalOponentEffects[0].cardEvent.effects.Execute(optionalOponentEffects[0].context);
            CardGameMultiplayer.Instance.ExecuteEventByIdClientRpc(CardGameManager.Instance.GetEventIndex(optionalOponentEffects[0].context.Source, optionalOponentEffects[0].cardEvent), optionalOponentEffects[0].context);
            optionalOponentEffects.RemoveAt(0);
            EvaluatePendingEffects(optionalOponentEffects);

        }

        buildingChain = false;
        CardGameMultiplayer.Instance.OnChainStateChanged();
    }

    // =========================
    // EVENTO TERMINOU
    // =========================

    public void ProcessPendingEffects() {

        if (pendingEffects.Count == 0) {
            StartCoroutine(ProcessPendingPassiveEffects());
            return;
        }
            
        StartCoroutine(BuildChain());
    }

    // =========================
    // MONTA CHAIN INICIAL
    // =========================

    private IEnumerator BuildChain() {
        buildingChain = true;
        CardGameMultiplayer.Instance.OnChainStateChanged();
        pendingResponses.Clear();

        List<PendingEffect> mandatoryEffects = new();
        List<PendingEffect> mandatoryOponentEffects = new();
        List<PendingEffect> optionalEffects = new();
        List<PendingEffect> optionalOponentEffects = new();

        Player turnPlayer = CardGameManager.Instance.turnPlayer;
        Player oponentPlayer = (turnPlayer == CardGameManager.Instance.player1) ? CardGameManager.Instance.player2 : CardGameManager.Instance.player1;
        foreach (var effect in pendingEffects) {

            if(effect.owner == turnPlayer) {
                if (effect.cardEvent.isOptional) {
                    Debug.Log("Optional Effect");
                    optionalEffects.Add(effect);
                } else {
                    Debug.Log("Mandatory Effect");
                    mandatoryEffects.Add(effect);
                }
            } else {
                if (effect.cardEvent.isOptional) {
                    Debug.Log("Optional Enemy Effect");
                    optionalOponentEffects.Add(effect);
                } else {
                    Debug.Log("Mandatory Enemy Effect");
                    mandatoryOponentEffects.Add(effect);
                }
            }
        }
        pendingEffects.Clear();

        while (mandatoryEffects.Count != 0) {
            PendingEffect mandatoryEffect;
            List<Card> cards = mandatoryEffects.Select(e => e.source).ToList();
            
            CardGameMultiplayer.Instance.SendResponsesClientRpc(cards[0].Owner.id, CardGameManager.Instance.GetIdListFromCardList(cards).ToArray(), false);
            Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

            yield return new WaitUntil(() => task.IsCompleted);

            EffectContext newContext = task.Result;
            if (newContext.Source == 0) break;
            
            mandatoryEffect = mandatoryEffects.FirstOrDefault(e => e.source == CardGameManager.Instance.GetCardFromLocalId(newContext.Source));
            mandatoryEffects.Remove(mandatoryEffect);

            yield return ActivateCardEffect(mandatoryEffect.cardEvent, newContext);

            AddChainLink(mandatoryEffect);

            EvaluatePendingEffects(mandatoryEffects);
            yield return Responses(turnPlayer);
            EvaluatePendingEffects(mandatoryEffects);

        }
        while (mandatoryOponentEffects.Count != 0) {
            PendingEffect mandatoryEffect;
            List<Card> cards = mandatoryOponentEffects.Select(e => e.source).ToList();

            CardGameMultiplayer.Instance.SendResponsesClientRpc(cards[0].Owner.id, CardGameManager.Instance.GetIdListFromCardList(cards).ToArray(), false);
            Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

            yield return new WaitUntil(() => task.IsCompleted);

            EffectContext newContext = task.Result;
            if (newContext.Source == 0) break;

            mandatoryEffect = mandatoryOponentEffects.FirstOrDefault(e => e.source == CardGameManager.Instance.GetCardFromLocalId(newContext.Source));
            mandatoryOponentEffects.Remove(mandatoryEffect);

            yield return ActivateCardEffect(mandatoryEffect.cardEvent, newContext);

            AddChainLink(mandatoryEffect);

            EvaluatePendingEffects(mandatoryEffects);
            yield return Responses(oponentPlayer);
            EvaluatePendingEffects(mandatoryEffects);
        }
        while (optionalEffects.Count != 0) {
            List<Card> cards = optionalEffects.Select(e => e.source).ToList();

            CardGameMultiplayer.Instance.SendResponsesClientRpc(cards[0].Owner.id, CardGameManager.Instance.GetIdListFromCardList(cards).ToArray(), true);
            Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

            yield return new WaitUntil(() => task.IsCompleted);

            EffectContext newContext = task.Result;
            if (newContext.Source == 0) break;

            PendingEffect optionalEffect = optionalEffects.FirstOrDefault(e => e.source == CardGameManager.Instance.GetCardFromLocalId(newContext.Source));
            if (optionalEffect != null) { 
                optionalEffects.Remove(optionalEffect);
                
                yield return ActivateCardEffect(optionalEffect.cardEvent, newContext);

                AddChainLink(optionalEffect);

                EvaluatePendingEffects(optionalEffects);
                yield return Responses(turnPlayer);
                EvaluatePendingEffects(optionalEffects);
            } else {
                optionalEffects.Clear();
            }

        }
        while (optionalOponentEffects.Count != 0) {
            List<Card> cards = optionalOponentEffects.Select(e => e.source).ToList();

            CardGameMultiplayer.Instance.SendResponsesClientRpc(cards[0].Owner.id, CardGameManager.Instance.GetIdListFromCardList(cards).ToArray(), true);
            Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

            yield return new WaitUntil(() => task.IsCompleted);

            EffectContext newContext = task.Result;
            if (newContext.Source == 0) break;

            PendingEffect optionalEffect = optionalOponentEffects.FirstOrDefault(e => e.source == CardGameManager.Instance.GetCardFromLocalId(newContext.Source));
            if (optionalEffect != null) {
                optionalOponentEffects.Remove(optionalEffect);

                yield return ActivateCardEffect(optionalEffect.cardEvent, newContext);

                AddChainLink(optionalEffect);

                EvaluatePendingEffects(optionalOponentEffects);
                yield return Responses(oponentPlayer);
                EvaluatePendingEffects(optionalOponentEffects);
            } else {
                optionalOponentEffects.Clear();
            }

        }

        buildingChain = false;
        CardGameMultiplayer.Instance.OnChainStateChanged();
        StartCoroutine(ResolveChain());
    }

    public IEnumerator ActivateIgnition(Card source, CardEvent cardEvent, EffectContext context, Player owner) {
        if (CardGameMultiplayer.Instance.IsServer) {
            buildingChain = true;
            CardGameMultiplayer.Instance.OnChainStateChanged();
        }
        
        pendingResponses.Clear();
        Player oponentPlayer = (owner == CardGameManager.Instance.player1) ? CardGameManager.Instance.player2 : CardGameManager.Instance.player1;

        PendingEffect optionalEffect = new() { 
            source = source,
            cardEvent = cardEvent,
            owner = owner,
            context = context,
        };

        if (cardEvent.cost != null) {
            //ExecuteIgnitionEffect
            yield return cardEvent.cost.Execute(context);
        }
        AddChainLink(optionalEffect);     

        CardGameManager.Instance.cardEventLogs.Add(new EventLog { 
            sourceCardId = source.cardId, 
            instanceId = source.instanceId,
            eventId = CardGameManager.Instance.GetEventIndex(context.Source, cardEvent), 
            turn = CardGameManager.Instance.turnCount });

        if(cardEvent.effectType != EffectTypes.doesNotStartChain) yield return DeclareCardEffect(source);


        yield return Responses(oponentPlayer);

        if (CardGameMultiplayer.Instance.IsServer) {
            buildingChain = false;
            CardGameMultiplayer.Instance.OnChainStateChanged();
        }
        StartCoroutine(ResolveChain());
    }

    public void AddChainLink(PendingEffect effect) {

        currentChain.Push(
                    new ChainLink {
                        linkNumber = currentChain.Count+1,
                        source = effect.source,
                        cardEvent = effect.cardEvent,
                        context = effect.context,
                        owner = effect.owner
                    }
                );


        Debug.Log(
                $"Chain Link {currentChain.Count}: {effect.source.name}"
        );

    }

    private void EvaluatePendingEffects(List<PendingEffect> effects) {
        for (int i = 0; i < effects.Count; i++) {
            if (!effects[i].source.EvaluateEvent(effects[i].cardEvent)) {
                effects.RemoveAt(i);
            }else if (effects[i].cardEvent.effectType == EffectTypes.response || effects[i].cardEvent.effectType == EffectTypes.ignitionResponse) {
                effects.RemoveAt(i);
            }
        }
    }

    private IEnumerator Responses(Player player) {

        while (pendingResponses.Count != 0) {
            List<PendingEffect> optionalEffects = new();
            List<PendingEffect> optionalOponentEffects = new();
            List<PendingEffect> mandatoryEffects = new();
            List<PendingEffect> mandatoryOponentEffects = new();


            foreach (var effect in pendingResponses) {

                if (effect.owner == player) {
                    if (effect.cardEvent.isOptional) {
                        optionalEffects.Add(effect);
                    } else {
                        mandatoryEffects.Add(effect);
                    }
                        
                } else {
                    if (effect.cardEvent.isOptional) {
                        optionalOponentEffects.Add(effect);
                    } else {
                        mandatoryOponentEffects.Add(effect);
                    }

                }
            }
            pendingResponses.Clear();
            
            if (mandatoryOponentEffects.Count != 0) {
                List<Card> cards = mandatoryOponentEffects.Select(e => e.source).ToList();

                CardGameMultiplayer.Instance.SendResponsesClientRpc(cards[0].Owner.id, CardGameManager.Instance.GetIdListFromCardList(cards).ToArray(), false);
                Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

                yield return new WaitUntil(() => task.IsCompleted);

                EffectContext newContext = task.Result;
                if (newContext.Source == 0) continue;

                PendingEffect mandatoryEffect = mandatoryOponentEffects.FirstOrDefault(e => e.source == CardGameManager.Instance.GetCardFromLocalId(newContext.Source));
                if (mandatoryEffect != null) {
                    //optionalOponentEffects.Remove(optionalEffect);


                    yield return ActivateCardEffect(mandatoryEffect.cardEvent, newContext);

                    AddChainLink(mandatoryEffect);
                    player = (mandatoryEffect.owner == CardGameManager.Instance.player1) ? CardGameManager.Instance.player2 : CardGameManager.Instance.player1;
                    continue;
                } 
            }
            if (mandatoryEffects.Count != 0) {
                List<Card> cards = mandatoryEffects.Select(e => e.source).ToList();

                CardGameMultiplayer.Instance.SendResponsesClientRpc(cards[0].Owner.id, CardGameManager.Instance.GetIdListFromCardList(cards).ToArray(), false);
                Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

                yield return new WaitUntil(() => task.IsCompleted);

                EffectContext newContext = task.Result;
                if (newContext.Source == 0) continue;

                PendingEffect mandatoryEffect = mandatoryEffects.FirstOrDefault(e => e.source == CardGameManager.Instance.GetCardFromLocalId(newContext.Source));
                if (mandatoryEffect != null) {
                    //optionalEffects.Remove(optionalEffect);

                    yield return ActivateCardEffect(mandatoryEffect.cardEvent, newContext);

                    AddChainLink(mandatoryEffect);
                    player = (mandatoryEffect.owner == CardGameManager.Instance.player1) ? CardGameManager.Instance.player2 : CardGameManager.Instance.player1;
                    continue;
                }
            }
            if (optionalOponentEffects.Count != 0) {
                List<Card> cards = optionalOponentEffects.Select(e => e.source).ToList();

                CardGameMultiplayer.Instance.SendResponsesClientRpc(cards[0].Owner.id, CardGameManager.Instance.GetIdListFromCardList(cards).ToArray(), true);
                Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

                yield return new WaitUntil(() => task.IsCompleted);

                EffectContext newContext = task.Result;
                if (newContext.Source == 0) continue;

                PendingEffect optionalEffect = optionalOponentEffects.FirstOrDefault(e => e.source == CardGameManager.Instance.GetCardFromLocalId(newContext.Source));
                if (optionalEffect != null) {
                    //optionalOponentEffects.Remove(optionalEffect);


                    yield return ActivateCardEffect(optionalEffect.cardEvent, newContext);

                    AddChainLink(optionalEffect);
                    player = (optionalEffect.owner == CardGameManager.Instance.player1) ? CardGameManager.Instance.player2 : CardGameManager.Instance.player1;
                    continue;
                } 
            }
            if (optionalEffects.Count != 0) {
                List<Card> cards = optionalEffects.Select(e => e.source).ToList();

                CardGameMultiplayer.Instance.SendResponsesClientRpc(cards[0].Owner.id, CardGameManager.Instance.GetIdListFromCardList(cards).ToArray(), true);
                Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

                yield return new WaitUntil(() => task.IsCompleted);

                EffectContext newContext = task.Result;
                if (newContext.Source == 0) continue;

                PendingEffect optionalEffect = optionalEffects.FirstOrDefault(e => e.source == CardGameManager.Instance.GetCardFromLocalId(newContext.Source));
                if (optionalEffect != null) {
                    //optionalEffects.Remove(optionalEffect);

                    yield return ActivateCardEffect(optionalEffect.cardEvent, newContext);

                    AddChainLink(optionalEffect);
                    player = (optionalEffect.owner == CardGameManager.Instance.player1) ? CardGameManager.Instance.player2 : CardGameManager.Instance.player1;
                    continue;
                }
            }
        }


    }

    private IEnumerator DeclareCardEffect(Card card) {

        if(card.isSet) yield return ActionSystem.Instance.Perform(new UnflipCardGA(card));
        yield return ActionSystem.Instance.Perform(new DeclareEffectGA(card));

        EffectContext newContext = new() {
            Source = card.cardId,
        };

        EventSystem.Instance.RaiseEvent(TriggerType.OnEffectActivate, newContext);

    }

    // =========================
    // RESOLUÇÃO
    // =========================

    private IEnumerator ResolveChain() {

        if (resolvingChain) {
            Debug.LogError("Still Resolving Chain");
            yield break;
        }

        if (CardGameMultiplayer.Instance.IsServer) {
            resolvingChain = true;
            CardGameMultiplayer.Instance.OnChainStateChanged();
        }

        while (currentChain.Count > 0) {

            ChainLink link = currentChain.Pop();


            Debug.Log(
                $"Resolving Chain Link {link.linkNumber}"
            );

            yield return StartCoroutine(
                ResolveEffect(link)
            );
        }

        if (CardGameMultiplayer.Instance.IsServer) {
            resolvingChain = false;
            CardGameMultiplayer.Instance.OnChainStateChanged();
        }

        Debug.Log("Chain Finished");
        yield return ProcessPendingPassiveEffects();
        EventSystem.Instance.FinishEvent();
    }

    // =========================
    // EXECUTA EFFECT NODES
    // =========================
    public IEnumerator ActivateCardEffect(CardEvent cardEvent, EffectContext context) {
        Card source = CardGameManager.Instance.GetCardFromLocalId(context.Source);
        int eventIndex = CardGameManager.Instance.GetEventIndex(context.Source, cardEvent);

        if (NetworkManager.Singleton.IsServer) {

            CardGameMultiplayer.Instance.SincActivateCardEffectServer(eventIndex, context);
        }

        if (cardEvent.cost != null) {
            yield return cardEvent.cost.Execute(new EffectContext() { Source = source.cardId, eventData = context.eventData });
        }
        if (cardEvent.effectType != EffectTypes.doesNotStartChain) yield return DeclareCardEffect(source);

        CardGameManager.Instance.cardEventLogs.Add(new EventLog { 
            sourceCardId = source.cardId, 
            instanceId = source.instanceId,
            eventId = eventIndex, 
            turn = CardGameManager.Instance.turnCount });


    }

    public IEnumerator ResolveEffectClient(CardEvent cardEvent, EffectContext context) {
        yield return cardEvent.effects.Execute(context);

        Card card = CardGameManager.Instance.GetCardFromLocalId(context.Source);
        if ((card.cardType == CardType.Spell || card.cardType == CardType.Trap) && !card.isSet && cardEvent != CardGameManager.Instance.placeSpellTrapCardEvent)
            yield return StartCoroutine(ActionSystem.Instance.Perform(
                new SendCardToGYGA(card)
        ));
        CardGameManager.Instance.UpdateCardsBorderVisual();
    }

    private IEnumerator ResolveEffect(ChainLink link) {

        if (NetworkManager.Singleton.IsServer) {
            CardGameMultiplayer.Instance.SincResolveEffectServer(CardGameManager.Instance.GetEventIndex(link.source.cardId, link.cardEvent), link.context);
            yield return link.cardEvent.effects.Execute(link.context);

            if ((link.source.cardType == CardType.Spell || link.source.cardType == CardType.Trap) && !link.source.isSet && link.cardEvent != CardGameManager.Instance.placeSpellTrapCardEvent)
                yield return StartCoroutine(ActionSystem.Instance.Perform(
                    new SendCardToGYGA(link.source)
            ));
            CardGameManager.Instance.UpdateCardsBorderVisual();
        } 
    }
    
}