using Unity.Netcode;
using UnityEngine;

public class EffectContext : INetworkSerializable{

    public int Source;
    public ulong Owner;

    public int TargetCard;
    public int TargetTile;

    public EventData eventData;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref Source);
        serializer.SerializeValue(ref Owner);
        serializer.SerializeValue(ref TargetCard);
        serializer.SerializeValue(ref TargetTile);

        EventType type = serializer.IsWriter
            ? eventData?.Type ?? EventType.None
            : EventType.None;

        serializer.SerializeValue(ref type);

        if (serializer.IsReader) {
            eventData = CreateEventData(type);
        }

        eventData?.NetworkSerialize(serializer);
    }


    private static EventData CreateEventData(EventType type) {
        return type switch {
            EventType.OnDestroyEventData => new OnDestroyEventData(),
            EventType.OnDrawEventData => new OnDrawEventData(),
            EventType.OnSummonEventData => new OnSummonEventData(),
            EventType.OnTakeDamageEventData => new OnTakeDamageEventData(),
            EventType.OnAttackedEventData => new OnAttackedEventData(),
            EventType.ReduceStarManaEventData => new RemoveStarManaEventData(),
            EventType.PassiveEventData => new PassiveEventData(),
            _ => null
        };
    }
}
