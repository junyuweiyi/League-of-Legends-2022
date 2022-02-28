/********************************************************************
	created:	2021/12/02
	author:		xp
	
	purpose:	包装 Dict 用来序列化
*********************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class DictWrapper<K, V>
{
    public List<K> listKey = new List<K>();
    public List<V> listValue = new List<V>();

    static DictWrapper<K, V> _wrapper;
    public static string Serialize(Dictionary<K, V> dict)
    {
        if (_wrapper == null)
            _wrapper = new DictWrapper<K, V>();

        _wrapper.listKey.Clear();
        _wrapper.listValue.Clear();
        foreach (var kv in dict)
        {
            _wrapper.listKey.Add(kv.Key);
            _wrapper.listValue.Add(kv.Value);
        }

        var text = JsonUtility.ToJson(_wrapper);
        _wrapper.listKey.Clear();
        _wrapper.listValue.Clear();
        return text;
    }

    public static Dictionary<K, V> Deserialize(string json)
    {
        var wrapper = JsonUtility.FromJson<DictWrapper<K, V>>(json);
        Dictionary<K, V> dict = new Dictionary<K, V>();
        if (wrapper.listKey.Count != wrapper.listValue.Count)
        {
            Debug.LogError("not same length");
            return dict;
        }
        for (int i = 0; i < wrapper.listKey.Count; i++)
        {
            var key = wrapper.listKey[i];
            var value = wrapper.listValue[i];
            dict[key] = value;
        }
        return dict;
    }

    public static void Deserialize(string json, Dictionary<K, V> dict)
    {
        var wrapper = JsonUtility.FromJson<DictWrapper<K, V>>(json);
        if (wrapper.listKey.Count != wrapper.listValue.Count)
        {
            Debug.LogError("not same length");
            return;
        }
        for (int i = 0; i < wrapper.listKey.Count; i++)
        {
            var key = wrapper.listKey[i];
            var value = wrapper.listValue[i];
            dict[key] = value;
        }
    }
}