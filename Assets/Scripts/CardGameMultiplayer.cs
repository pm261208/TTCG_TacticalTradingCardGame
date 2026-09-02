using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CardGameMultiplayer : NetworkBehaviour {

    public const int MAX_PLAYER_AMOUNT = 3;
    public const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";

    public static CardGameMultiplayer Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler<OnLoadEventCompletedEventArgs> OnLoadEventCompleted;

    public class OnLoadEventCompletedEventArgs : EventArgs {
        public string sceneName;
    }

    private NetworkList<PlayerData> playerDataNetworkList;
    private NetworkVariable<ulong> turnPlayerId = new(0UL);
    private int setPlayers;

    private string playerName;
    private TaskCompletionSource<EffectContext> _tcs =
    new(TaskCreationOptions.RunContinuationsAsynchronously);

    [SerializeField] private DeckSO cardSODatabase; 

    private void Awake() {
        Instance = this;

        playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));

        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    }

    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

    private void CardGameManager_OnServerMatchDataSet(object sender, EventArgs e) {
        SetPlayerClientRpc(
            CardGameManager.Instance.player1.id,
            CardGameManager.Instance.player1.playerName,
            GetCardSOIndexListFromCardSOList(CardGameManager.Instance.deck1SO.Deck).ToArray(),
            GetCardIdList(CardGameManager.Instance.deck1).ToArray(),
            CardGameManager.Instance.player2.id,
            CardGameManager.Instance.player2.playerName,
            GetCardSOIndexListFromCardSOList(CardGameManager.Instance.deck2SO.Deck).ToArray(),
            GetCardIdList(CardGameManager.Instance.deck2).ToArray()
        );
           
    }

    [Rpc(SendTo.Server)]
    public void PlayerMatchDataSincServerRpc() {
        setPlayers += 1;
        if (setPlayers == MAX_PLAYER_AMOUNT-1) {
            CardGameManager.Instance.StartMatchT();
            PlayerSetClientRpc();
            GenerateTurnPlayerServerRpc();
        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerSetClientRpc() {
        CardGameManager.Instance.StartMatchT();     
    }

    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent) {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartServer() {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        CardGameManager.Instance.OnPlayerSet += CardGameManager_OnServerMatchDataSet;
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += NetworkManager_OnLoadEventCompleted;
    }


    public void StartClient() {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void TurnPlayerId_OnValueChaged(ulong previousValue, ulong newValue) {
        CardGameManager.Instance.ChangeTurn(newValue);
    }

    private void NetworkManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut) {
        OnLoadEventCompleted?.Invoke(this, new OnLoadEventCompletedEventArgs {
            sceneName = sceneName,
        });
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        response.Approved = true;
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId) {
        playerDataNetworkList.Add(new PlayerData {
            clientId = clientId,
        });
        if (playerDataNetworkList.Count >= 2) {
            Debug.Log("BeginMatch");
            Loader.LoadNetwork(Loader.Scene.DuelScene);
        }
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId) {
        for (int i = 0; i < playerDataNetworkList.Count; i++) {
            PlayerData playerData = playerDataNetworkList[i];
            if (playerData.clientId == clientId) {
                //Disconected
                playerDataNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId) {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
        SetPlayerServerRpc(GetPlayerName(), GetCardSOIndexListFromCardSOList(CardGameManager.Instance.deck1SO.Deck).ToArray());
    }

    public void SincCardEvent(int eventIndex, EffectContext ctx) {
        if (NetworkManager.Singleton.IsClient) {
            SincCardEventWithServerRpc(eventIndex, ctx);
        }
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerNameServerRpc(string playerName, RpcParams rpcParams = default) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(rpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerName = playerName;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerIdServerRpc(string playerId, RpcParams rpcParams = default) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(rpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        playerData.playerId = playerId;
        playerDataNetworkList[playerDataIndex] = playerData;
    }

    [Rpc(SendTo.Server)]
    private void SetPlayerServerRpc(string playerName, int[] playerDeck, RpcParams rpcParams = default) {
        int playerDataIndex = GetPlayerDataIndexFromClientId(rpcParams.Receive.SenderClientId);
        PlayerData playerData = playerDataNetworkList[playerDataIndex];

        if (CardGameManager.Instance.player1 == null) {
            CardGameManager.Instance.player1 = new Player() {
                id = playerData.clientId,
                playerName = playerName,
                playerData = playerData,
                playerDeck = ScriptableObject.CreateInstance<DeckSO>(),
            };
            CardGameManager.Instance.player1.playerDeck.Deck = GetCardSOListFromIndexList(new List<int>(playerDeck));
        } else if (CardGameManager.Instance.player2 == null) {
            CardGameManager.Instance.player2 = new Player() {
                id = playerData.clientId,
                playerName = playerName,
                playerData = playerData,
                playerDeck = ScriptableObject.CreateInstance<DeckSO>(),
            };
            CardGameManager.Instance.player2.playerDeck.Deck = GetCardSOListFromIndexList(new List<int>(playerDeck));
        }

        CardGameManager.Instance.localPlayer = new Player() { 
            id = 0
        };
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetPlayerClientRpc(ulong player1Id, string player1Name, int[] player1Deck, int[] player1DeckIds, ulong player2Id, string player2Name, int[] player2Deck, int[] player2DeckIds) {
        DeckSO deck1SO = ScriptableObject.CreateInstance<DeckSO>();
        DeckSO deck2SO = ScriptableObject.CreateInstance<DeckSO>();
        deck1SO.Deck = GetCardSOListFromIndexList(new List<int>(player1Deck));
        deck2SO.Deck = GetCardSOListFromIndexList(new List<int>(player2Deck));

        CardGameManager.Instance.player1 = new Player { id = player1Id, playerName = player1Name, playerDeck = deck1SO };
        CardGameManager.Instance.player2 = new Player { id = player2Id, playerName = player2Name, playerDeck = deck2SO };
        CardGameManager.Instance.deck1SO = deck1SO;
        CardGameManager.Instance.deck2SO = deck2SO;

        Debug.Log("setClient");
        CardGameManager.Instance.SetMatchDataClient();
        CardGameManager.Instance.SincDecks(new List<int>(player1DeckIds), new List<int>(player2DeckIds));
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SincActionNodeWithClientRpc(int eventId, EffectContext ctx) {

        ActivateEventById(eventId, ctx);
    }

    [Rpc(SendTo.Server)]
    public void SincCardEventWithServerRpc(int eventId, EffectContext ctx) {

        //fala para o servidor qual evento executar
        ActivateEventById(eventId, ctx);

        SincActionNodeWithClientRpc(eventId, ctx);
    }

    [Rpc(SendTo.Server)]
    public void SendInteractionResultServerRpc(EffectContext ctx) {
        _tcs?.TrySetResult(ctx);
        SendInteractionResultClientRpc(ctx);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SendInteractionResultClientRpc(EffectContext ctx) {

        _tcs?.TrySetResult(ctx);
    }

    public Task<EffectContext> WaitForNewContext() {
        _tcs = new TaskCompletionSource<EffectContext>();
        return _tcs.Task;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SendResponsesClientRpc(ulong clientId, int[] cardIds, bool isCancelable) {

        if (CardGameManager.Instance.localPlayer.id == clientId) {

            StartCoroutine(SelectCardWindownInteraction(cardIds, isCancelable));
        }
    }

    private IEnumerator SelectCardWindownInteraction(int[] cardIds, bool isCancelable) { 

        List<Card> cards = CardGameManager.Instance.GetCardListFromLocalIdList(new List<int>(cardIds));
        var interaction = new SelectCardWindowInteraction(cards, isCancelable);
        InteractionSystem.Instance.StartInteraction(interaction);
        yield return interaction.WaitForFinish();
        if (interaction.selectedCard != null) {
            EffectContext context = new(){ Source = interaction.selectedCard.cardId };
            SendInteractionResultServerRpc(context);
        } else {
            SendInteractionResultServerRpc(new());
        }

    }

    public void SincActivateCardEffectServer(int cardEventId, EffectContext context) {

        Debug.Log("SERVER ActivateEventId: " + cardEventId);
        NewSincActivateCardEffectClientRpc(cardEventId, context);
    }


    public void SincResolveEffectServer(int cardEventId, EffectContext context) {

        Debug.Log("SERVER ResolveEventId: " + cardEventId + " ResolvedCardID: "+ context.Source);
        NewSincResolveEffectClientRpc(cardEventId, context);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void NewSincActivateCardEffectClientRpc(int cardEventId, EffectContext context) {

        CardEvent cardEvent = CardGameManager.Instance.GetCardFromLocalId(context.Source).GetCardSO().events[cardEventId];
        StartCoroutine(ChainSystem.Instance.ActivateCardEffect(cardEvent, context));
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void NewSincResolveEffectClientRpc(int cardEventId, EffectContext context) {

        CardEvent cardEvent = CardGameManager.Instance.GetEventById(cardEventId, CardGameManager.Instance.GetCardFromLocalId(context.Source));
        Debug.Log("CLIENT ResolveEventId: " + cardEventId + " ResolvedCardID: " + context.Source);
        StartCoroutine(ChainSystem.Instance.ResolveEffectClient(cardEvent, context));
    }

    [Rpc(SendTo.Server)]
    public void ChangeTurnPlayerServerRpc() {

        turnPlayerId.Value = (turnPlayerId.Value == CardGameManager.Instance.player1.id) ? CardGameManager.Instance.player2.id : CardGameManager.Instance.player1.id;
    }

    [Rpc(SendTo.Server)]
    public void GenerateTurnPlayerServerRpc() {
        ulong initialPlayer = CardGameManager.Instance.player1.id;

        turnPlayerId.Value = initialPlayer;
        turnPlayerId.OnValueChanged += TurnPlayerId_OnValueChaged;
        GenerateTurnPlayerClientRpc(initialPlayer);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void GenerateTurnPlayerClientRpc(ulong playerId) {

        turnPlayerId.OnValueChanged += TurnPlayerId_OnValueChaged;
        CardGameManager.Instance.DefineInitialTurnPlayer(playerId);
    }

    public void OnChainStateChanged() {
        ChainSystem_OnChainStateChangedClientRpc(ChainSystem.Instance.buildingChain, ChainSystem.Instance.resolvingChain);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ChainSystem_OnChainStateChangedClientRpc(bool buildingChain, bool resolvingChain) {
        ChainSystem.Instance.buildingChain = buildingChain;
        ChainSystem.Instance.resolvingChain = resolvingChain;
    }

    public bool IsPlayerIndexConnected(int playerIndex) {
        return playerIndex < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex) {
        return playerDataNetworkList[playerIndex];
    }

    public int GetPlayerDataIndexFromClientId(ulong clientId) {
        for (int i = 0; i < playerDataNetworkList.Count; i++) {
            if (playerDataNetworkList[i].clientId == clientId) return i;
        }
        return -1;
    }
    public PlayerData GetPlayerDataFromClientId(ulong clientId) {
        foreach (PlayerData playerData in playerDataNetworkList) {
            if (playerData.clientId == clientId) return playerData;
        }
        return default;
    }

    public PlayerData GetPlayerData() {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public string GetPlayerName() {
        return playerName;
    }

    public void SetPlayerName(string playerName) {
        this.playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }
    //temp
    public int GetPlayerAmount() {
        return playerDataNetworkList.Count;
    }

    public int GetCardSOIndex(CardSO cardSO) {
        return cardSODatabase.Deck.IndexOf(cardSO);
    }

    public CardSO GetCardSOFromIndex(int cardSOIndex) {
        return cardSODatabase.Deck[cardSOIndex];
    }
    public List<int> GetCardSOIndexListFromCardSOList(List<CardSO> cardSOList) {
        List<int> indexList = new();
        foreach (CardSO cardSO in cardSOList) {
            indexList.Add(GetCardSOIndex(cardSO));
        }
        return indexList;
    }

    public List<CardSO> GetCardSOListFromIndexList(List<int> cardSOIndexList) {
        List<CardSO> cardSOList = new();
        foreach (int index in cardSOIndexList) {
            cardSOList.Add(GetCardSOFromIndex(index));
        }
        return cardSOList;
    }

    public List<int> GetCardIdList(List<Card> cardList) {
        List<int> idList = new();
        foreach (Card card in cardList) {
            idList.Add(card.cardId);
        }
        return idList;
    }

    public CardSO GetCardSOFromCardId(int cardId) {
        foreach(CardSO card in cardSODatabase.Deck) {
            if (card.id == cardId) {
                return card;
            }
        }
        return null;
    }

    private void ActivateEventById(int eventId, EffectContext ctx) {
        CardEvent cardEvent = CardGameManager.Instance.GetEventById(eventId, CardGameManager.Instance.GetCardFromLocalId(ctx.Source));

        StartCoroutine(ChainSystem.Instance.ActivateIgnition(CardGameManager.Instance.GetCardFromLocalId(ctx.Source), cardEvent, ctx, CardGameManager.Instance.GetCardFromLocalId(ctx.Source).Owner));
    }

    [Rpc(SendTo.Server)]
    public void SincDrawServerRpc(ulong playerId, int drawNumber) {

        EffectContext context = new() { Owner = playerId };
        DrawCardNode drawCard = new(){ drawNumber = drawNumber };
        StartCoroutine(drawCard.Execute(context));
        SincDrawClientRpc(playerId, drawNumber);
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void SincDrawClientRpc(ulong playerId, int drawNumber) {

        EffectContext context = new() { Owner = playerId};
        DrawCardNode drawCard = new() { drawNumber = drawNumber };
        StartCoroutine(drawCard.Execute(context));
    }
}
