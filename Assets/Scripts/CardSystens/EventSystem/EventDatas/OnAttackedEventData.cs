using Unity.Netcode;
using UnityEngine;

public class OnAttackedEventData : EventData {

    public override EventType Type => EventType.OnAttackedEventData;

    public int[] attackedCard;

    protected override void Serialize<T>(BufferSerializer<T> serializer) {
        serializer.SerializeValue(ref attackedCard);
    }
}
