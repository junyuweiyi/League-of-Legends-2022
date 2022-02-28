/********************************************************************
	created:	2020/12/21
	author:		maoqy
	
	purpose:	
*********************************************************************/

using iFramework;
using System.Collections;
using System.Collections.Generic;

public class PooledHashSet<T> : IReference
{
    public HashSet<T> Set { get; } = new HashSet<T>();
    public void Clear()
    {
        Set.Clear();
    }

    public static PooledHashSet<T> Get()
    {
        return ReferencePool.Acquire<PooledHashSet<T>>();
    }
}