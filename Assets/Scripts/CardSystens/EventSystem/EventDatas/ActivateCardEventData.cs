using Unity.Netcode;
using UnityEngine;

public class ActivateCardEventData : EventData {

    public override EventType Type => EventType.ActivateCardEventData;

    public int eventIndex;

    protected override void Serialize<T>(BufferSerializer<T> serializer) {
        serializer.SerializeValue(ref eventIndex);
    }
}
