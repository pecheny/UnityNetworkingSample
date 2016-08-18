using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Config : MonoBehaviour {
    public int port;
    public string ip;
    public string localIp;

    void Reset() {
        ip = LocalIPAddress();
        localIp = LocalIPAddress();
        port = 13000;
    }

    public string GetIp() {
        return ip;
    }

    public string GetLocalIp() {
        return localIp;
    }

    public int GetPort() {
        return port;
    }

    string LocalIPAddress() {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                localIP = ip.ToString();
                break;
            }
        }
        return localIP;
    }

}
