using UnityEngine;
using UnityEngine.UI;

public class DeckSelectMenuUI : MonoBehaviour{

    [SerializeField] private Button backButton;
    [SerializeField] private DeckTemplate deckTemplate;

    private void Start() {
        backButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        deckTemplate.gameObject.SetActive(false);
    }
}
