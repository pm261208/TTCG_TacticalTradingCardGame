using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour{

    public static ObjectManager Instance {  get; private set; }

    public Transform handPlayer;
    public Transform handOpponent;
    public Transform deckPlayer;
    public Transform deckOpponent;
    public Transform gyPlayer;
    public Transform gyOpponent;
    public Transform field;
    public Transform attackPlayerButton;

    private void Awake() {
        Instance = this;
    }

}
