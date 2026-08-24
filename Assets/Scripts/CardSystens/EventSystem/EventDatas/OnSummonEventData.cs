using Unity.Netcode;
using UnityEngine;

public class OnSummonEventData : EventData{

    public override EventType Type => EventType.OnSummonEventData;

    public int[] summonedCards;

    protected override void Serialize<T>(BufferSerializer<T> serializer) {
        serializer.SerializeValue(ref summonedCards);
    }
}
