using UnityEngine;

[RequireComponent(typeof(TcpTransport))]
public class NetworkingExample : MonoBehaviour {
    TcpTransport tcpTransport;
    SampleNetworkServiceImpl service;
    SampleProtocol sampleProtocol;
    bool connected;


    void Awake() {
        tcpTransport = GetComponent<TcpTransport>();
        sampleProtocol = new SampleProtocol();
        service = new SampleNetworkServiceImpl(tcpTransport, sampleProtocol, GameObject.CreatePrimitive(PrimitiveType.Sphere).transform);

        tcpTransport.clientConnected = OnClientConnected;
        tcpTransport.connectionToServerFailed = OnConnectionToServerFailed;
        tcpTransport.connectedToServer = OnConnectedToServer;
        tcpTransport.peerDisconnected = OnDisconnect;

        tcpTransport.TryStartClient();
    }


    void OnConnectionToServerFailed() {
        tcpTransport.StartServer();
    }

    void OnConnectedToServer() {
        connected = true;
    }

    void OnClientConnected() {
        connected = true;
    }

    void OnDisconnect() {
        Debug.Log("Disconnect");
        connected = false;
    }


    void Update() {
        float speed = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) {
            speed = -1;
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            speed = 1;
        }
        transform.Translate(speed * Time.deltaTime, 0, 0);
        if (!connected) {
            return;
        }
        service.SendUpdatePositionPacket(transform.position);
    }


}