using System;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;

public class DedicatedServerManager : MonoBehaviour {

    private const string CLOSESERVER_URL = "http://localhost:3000/api/server-closed";
    [System.Serializable]
    private class ServerClosedData {
        public ushort port;
    }

    private async void Start() {
        // Só executa se for uma build de Servidor Dedicado
#if UNITY_SERVER || UNITY_STANDALONE_SERVER
        Debug.Log("[SERVER] Inicializando instância dedicada...");

        ushort serverPort = GetPortFromArgs();

        // Configura a porta no Unity Transport (NGO)
        /*
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(relayData);
        transport.SetConnectionData("0.0.0.0", serverPort);
        */

        await CardGameLobby.Instance.InitializeUnityAuthenticationAsync();
        // Se você utiliza Lobby/Relay, chame a criação do Lobby aqui
        CardGameLobby.Instance.CreateLobby("testLobby", false);
        Debug.Log($"[SERVER] Servidor rodando na porta {serverPort}!");
#endif

#if UNITY_SERVER || UNITY_STANDALONE_SERVER
        // Inscreve no evento quando um cliente desconecta
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
#endif
    }

    private async void OnClientDisconnected(ulong clientId) {
#if UNITY_SERVER || UNITY_STANDALONE_SERVER
        // Se após a saída restarem 0 clientes (ou se for o fim do jogo), encerra a instância
        if (NetworkManager.Singleton.ConnectedClientsIds.Count <= 1) 
        {
            string responseJson = await SendPostRequest(CLOSESERVER_URL, GetCurrentPort());
            Debug.Log("[SERVER] Partida encerrada ou jogadores saíram. Fechando servidor...");
            
            // Encerra o processo do Unity do sistema operacional
            Application.Quit();
        }
#endif
    }


    // Método auxiliar para ler a flag "-port" enviada pelo Node.js
    private ushort GetPortFromArgs() {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "-port" && i + 1 < args.Length) {
                if (ushort.TryParse(args[i + 1], out ushort parsedPort)) {
                    return parsedPort;
                }
            }
        }
        return 7777; // Porta padrão caso não venha parâmetro
    }

    private async Task<string> SendPostRequest(string url, ushort port) {
        ServerClosedData data = new ServerClosedData { port = port };
        string jsonBody = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success) {
                Debug.Log("[SERVER] Node.js notificado com sucesso!");
                return request.downloadHandler.text;
            }

            Debug.LogError($"[SERVER] Erro ao avisar o Node.js: {request.error}");
            return null;
        }
    }

    public static ushort GetCurrentPort() {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++) {
            if (args[i] == "-port" && i + 1 < args.Length) {
                if (ushort.TryParse(args[i + 1], out ushort parsedPort)) {
                    return parsedPort;
                }
            }
        }
        return 7777; // Porta padrão de fallback caso nada tenha sido passado
    }
}