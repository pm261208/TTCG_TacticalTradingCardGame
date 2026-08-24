using System;
using UnityEngine;
using UnityEngine.Events;

public class EventSystem : Singleton<EventSystem> {

    public event EventHandler<OnProcessTrigerEventArgs> OnProcessTriger;
    public class OnProcessTrigerEventArgs : EventArgs {
        public TriggerType trigger; 
        public EffectContext ctx;
    }



    public void RaiseEvent(TriggerType trigger, EffectContext ctx) {    
        OnProcessTriger?.Invoke(this, new OnProcessTrigerEventArgs {
            trigger = trigger,
            ctx = ctx
        });

        if(!ChainSystem.Instance.buildingChain && !ChainSystem.Instance.resolvingChain) {
            FinishEvent();
        }
    }

    public void FinishEvent() {
        ChainSystem.Instance.ProcessPendingEffects();
    }
}
