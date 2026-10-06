using System;
using System.Collections.Generic;
using UnityEngine;
using SerializeReferenceEditor;

[Serializable]
public class CardEvent {
    public List<TriggerType> trigger = new();
    [SerializeReference]
    [SR]
    public EffectNode effects;
    [SerializeReference]
    [SR]
    public EffectNode cost;
    [SerializeReference]
    [SR]
    public List<EventCondition> conditions;
    public bool isOptional;
    public EffectTypes effectType;

}
