using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class ChainSystem : Singleton<ChainSystem> {

    private List<PendingEffect> pendingEffects = new();
    private List<PendingEffect> pendingResponses = new();

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

    // =========================
    // EVENTO TERMINOU
    // =========================

    public void ProcessPendingEffects() {

        if (pendingEffects.Count == 0)
            return;
        StartCoroutine(BuildChain());
    }

    // =========================
    // MONTA CHAIN INICIAL
    // =========================

    private IEnumerator BuildChain() {
        buildingChain = true;
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
            EvaluatePendingEffects(mandatoryOponentEffects);
            yield return Responses(oponentPlayer);

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
            } else {
                optionalOponentEffects.Clear();
            }

        }

        buildingChain = false;
        StartCoroutine(ResolveChain());
    }

    public IEnumerator ActivateIgnition(Card source, CardEvent cardEvent, EffectContext context, Player owner) {
        buildingChain = true;
        pendingResponses.Clear();
        Player oponentPlayer = (owner == CardGameManager.Instance.player1) ? CardGameManager.Instance.player2 : CardGameManager.Instance.player1;

        PendingEffect optionalEffect = new PendingEffect();
        optionalEffect.source = source;
        optionalEffect.cardEvent = cardEvent;
        optionalEffect.context = context;
        optionalEffect.owner = owner;

        if (cardEvent.cost != null) {
            //ExecuteIgnitionEffect
            yield return cardEvent.cost.Execute(context);
        }
        AddChainLink(optionalEffect);     

        CardGameManager.Instance.cardEventLogs.Add(new EventLog { 
            sourceCardId = source.cardId, 
            eventId = CardGameManager.Instance.GetEventIndex(context.Source, cardEvent), 
            turn = CardGameManager.Instance.turnCount });
        source.activatedEventsInstance.Add(CardGameManager.Instance.GetEventIndex(context.Source, cardEvent));

        if(cardEvent.effectType != EffectTypes.doesNotStartChain) yield return DeclareCardEffect(source);


        yield return Responses(oponentPlayer);

        buildingChain = false;
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
            }
        }
    }

    private IEnumerator Responses(Player player) {

        while (pendingResponses.Count != 0) {
            List<PendingEffect> optionalEffects = new();
            List<PendingEffect> optionalOponentEffects = new();

            foreach (var effect in pendingResponses) {

                if (effect.owner == player) {
                    optionalEffects.Add(effect);
                } else {
                    optionalOponentEffects.Add(effect);
                }
            }
            pendingResponses.Clear();
            if (optionalOponentEffects.Count != 0) {
                List<Card> cards = optionalOponentEffects.Select(e => e.source).ToList();

                CardGameMultiplayer.Instance.SendResponsesClientRpc(cards[0].Owner.id, CardGameManager.Instance.GetIdListFromCardList(cards).ToArray(), false);
                Task<EffectContext> task = CardGameMultiplayer.Instance.WaitForNewContext();

                yield return new WaitUntil(() => task.IsCompleted);

                EffectContext newContext = task.Result;
                if (newContext.Source == 0) break;

                PendingEffect optionalEffect = optionalOponentEffects.FirstOrDefault(e => e.source == CardGameManager.Instance.GetCardFromLocalId(newContext.Source));
                if (optionalEffect != null) {
                    optionalOponentEffects.Remove(optionalEffect);

                    yield return ActivateCardEffect(optionalEffect.cardEvent, newContext);

                    AddChainLink(optionalEffect);
                    player = (optionalEffect.owner == CardGameManager.Instance.player1) ? CardGameManager.Instance.player2 : CardGameManager.Instance.player1;
                    break;
                } 
            }
            if (optionalEffects.Count != 0) {
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
                    player = (optionalEffect.owner == CardGameManager.Instance.player1) ? CardGameManager.Instance.player2 : CardGameManager.Instance.player1;
                    break;
                }
            }
        }


    }

    private IEnumerator DeclareCardEffect(Card card) {

        yield return ActionSystem.Instance.Perform(new DeclareEffectGA(card));

        EffectContext newContext = new EffectContext();
        newContext.Source = card.cardId;

        EventSystem.Instance.RaiseEvent(TriggerType.OnEffectActvate, newContext);

    }

    // =========================
    // RESOLUÇÃO
    // =========================

    private IEnumerator ResolveChain() {

        if (resolvingChain)
            yield break;

        resolvingChain = true;

        while (currentChain.Count > 0) {

            ChainLink link = currentChain.Pop();

            Debug.Log(
                $"Resolving Chain Link {link.linkNumber}"
            );

            yield return StartCoroutine(
                ResolveEffect(link)
            );
        }

        resolvingChain = false;

        Debug.Log("Chain Finished");
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
            eventId = eventIndex, 
            turn = CardGameManager.Instance.turnCount });

        source.activatedEventsInstance.Add(CardGameManager.Instance.GetEventIndex(context.Source, cardEvent));


    }

    public IEnumerator ResolveEffectClient(CardEvent cardEvent, EffectContext context) {
        yield return cardEvent.effects.Execute(context);

        if(CardGameManager.Instance.GetCardFromLocalId(context.Source).cardType == CardType.Spell)
            yield return StartCoroutine(ActionSystem.Instance.Perform(
                new SendCardToGYGA(CardGameManager.Instance.GetCardFromLocalId(context.Source))
        ));
        CardGameManager.Instance.UpdateCardsBorderVisual();
    }

    private IEnumerator ResolveEffect(ChainLink link) {

        if (NetworkManager.Singleton.IsServer) {
            CardGameMultiplayer.Instance.SincResolveEffectServer(CardGameManager.Instance.GetEventIndex(link.source.cardId, link.cardEvent), link.context);
            yield return link.cardEvent.effects.Execute(link.context);

            if (link.source.cardType == CardType.Spell)
                yield return StartCoroutine(ActionSystem.Instance.Perform(
                    new SendCardToGYGA(link.source)
            ));
            CardGameManager.Instance.UpdateCardsBorderVisual();
            EventSystem.Instance.FinishEvent();
        } 
    }
    
}