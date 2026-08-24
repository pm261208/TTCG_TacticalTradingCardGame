using System.Collections;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;


public class SendCardToGYNode : EffectNode{
    public override string HeaderText => "Requires: Card";

    public override IEnumerator Execute(EffectContext context) {
        SendCardToGYGA sentCardToGYGA = new( CardGameManager.Instance.GetCardFromLocalId(GetCardSubject(context)[0]));
        yield return ActionSystem.Instance.Perform(sentCardToGYGA);

        if (NetworkManager.Singleton.IsServer) {
            EffectContext newContext = new EffectContext();
            newContext.Source = GetCardSubject(context)[0];
            EventSystem.Instance.RaiseEvent(TriggerType.OnSentToGY, newContext);
        }

        if (nextEffect != null) {
            yield return nextEffect?.Execute(context);
        }
    }
}
