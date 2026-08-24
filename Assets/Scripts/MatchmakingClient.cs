using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class MatchmakingClient : MonoBehaviour {
    private const string MATCHMAKER_URL = "http://localhost:3000/api/request-match";

    public async void RequestMatchmaking() {
        Debug.Log("Solicitando servidor ao Matchmaker...");
        string responseJson = await SendPostRequest(MATCHMAKER_URL, "{}");

        if (!string.IsNullOrEmpty(responseJson)) {
            MatchResponse response = JsonUtility.FromJson<MatchResponse>(responseJson);
            Debug.Log($"Status: {response.status} | Porta: {response.port}");

            await CardGameLobby.Instance.InitializeUnityAuthenticationAsync();
            await Task.Delay(5000);
            // Após saber que o servidor foi alocado/criado, 
            // você usa o seu CardGameLobby para entrar via QuickJoin no LobbyService da Unity
            CardGameLobby.Instance.QuickJoin();
        }
    }

    private async Task<string> SendPostRequest(string url, string jsonBody) {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success) {
                return request.downloadHandler.text;
            }

            Debug.LogError($"Erro ao chamar Matchmaker: {request.error}");
            return null;
        }
    }

    [System.Serializable]
    private class MatchResponse {
        public string status;
        public ushort port;
        public string message;
    }
}