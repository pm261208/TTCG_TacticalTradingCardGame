using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay; // Necessário para RelayServerData

public class DedicatedServerManager : MonoBehaviour {

    private const string REGISTERSERVER_URL = "http://localhost:3000/api/register-server";
    private const string CLOSESERVER_URL = "http://localhost:3000/api/server-closed";

    // Capacidade máxima de jogadores por partida (excluindo o servidor)
    private const int MAX_PLAYERS = 2;

    [System.Serializable]
    private class RegisterServerData {
        public ushort port;
        public string joinCode;
    }

    [System.Serializable]
    private class ServerClosedData {
        public ushort port;
    }
    private void Awake() {
#if UNITY_SERVER || UNITY_STANDALONE_SERVER
        DontDestroyOnLoad(gameObject);
        Loader.Load(Loader.Scene.TestingConnectionScene);
#endif
    }

    private async void Start() {
#if UNITY_SERVER || UNITY_STANDALONE_SERVER
        Debug.Log("[SERVER] Inicializando instância dedicada...");
        

        ushort serverPort = GetPortFromArgs();
/*
        try {
            // 1. Inicializa serviços e autentica o servidor anonimamente
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn) {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            // 2. Cria alocação no Relay Server
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MAX_PLAYERS);

            // 3. Obtém o Join Code gerado
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"[SERVER] Relay Code gerado: {joinCode}");

            // 4. Configura o Unity Transport extraindo os dados manualmente (sem RelayServerData)
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                true // Define como 'true' para ativar a criptografia "dtls"
            );

            // 5. Inicia o servidor
            if (NetworkManager.Singleton.StartServer()) {
                Debug.Log($"[SERVER] Servidor rodando na porta local {serverPort} via Relay!");
                
                // 6. Avisa o Node.js que o servidor está pronto e envia o Join Code
                await RegisterServerNoNodeJs(REGISTERSERVER_URL, serverPort, joinCode);
                
                // Inscreve no evento de desconexão
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            } else {
                Debug.LogError("[SERVER] Falha ao iniciar o NetworkManager como servidor.");
                Application.Quit();
            }
        }
        catch (Exception e) {
            Debug.LogError($"[SERVER] Erro na inicialização do Relay: {e.Message}");
            Application.Quit();
        }*/
        await CardGameLobby.Instance.InitializeUnityAuthenticationAsync();

        string lobbyName = "LobbyName" + UnityEngine.Random.Range(100, 1000);
        await CardGameLobby.Instance.CreateLobby(lobbyName, false);
        await RegisterServerNoNodeJs(REGISTERSERVER_URL, serverPort, CardGameLobby.Instance.joinedLobby.LobbyCode);
#endif
    }

    private async void OnClientDisconnected(ulong clientId) {
#if UNITY_SERVER || UNITY_STANDALONE_SERVER
        // Se após a saída restarem 0 clientes (ou se for o fim do jogo), encerra a instância
        if (NetworkManager.Singleton.ConnectedClientsIds.Count <= 1) 
        {
            Debug.Log("[SERVER] Partida encerrada ou jogadores saíram. Fechando servidor...");
            await SendCloseRequest(CLOSESERVER_URL, GetPortFromArgs());
            
            Application.Quit();
        }
#endif
    }

    private ushort GetPortFromArgs() {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "-port" && i + 1 < args.Length) {
                if (ushort.TryParse(args[i + 1], out ushort parsedPort)) {
                    return parsedPort;
                }
            }
        }
        return 7777; // Porta padrão
    }

    private async Task RegisterServerNoNodeJs(string url, ushort port, string joinCode) {
        RegisterServerData data = new RegisterServerData { port = port, joinCode = joinCode};
        string jsonBody = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log("[SERVER] Código Relay registrado com sucesso no Node.js!");

                ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                CardGameMultiplayer.Instance.matchID = response.matchId;
            } else {
                Debug.LogError($"[SERVER] Erro ao registrar no Node.js: {request.error}");
            }
        }
    }

    private async Task SendCloseRequest(string url, ushort port) {
        ServerClosedData data = new ServerClosedData { port = port };
        string jsonBody = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"[SERVER] Erro ao avisar fechamento ao Node.js: {request.error}");
            }
        }
    }
    [System.Serializable]
    private class ServerResponse {
        public string status;
        public int matchId;
    }
}