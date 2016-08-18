using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FSDataBuilder {


    public enum fsDataType {
        Array,
        Object,
        Double,
        Int64,
        Boolean,
        String,
        Null
    }



    private Dictionary<string, fsData> dic = new Dictionary<string, fsData>();

    public FSDataBuilder AddRecord(string key, object value) {
        if (dic.ContainsKey(key)) {
            throw new Exception("FSDataBuilder: Already have record with key " + key);
        }
        fsData myVal;
        if ((value is double) || (value is float)) {
            myVal = new fsData(Convert.ToDouble(value));
        } else if (value is int) {
            myVal = new fsData((int) value);
        } else if (value is bool) {
            myVal = new fsData((bool) value);
        } else if (value is string) {
            myVal = new fsData((string) value);
        } else if (value is List<fsData>) {
            myVal = new fsData((List<fsData>) value);
        } else if (value is fsData) {
            myVal = (fsData) value;
        } else if (value ==null) {
            myVal = new fsData();
        } else {
            throw new Exception("FSDataBuilder: Unsupported value type " + value.ToString());
        }
        dic[key] = myVal;
        return this;
    }

    public fsData Build() {
        return new fsData(dic);
    }
}
