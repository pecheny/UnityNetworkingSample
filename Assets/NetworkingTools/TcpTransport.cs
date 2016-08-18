using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
[RequireComponent(typeof(Config))]
public class TcpTransport : MonoBehaviour {
    public Config config;

    public Action<PacketBase> receiveCallback;
    /// <summary>
    /// client-side callback after connect
    /// </summary>
    public Action connectedToServer;
    /// <summary>
    /// callback called if connection to server with given options failed.
    /// can be used in p2p case to switch for listening mode if other peer did't ready yet.
    /// </summary>
    public Action connectionToServerFailed;
    /// <summary>
    /// server-side callback
    /// </summary>
    public Action clientConnected;
    /// <summary>
    /// server-side callbackafter starting listening the port
    /// </summary>
    public Action startedAsServer;
    public Action peerDisconnected;

    TcpListener server;
    TcpClient client;


    private Queue<PacketBase> msgs = new Queue<PacketBase>();
    private Queue<Action> delayedCallbacks = new Queue<Action>();


    public void StartServer() {
        Log("Start server");
        Thread serverThread = new Thread(() => ConnectionListener());
        serverThread.IsBackground = true;
        serverThread.Start();
        if (startedAsServer != null) {
            startedAsServer();
        }
    }

    public void TryStartClient() {
        try {
            StartClient();
        } catch (Exception e) {
            Log("Connection to server failed: " + e.ToString());
            if (connectionToServerFailed != null) {
                connectionToServerFailed();
            }
        }
    }

    public void StartClient() {
        client = new TcpClient(config.GetIp(), config.GetPort());
        if (client != null) {
            Log("Connect as client");
            if (connectedToServer != null) {
                connectedToServer();
            }
            LaunchThreadForClient(client);
        }
    }

    void LaunchThreadForClient(TcpClient client) {
        client.ReceiveTimeout = 1;
        var thread = new Thread(PacketReceiverTask);
        thread.IsBackground = true;
        thread.Start(client);
    }

    private void ConnectionListener() {
        try {
            var localIPAddress = config.GetLocalIp();
            server = new TcpListener(IPAddress.Parse(localIPAddress), config.GetPort());
            server.Start();
            Log("Server started at " + localIPAddress);
        } catch (Exception e ) {
            Debug.Log(e.ToString());
        }

        client = server.AcceptTcpClient();
        if (clientConnected != null) {
            delayedCallbacks.Enqueue(clientConnected);
        }
        LaunchThreadForClient(client);
    }

    void PacketReceiverTask(object param) {
        var client = param as TcpClient;
        Log("Connected...");
        while (client.Connected) {
            PacketBase message = Serializer.DeserializeWithLengthPrefix<PacketBase>(client.GetStream(), PrefixStyle.Fixed32);
            if (message == null) {
                break;
            }
            lock (msgs) {
                msgs.Enqueue(message);
            }
        }
        Log("Disconnecdet...");
        client.Close();
    }

    public virtual void Send(PacketBase message) {
        if (!client.Connected) {
            if(peerDisconnected!=null) {
                peerDisconnected();
            }
            Shutdown();
            return;
        }
        Serializer.NonGeneric.SerializeWithLengthPrefix(client.GetStream(), message, PrefixStyle.Fixed32, 0);
    }

    private void Log(string s) {
        Debug.Log(s);
    }

    void Update() {
        if (receiveCallback == null) return;
        lock (delayedCallbacks) {
            while (delayedCallbacks.Count > 0) {
                delayedCallbacks.Dequeue()();
            }
        }
        lock (msgs) {
            while (msgs.Count > 0) {
                receiveCallback(msgs.Dequeue());
            }
        }
    }


    public void Shutdown() {
        //        listening = false;
        try {
            client.Close();
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
        try {
            server.Stop();
        } catch (Exception e) {
            Debug.Log(e.Message);
        }
    }

    void OnValidate() {
        if (config == null) {
            config = GetComponent<Config>();
        }
    }

    void OnApplicationQuit() {
        Shutdown();
    }

}
