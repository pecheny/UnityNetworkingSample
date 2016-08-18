using UnityEngine;
interface SampleNetworkService {
    void SendUpdatePositionPacket(Vector3 position);

    void UpdatePositionPacketHandler(UpdatePositionPacket packet);

}
