using Unity.Netcode;
using UnityEngine;

public class PassiveEventData : EventData{

    public override EventType Type => EventType.PassiveEventData;

    protected override void Serialize<T>(BufferSerializer<T> serializer) {
        
    }
}
