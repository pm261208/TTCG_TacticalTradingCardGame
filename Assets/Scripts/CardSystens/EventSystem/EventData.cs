using System.Runtime.Serialization;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public abstract class EventData : INetworkSerializable {

    public abstract EventType Type { get; }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        Serialize(serializer);
    }

    protected abstract void Serialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter;
}
