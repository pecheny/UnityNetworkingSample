using FullSerializer;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ProtocolGenerator {

    const string TAB = "    ";


    public const int INDENT_CLASS_BODY = 1;
    public const int INDENT_METHOD_BODY = 2;
    public const int INDENT_SUBBODY_1 = 3;
    public const int INDENT_SUBBODY_2 = 4;

    [MenuItem("Window/Generate protocol &a")]

    private static void GenarateTemplate() {
        var path = EditorUtility.OpenFilePanel("Chose protocol description", ".", "json");
        if (string.IsNullOrEmpty(path)) return;
        var dirPathBase = path.Remove(path.LastIndexOf("/"));
        var str = File.ReadAllText(path);
        var data = fsJsonParser.Parse(str);
        var dic = data.AsDictionary;
        var messages = dic["messages"].AsDictionary;
        var protocolName = dic["name"].AsString;
        // create packets
        var protocolDir = dirPathBase  + "/generated/" + protocolName + "/";
        var packets_path = protocolDir + "Packets/";
        if (!Directory.Exists(protocolDir)) {
            Directory.CreateDirectory(protocolDir);
        }
        if (!Directory.Exists(packets_path)) {
            Directory.CreateDirectory(packets_path);
        }

        foreach (var name in messages.Keys) {
            File.WriteAllText(packets_path + name + ".cs", GeneratePacket(name, messages[name]));
        }



        // create protocol
        var prCont = GenerateProtocolHeader(protocolName);
        prCont += GenerateMessageTypes(protocolName, messages);
        prCont += GenerateDelegates(protocolName, messages);
        prCont += GenerateHandlePacket(protocolName, messages);
        prCont += GenerateSendMethods(protocolName, messages);
        prCont += GenerateFooter(0);
        File.WriteAllText(protocolDir + protocolName + ".cs", prCont);

        var srvcCont = GenerateService(protocolName, messages);
        File.WriteAllText(protocolDir + GenerateServiceBaseName(protocolName) + ".cs", srvcCont);
        srvcCont = GenerateServiceInterface(protocolName, messages);

        File.WriteAllText(protocolDir + GenerateServiceName(protocolName)+ ".cs", srvcCont);
        Debug.Log("Protocol generation is done.");
    }



    //========= Interface =====

    private static string GenerateServiceInterface(string protocolName,  Dictionary<string, fsData> messages){
        var s = "";
        s += GenerateServiceInterfaceHeader(protocolName);
        s += GenerateServiceInterfaceSendMethods(protocolName, messages);
        s += GenerateServiceInterfaceHandlerMethods(protocolName, messages);
        s += GenerateFooter(0);
        return s;
    }

    private static string GenerateServiceInterfaceHeader(string name) {
        var s = "";
        s += "using UnityEngine;\n";
        s += "interface " + GenerateServiceName(name) + " {\n";
//        s += GetFormatted("bool peerIsReady { get; }", "{0}\n\n", INDENT_CLASS_BODY);
//        s += GetFormatted("bool peerGameIsOver { get; }", "{0}\n\n", INDENT_CLASS_BODY);
//        s += GetFormatted("void Enable();", "{0}\n\n", INDENT_CLASS_BODY);
//        s += GetFormatted("void Disable();", "{0}\n\n", INDENT_CLASS_BODY);
        return s;
    }

    static string GenerateServiceInterfaceSendMethods(string protocolName, Dictionary<string, fsData> messages) {
        var s = "";
        foreach (KeyValuePair<string, fsData> msg in messages) {
            s += GenerateServiceInterfaceSendMethod(protocolName, msg.Key, msg.Value.AsDictionary["fields"]);
        }
        return s;
    }

    static string GenerateServiceInterfaceHandlerMethods(string protocolName, Dictionary<string, fsData> messages) {
        var s = "";
        foreach (KeyValuePair<string, fsData> msg in messages) {
            var packetName = msg.Key;
            var handlerName = GenerateHandlerName(packetName);
            s += GetFormatted("void " + handlerName + "(" + packetName +" packet);", "{0}\n\n", INDENT_CLASS_BODY);
        }
        return s;
    }

    static string GenerateServiceInterfaceSendMethod(string protocolName, string packetName, fsData fields) {
        var s = "";
        s += GetFormatted("void" + " Send" + packetName + "(" + GetSignature(fields) + ");\n\n", "{0}", INDENT_CLASS_BODY);

        return s;
    }


    //========= Service =====

    private static string GenerateService(string protocolName,  Dictionary<string, fsData> messages){
        var s = "";
        s += GenerateServiceHeader(protocolName);
        s += GenerateServiceSetUpMethod(protocolName, messages);
        s += GenerateServiceSendMethods(protocolName, messages);
        s += GenerateServiceHandlerMethods(protocolName, messages);
        s += GenerateFooter(0);
        return s;
    }


    private static string GenerateServiceSetUpMethod(string protocolName, Dictionary<string, fsData> messages) {
        var s = "";
        s += GetFormatted("public " + GenerateServiceBaseName(protocolName)  + "(TcpTransport tcpTransport, " + protocolName  + " " + ToLowerFirstChar(protocolName)  + ") {\n", "{0}", INDENT_CLASS_BODY);
        s += GetFormatted("this.tcpTransport = tcpTransport", "{0};\n", INDENT_METHOD_BODY);
        s += GetFormatted("tcpTransport.receiveCallback = " + ToLowerFirstChar(protocolName) +".HandlePacket", "{0};\n", INDENT_METHOD_BODY);
        s += GetFormatted("this." +  ToLowerFirstChar(protocolName) + " = " + ToLowerFirstChar(protocolName), "{0};\n", INDENT_METHOD_BODY);
        foreach (KeyValuePair<string, fsData> msg in messages) {
            var handlerName = GenerateHandlerName(msg.Key);
            s += GetFormatted(ToLowerFirstChar(protocolName) + "." + handlerName  + " = this." + handlerName, "{0};\n", INDENT_METHOD_BODY);
        }
        s += GenerateFooter(INDENT_CLASS_BODY);
        return s;
    }
    private static string GenerateServiceHeader(string name) {
        var s = "";
        s += "using UnityEngine;\n";
        s += "public abstract class " + GenerateServiceBaseName(name) +   " : " + GenerateServiceName(name)+"  {\n";
        s += GetFormatted("TcpTransport tcpTransport;\n", "{0}", INDENT_CLASS_BODY);
        s += GetFormatted("" + name  + " " + ToLowerFirstChar(name) + ";\n", "{0}", INDENT_CLASS_BODY);
        return s;
    }

    static string GenerateServiceSendMethods(string protocolName, Dictionary<string, fsData> messages) {
        var s = "";
        foreach (KeyValuePair<string, fsData> msg in messages) {
            s += GenerateServiceSendMethod(protocolName, msg.Key, msg.Value.AsDictionary["fields"]);
        }
        return s;
    }

    static string GenerateServiceHandlerMethods(string protocolName, Dictionary<string, fsData> messages) {
        var s = "";
        foreach (KeyValuePair<string, fsData> msg in messages) {
            var packetName = msg.Key;
            var handlerName = GenerateHandlerName(packetName);
            s += GetFormatted("public abstract void " + handlerName + "(" + packetName +" packet);", "{0}\n\n", INDENT_CLASS_BODY);
        }
        return s;
    }

    static string GenerateServiceSendMethod(string protocolName, string packetName, fsData fields) {
        var s = "";
        s += GetFormatted("public void" + " Send" + packetName + "(", "{0}", INDENT_CLASS_BODY);
        s += GetSignature(fields);
        s += "){\n";
        s += GetFormatted("var packet = " + ToLowerFirstChar(protocolName) + ".Serialize" + packetName +"(", "{0}", INDENT_METHOD_BODY);
        s+= GetSignatureCall(fields);
        s += ");\n";
        s += GetFormatted("tcpTransport.Send(packet);", "{0};\n", INDENT_METHOD_BODY);
        s += GenerateFooter(INDENT_CLASS_BODY);
        return s;
    }



    // ======= Packet =====

    static string GeneratePacket(string name, fsData message) {
        var s = "";
        s += GeneratePacketHeader(name);
        s += GeneratePacketBody(message.AsDictionary["fields"].AsList);
        s += GetFormatted("[ProtoMember(ProtocolBase.PACKET_TYPE_ID)]    public int Id { get; set; }\n", "{0}", INDENT_CLASS_BODY);
        s += GenerateFooter(0);
        return s;
    }

    private static string GeneratePacketHeader(string name) {
        var s = "";
        s += "using ProtoBuf;";
        s += "using UnityEngine;\n";
        s += "[ProtoContract]\n";
        s += "public class " + name + " : PacketBase {\n";
        return s;
    }

    private static string GeneratePacketBody(List<fsData> fields) {
        var s = "";
        for (int i = 0; i < fields.Count; i++ ) {
            var  field = fields[i].AsDictionary;
            s += GetFormatted("[ProtoMember(" + (i + 1) + ")] public " + field["type"].AsString + " " + field["name"].AsString, "{0};\n", INDENT_CLASS_BODY);
        }
        s += "\n";
        return s;
    }
    //========= Protocol =====
    private static string GenerateProtocolHeader(string name) {
        var s = "";
        s += "using ProtoBuf;\n";
        s += "using System;\n";
        s += "using UnityEngine;\n";
        s += "public class " + name + " : ProtocolBase {\n";
        return s;
    }

    static string GenerateMessageTypes(string protocolName, Dictionary<string, fsData> messages) {
        var s = "";
        s += GetFormatted("public enum " + GenerateTypesEnumName(protocolName) + " : int {\n", "{0}", INDENT_CLASS_BODY);
        var keys = new List<string>(messages.Keys);
        for (int i = 0; i < keys.Count; i++ ) {
            var  name = keys[i];
            s += GetFormatted(name + " = " + i + ",\n", "{0}", INDENT_METHOD_BODY);
        }
        s += GenerateFooter(INDENT_CLASS_BODY);
        return s;
    }
    static string GenerateDelegates(string protocolName, Dictionary<string, fsData> messages) {
        var s = "";
        var keys = new List<string>(messages.Keys);
        for (int i = 0; i < keys.Count; i++ ) {
            var  name = keys[i];
            s += GetFormatted("public Action<" + name + "> " + GenerateHandlerName(name) + ";\n", "{0}", INDENT_CLASS_BODY);
        }
        return s;
    }

    static string GenerateHandlePacket(string protocolName, Dictionary<string, fsData> messages) {
        var s = "";
        s += GetFormatted("public override void HandlePacket(PacketBase packet) {\n", "{0}", INDENT_CLASS_BODY);
        s += GetFormatted("var packetId = (" + GenerateTypesEnumName(protocolName) + ")packet.Id;\n", "{0}", INDENT_METHOD_BODY);
        s += GetFormatted("switch (packetId) {\n", "{0}", INDENT_METHOD_BODY);
        var keys = new List<string>(messages.Keys);
        for (int i = 0; i < keys.Count; i++ ) {
            var  name = keys[i];
            s += GetFormatted(" case " + GenerateTypesEnumName(protocolName) + "." + name + " : " + GenerateHandlerName(name) + "(Serializer.ChangeType<PacketBase, " + name + ">(packet)); break", "{0};\n", INDENT_SUBBODY_1);
        }
        s += GetFormatted(" default : throw new Exception(\"Unknown packet type\")", "{0};\n", INDENT_SUBBODY_1);
        s += GenerateFooter(INDENT_METHOD_BODY);
        s += GenerateFooter(INDENT_CLASS_BODY);
        return s;
    }

    static string GenerateSendMethods(string protocolName, Dictionary<string, fsData> messages) {
        var s = "";
        foreach (KeyValuePair<string, fsData> msg in messages) {
            s += GenerateSendMethod(protocolName, msg.Key, msg.Value.AsDictionary["fields"]);
        }
        return s;
    }

    static string GenerateSendMethod(string protocolName, string packetName, fsData fields) {
        var s = "";
        s += GetFormatted("public " + packetName + " Serialize" + packetName + "(", "{0}", INDENT_CLASS_BODY);
        s += GetSignature(fields);
        s += "){\n";
        var instanceName = ToLowerFirstChar(packetName);
        s += GetFormatted(" var " + instanceName + "  = new " + packetName + "();\n", "{0}", INDENT_METHOD_BODY);
        s += GetFormatted(instanceName + ".Id = (int) " + GenerateTypesEnumName(protocolName) + "." + packetName, "{0};\n", INDENT_METHOD_BODY);
        foreach (fsData field in fields.AsList) {
            var dic = field.AsDictionary;
            s += GetFormatted(instanceName + "." + dic["name"].AsString + " = " + dic["name"].AsString, "{0};\n", INDENT_METHOD_BODY);
        }
        s += GetFormatted("return " + instanceName + ";\n", "{0}", INDENT_METHOD_BODY);
        s += GenerateFooter(INDENT_CLASS_BODY);
        return s;
    }

    //====== Names ======

    private static string GenerateTypesEnumName(string protocolName) {
        return protocolName + "PacketType";
    }

    private static string GenerateServiceName(string protocolName) {
        return protocolName.Replace("Protocol",  "NetworkService");
    }

    private static string GenerateServiceBaseName(string protocolName) {
        return protocolName.Replace("Protocol",  "NetworkService") + "Base";
    }


    private static string GenerateHandlerName(string protocolName) {
        return protocolName + "Handler";
    }



    //====== Unility =====
    private static string GetFormatted(string val, string template, int tabs) {
        var s = "";
        for (int i = 0; i < tabs; i++) {
            s += TAB;
        }
        s += string.Format(template, val);
        return s;
    }

    static string GetSignature(fsData fields) {
        var s = "";
        var list = fields.AsList;
        if (list.Count == 0) return "";
        foreach (fsData field in list) {
            var dic = field.AsDictionary;
            s += dic["type"].AsString + " " + dic["name"].AsString + ", ";
        }
        s = s.Remove(s.LastIndexOf(","));
        return s;
    }

    static string GetSignatureCall(fsData fields) {
        var s = "";
        var list = fields.AsList;
        if (list.Count == 0) return "";
        foreach (fsData field in list) {
            var dic = field.AsDictionary;
            s += dic["name"].AsString + ", ";
        }
        s = s.Remove(s.LastIndexOf(","));
        return s;
    }
    private static string GenerateFooter(int level) {
        var s = GetFormatted("}\n", "{0}", level);
        return s;
    }

    public static string ToLowerFirstChar(string input) {
        string newString = input;
        if (!string.IsNullOrEmpty(newString) && char.IsUpper(newString[0]))
            newString = char.ToLower(newString[0]) + newString.Substring(1);
        return newString;
    }


}
