using UnityEngine;

public class SampleNetworkServiceImpl : SampleNetworkServiceBase {


    Transform remoteObject;
    public SampleNetworkServiceImpl(TcpTransport tcpTransport, SampleProtocol sampleProtocol, Transform remoteObject) : base(tcpTransport, sampleProtocol){
        this.remoteObject = remoteObject;
    }

    public override void UpdatePositionPacketHandler(UpdatePositionPacket packet) {
        remoteObject.position = packet.position;
    }

}