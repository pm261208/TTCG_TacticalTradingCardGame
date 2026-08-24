using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class OnDrawEventData : EventData{

    public override EventType Type => EventType.OnDrawEventData;

    public int[] drawedCards;

    protected override void Serialize<T>(BufferSerializer<T> serializer) {
        serializer.SerializeValue(ref drawedCards);
    }
}
