using Unity.Netcode;
using UnityEngine;

public class OnTakeDamageEventData : EventData{

    public override EventType Type => EventType.OnTakeDamageEventData;

    public int damageTaken;
    public int playerId;

    protected override void Serialize<T>(BufferSerializer<T> serializer) {
        serializer.SerializeValue(ref damageTaken);
        serializer.SerializeValue(ref playerId);
    }
}
