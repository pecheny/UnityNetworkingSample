using ProtoBuf;
using UnityEngine;

[ProtoContract]
class QuatSurrogate {
    [ProtoMember(1)]
    public float  x { get; set; }
    [ProtoMember(2)]
    public float  y { get; set; }
    [ProtoMember(3)]
    public float  z { get; set; }
    [ProtoMember(4)]
    public float w { get; set; }

    public static implicit operator Quaternion(QuatSurrogate value) {
        return new Quaternion(value.x, value.y, value.z, value.w);
    }
    public static implicit operator QuatSurrogate(Quaternion value) {
        var s = new QuatSurrogate();
        s.x = value.x;
        s.y = value.y;
        s.z = value.z;
        s.w = value.w;
        return s;
    }
}