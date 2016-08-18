using ProtoBuf;
using System;
using UnityEngine;
public class SampleProtocol : ProtocolBase {
    public enum SampleProtocolPacketType : int {
        UpdatePositionPacket = 0,
    }
    public Action<UpdatePositionPacket> UpdatePositionPacketHandler;
    public override void HandlePacket(PacketBase packet) {
        var packetId = (SampleProtocolPacketType)packet.Id;
        switch (packetId) {
             case SampleProtocolPacketType.UpdatePositionPacket : UpdatePositionPacketHandler(Serializer.ChangeType<PacketBase, UpdatePositionPacket>(packet)); break;
             default : throw new Exception("Unknown packet type");
        }
    }
    public UpdatePositionPacket SerializeUpdatePositionPacket(Vector3 position){
         var updatePositionPacket  = new UpdatePositionPacket();
        updatePositionPacket.Id = (int) SampleProtocolPacketType.UpdatePositionPacket;
        updatePositionPacket.position = position;
        return updatePositionPacket;
    }
}
