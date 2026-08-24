using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ShowServer : MonoBehaviour{

    [SerializeField] private TextMeshProUGUI text;

    private void Start(){
        if (NetworkManager.Singleton.IsServer) {
            text.gameObject.SetActive(true);
        } else {
            text.gameObject.SetActive(false);
        }
    }
}
