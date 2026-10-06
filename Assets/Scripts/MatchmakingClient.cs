using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Audio.ProcessorInstance;


public class MatchmakingClient : MonoBehaviour {
    private const string MATCHMAKER_URL = "https://reborn-suitor-sister.ngrok-free.dev/api/request-match";
    private const string WAITING_URL = "https://reborn-suitor-sister.ngrok-free.dev/api/waiting-match";

    private void Start() {
#if !UNITY_SERVER && !UNITY_STANDALONE_SERVER
        RequestMatchmaking();
#endif
    }

    public async void RequestMatchmaking() {
        Debug.Log("Solicitando partida...");
        string responseJson = await SendPostRequest(MATCHMAKER_URL, "{}");

        if (!string.IsNullOrEmpty(responseJson)) {
            MatchResponse response = JsonUtility.FromJson<MatchResponse>(responseJson);

            if (response.status == "WAIT") {
                Debug.Log("Servidor iniciando. Tentando novamente em 3 segundos...");
                CardGameMultiplayer.Instance.matchID = response.matchId;
                await Task.Delay(3000);
                WaitForMatch(); // Aguarda partida
            }else if (response.status == "JOIN_EXISTING") {
                CardGameMultiplayer.Instance.matchID = response.matchId;
                Debug.Log($"Conectando via Relay Code: {response.joinCode}");

                JoinServer(response.joinCode);
            }
        }
    }
    public async void WaitForMatch() {
        Debug.Log("Aguardando partida...");
        MatchResponse matchRequest = new() { matchId = CardGameMultiplayer.Instance.matchID };
        string responseJson = await SendPostRequest(WAITING_URL, JsonUtility.ToJson(matchRequest));

        if (!string.IsNullOrEmpty(responseJson)) {
            MatchResponse response = JsonUtility.FromJson<MatchResponse>(responseJson);

            if (response.status == "WAIT") {
                Debug.Log("Servidor iniciando. Tentando novamente em 3 segundos... " + CardGameMultiplayer.Instance.matchID);
                await Task.Delay(3000);
                WaitForMatch(); // Tenta de novo
            }else if (response.status == "JOIN_EXISTING") {
                Debug.Log(responseJson);
                Debug.Log($"Conectando via Relay Code: {response.joinCode}");

                JoinServer(response.joinCode);
            }
        }
    }

    
    private async Task<string> SendPostRequest(string url, string jsonBody) {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            // Regras HTTP obrigatórias
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Bypass-Tunnel-Reminder", "true"); // Resolve o erro 408 do Localtunnel
            request.SetRequestHeader("ngrok-skip-browser-warning", "true"); // Resolve o erro 408 do Localtunnel

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success) {
                return request.downloadHandler.text;
            }

            Debug.LogError($"Erro HTTP: {request.error}");
            return null;
        }
    }

    private async void JoinServer(string joinCode) {
        // Garante que o cliente está autenticado na Unity
        await CardGameLobby.Instance.InitializeUnityAuthenticationAsync();
        CardGameLobby.Instance.JoinWithCode(joinCode);
    }

    [System.Serializable]
    private class MatchResponse {
        public string status;
        public string message;
        public string joinCode;
        public int matchId;
    }

}