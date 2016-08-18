## How to:  Unity, TCP, peer to peer and protobuf.

 This project aimed to demonstrate following things:
 - How to use Tcp connection in C#
 - How to organize peer 2 peer connection
 - How to use protobuf serialization
 - How to design network layer in your application in order to decouple networking and game logic
 - How to use code generation as aid for all this stuff usage.

 ### Content

 `TcpTransport` is a class which can take a role of Tcp server as well as a Tcp client. It had been written for 2x players game connected in local network.
 When instance of the game starts it looks if someone (on given in config ip address and port) is already listening for connection.
 If search wasn't successful it starts listening by itself (works as a server) so the next instance will able to connect as a client.
 Due to simplicity of the example `TcpTransport` extends `MonoBehaviour` and uses Update() loop.

 The `Config` also takes illustrative role. In real project you may want to put configuration in external file so you would able to configure same build for different PCs.

_Protobuf_. (and) is a very fast and compact way of serialization. In Unity we would use [.NET implementation](https://github.com/mgravell/protobuf-net).
In comparison with some other serializers (like JSON) it has some limitation.
You need to define format of all packets you want to serialize in the following way:

```csharp
[ProtoContract]
class Person {
    [ProtoMember(1)]
    public int Id {get;set;}
    [ProtoMember(2)]
    public string Name {get;set:}
    [ProtoMember(3)]
    public Address Address {get;set;}
}
[ProtoContract]
class Address {
    [ProtoMember(1)]
    public string Line1 {get;set;}
    [ProtoMember(2)]
    public string Line2 {get;set;}
}
```

This approach is also useful as fixation of protocol contract. The disadvantage is need to write some boilerplate code. It can be negated by code generation.
This project includes a tool (`ProtocolGenerator`, "Window/Generate protocol" in Unity editor) which takes protocol description in JSON format
and generates all the packets and some useful classes to work with this packets.

Protocol description sample:

```json
{
  "name":"SampleProtocol",
  "messages":{
    "UpdateBonePositionPacket":{
      "fields" : [
        {
          "type":"Vector3",
          "name":"position"
        }
      ]
    }
  }
}
```

Generated usefull classes are:

 - `Protocol` itself. it encapsulates all work with specific packets.
 - `NetworkService` – the interface which you would use across all "business logic" code to pass data outside. You can make your own implementation of this this service for testing purposes.
 - `NetworkServiceBase` – a base for network implementation of the service. You need to extend the base and implement handlers for packets of every type.
 The implementation requires references  to tcpTransport, protocol and all things you want to handle. This example takes all dependencies as constructor arguments.
 In more complex project you may want to use some DI-container like [Zenject](https://github.com/modesttree/Zenject).

 ### Pipeline

 As you can suspect, your application would work following way:

 - send data from whatever you want through the Service
 - handle all kinds of packets in the service

 So your "business logic" would not know about anything except the service which represents protocol api and can be implemented in several ways.

 The other point is configuration - a place to assembly all parts to work together. You can look at `NetworkingExample` class. As i already said in real projects i would prefer [Zenject](https://github.com/modesttree/Zenject).

Advantage of this approach is strict protocol. When you change the contract - you change it in one place (json) and generator make you to implement support for all this changes.
This sample project pipeline has a lack of implementation for step of changing the contract. I mean following: if you change protocol you would clean "generated" folder.
In this case your codebase became inconsistent i.e. implementation classes woud refer to the classes you had deleted. So the Unity editor will require fix all errors.
In this sample project all you need is comment all content of `NetworkingExample` and `SampleNetworkServiceImpl` classes. In real project it can turn into pain since number of classes
which refer to generated ones can be big.
I can see solution, it contain following steps:
 1. put all code depending on generated classes in one folder
 2. move it outside the project for the time of generation
I guess [Projeny](https://github.com/modesttree/projeny) module manager for unity can help handle this actions. It combines "UnityProject" folder from symlinks to real folders outside
the folder, so you can have several unity projects which deals with same module folders. Thus you can keep one project for one module with protocol description and generated classes only.
 Other project can refer to this module and also contain dependencies. You always can open project with single module and do some generation since it doesn't contain any dependencies.

### Run the example

1. Make a build
2. Run two instances in any combination (build+build, editor+build, build+editor)
3. Editor reports some events to the console
4. You should be able to move object with left and right keyboard arrows.
5. The cube represents a local object and the sphere – remote one.
6. If you want to test on different PCs - don't forget to change config.
7. Also if you have a firewall, ensure that it allows to communicate given application thou given port.
