using ProtoBuf;using UnityEngine;
[ProtoContract]
public class UpdatePositionPacket : PacketBase {
    [ProtoMember(1)] public Vector3 position;

    [ProtoMember(ProtocolBase.PACKET_TYPE_ID)]    public int Id { get; set; }
}
