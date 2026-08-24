using System;
using System.Collections.Generic;
using UnityEngine;
using SerializeReferenceEditor;

[System.Serializable]
public class CardEvent {
    public TriggerType trigger;
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
