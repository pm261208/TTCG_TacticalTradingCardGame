using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;



public abstract class CardSO : ScriptableObject{

    public int id;
    public string cardName;
    public CardType cardType;
    public string description;

    [SerializeReference]
    [SR]
    public List<CardEvent> events;
}
