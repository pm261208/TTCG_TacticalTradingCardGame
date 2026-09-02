using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SerializeReferenceEditor;
using UnityEngine;


[Serializable]
public abstract class EffectNode {

    [SerializeReference]
    [SR]
    public EffectNode nextEffect;
    public abstract IEnumerator Execute(EffectContext context);

    public abstract string HeaderText { get; }
    public string cardSubject;


    protected List<int> GetCardSubject(EffectContext context) {
        List<int> subjects = new();
        string[] cardSubjects = cardSubject.Split(' ');


        foreach (string sub in cardSubjects) {
            FieldInfo field = typeof(EffectContext).GetField(sub, BindingFlags.Public | BindingFlags.Instance);


            if (field == null) {
                field = context.eventData.GetType().GetField(sub, BindingFlags.Public | BindingFlags.Instance);
                if (field == null) {
                    Debug.LogError("Campo não encontrado!");
                } else {
                    object value = field.GetValue(context.eventData);

                    if (value is int intValue)
                        subjects.Add(intValue);
                }
            } else {
                object value = field.GetValue(context);

                if (value is int intValue)
                    subjects.Add(intValue);
            }
        }
        
        return subjects;
    }
}
