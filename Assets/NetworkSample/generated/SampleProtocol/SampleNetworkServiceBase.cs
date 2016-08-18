using UnityEngine;
public abstract class SampleNetworkServiceBase : SampleNetworkService  {
    TcpTransport tcpTransport;
    SampleProtocol sampleProtocol;
    public SampleNetworkServiceBase(TcpTransport tcpTransport, SampleProtocol sampleProtocol) {
        this.tcpTransport = tcpTransport;
        tcpTransport.receiveCallback = sampleProtocol.HandlePacket;;
        this.sampleProtocol = sampleProtocol;
        sampleProtocol.UpdatePositionPacketHandler = this.UpdatePositionPacketHandler;
    }
    public void SendUpdatePositionPacket(Vector3 position){
        var packet = sampleProtocol.SerializeUpdatePositionPacket(position);
        tcpTransport.Send(packet);;
    }
    public abstract void UpdatePositionPacketHandler(UpdatePositionPacket packet);

}
