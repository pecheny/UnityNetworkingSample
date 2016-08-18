using ProtoBuf;
using UnityEngine;

[ProtoContract]
class Ve3Surrogate
{
    [ProtoMember(1)]
    public float  x { get; set; }
    [ProtoMember(2)]
    public float  y { get; set; }
    [ProtoMember(3)]
    public float  z { get; set; }


    public static implicit operator Vector3(Ve3Surrogate value)
    {
        return new Vector3(value.x, value.y, value.z);
    }
    public static implicit operator Ve3Surrogate(Vector3 value)
    {
        var s = new Ve3Surrogate();
        s.x = value.x;
        s.y = value.y;
        s.z = value.z;
        return s;
    }
}