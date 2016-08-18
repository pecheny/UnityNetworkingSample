using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;

abstract public class ProtocolBase {
    public const int PACKET_TYPE_ID = 999;

    public ProtocolBase() {
        RuntimeTypeModel.Default.Add(typeof(Vector3), false).SetSurrogate(typeof(Ve3Surrogate));
        RuntimeTypeModel.Default.Add(typeof(Quaternion), false).SetSurrogate(typeof(QuatSurrogate));
    }

    abstract public void HandlePacket(PacketBase packet);
}

[ProtoContract]
public class PacketBase : Extensible {
    [ProtoMember(ProtocolBase.PACKET_TYPE_ID)]
    public int Id { get; set; }
}
