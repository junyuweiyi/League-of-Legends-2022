/********************************************************************
	created:	2021/12/02
	author:		maoqy
	
	purpose:	包装 List 用来序列化
*********************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class ListWrapper<T>
{
    public List<T> list;

    static ListWrapper<T> _wrapper;
    public static string Serialize(List<T> l)
    {
        if (_wrapper == null)
            _wrapper = new ListWrapper<T>();

        _wrapper.list = l;

        var text = JsonUtility.ToJson(_wrapper);
        _wrapper.list = null;
        return text;
    }

    public static List<T> Deserialize(string json)
    {
        return JsonUtility.FromJson<ListWrapper<T>>(json)?.list;
    }
}