using Unity.Netcode;
using UnityEngine;

public class RemoveStarManaEventData : EventData {

    public override EventType Type => EventType.ReduceStarManaEventData;

    public int reduceAmount;
    public int player;

    protected override void Serialize<T>(BufferSerializer<T> serializer) {
        serializer.SerializeValue(ref reduceAmount);
        serializer.SerializeValue(ref player);
    }
}
