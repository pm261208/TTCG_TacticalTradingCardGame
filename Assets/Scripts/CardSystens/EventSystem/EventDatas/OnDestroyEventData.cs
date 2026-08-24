using Unity.Netcode;
using UnityEngine;

public class OnDestroyEventData : EventData{

    public override EventType Type => EventType.OnDestroyEventData;

    public int[] destroyedCards;

    protected override void Serialize<T>(BufferSerializer<T> serializer) {
        serializer.SerializeValue(ref destroyedCards);
    }
}
