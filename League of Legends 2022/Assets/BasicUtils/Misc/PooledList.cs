/********************************************************************
	created:	2020/12/21
	author:		maoqy
	
	purpose:	
*********************************************************************/

using iFramework;
using System.Collections;
using System.Collections.Generic;

public class PooledList<T> : IReference
{
    public List<T> List { get; } = new List<T>(1);
    public void Clear()
    {
        List.Clear();
    }

    public static PooledList<T> Get()
    {
        return ReferencePool.Acquire<PooledList<T>>();
    }
}

public class PooledDictionary<TKey, TValue> : IReference
{
    public Dictionary<TKey, TValue> Dict { get; } = new Dictionary<TKey, TValue>();
    public void Clear()
    {
        Dict.Clear();
    }
    public static PooledDictionary<TKey, TValue> Get()
    {
        return ReferencePool.Acquire<PooledDictionary<TKey, TValue>>();
    }
}

public class Pooled2DList<T> : IReference
{
    public List<PooledList<T>> List2d { get; } = new List<PooledList<T>>();

    public List<T> this[int index] { get { return List2d[index].List; } }

    public int Count => List2d.Count;
    public void Clear()
    {
        foreach (var list in List2d)
        {
            ReferencePool.Release(list);
        }
        List2d.Clear();
    }

    public void Add(PooledList<T> list)
    {
        List2d.Add(list);
    }

    public void RemoveAt(int index)
    {
        var pooledList = List2d[index];
        List2d.RemoveAt(index);
        ReferencePool.Release(pooledList);
    }

    public PooledList<T> FetchAt(int index)
    {
        var pooledList = List2d[index];
        List2d.RemoveAt(index);
        return pooledList;
    }

    public void Insert(int index, PooledList<T> pooledList)
    {
        List2d.Insert(index, pooledList);
    }
}