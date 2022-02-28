/********************************************************************
	created:	2020/09/22
	author:		maoqy
	
	purpose:	集合相关扩展函数
*********************************************************************/
using System;
using System.Collections.Generic;

namespace iFramework
{
    /// <summary>
    /// 集合相关扩展函数
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// 方便为List &lt; KeyValuePair &lt; T1, T2 &gt; &gt;添加键值的接口
        /// </summary>
        /// <param name="list">需要添加键值的列表</param>
        /// <param name="key">需要添加到列表中的键值对中的键</param>
        /// <param name="value">需要添加到列表中的键值对中的值</param>
        /// <typeparam name="T1">列表中键的类型</typeparam>
        /// <typeparam name="T2">列表中值的类型</typeparam>
        /// <returns></returns>
        public static void Add<T1, T2>(this List<KeyValuePair<T1, T2>> list, T1 key, T2 value)
        {
            list.Add(new KeyValuePair<T1, T2>(key, value));
        }
        /// <summary>
        /// 方便为 List&lt;(T1, T2)&gt;添加 Tuple 的接口
        /// </summary>
        /// <param name="list">需要添加 Tuple 的列表</param>
        /// <param name="arg1">Tuple 的第一个元素</param>
        /// <param name="arg2">Tuple 的第二个元素</param>
        /// <typeparam name="T1">Tuple 第一个元素的类型</typeparam>
        /// <typeparam name="T2">Tuple 第二个元素的类型</typeparam>
        /// <returns></returns>
        public static void Add<T1, T2>(this List<(T1, T2)> list, T1 arg1, T2 arg2)
        {
            list.Add((arg1, arg2));
        }
        /// <summary>
        /// 方便为 List&lt;(T1, T2, T3)&gt;添加 Tuple 的接口
        /// </summary>
        /// <param name="list">需要添加 Tuple 的列表</param>
        /// <param name="arg1">Tuple 的第一个元素</param>
        /// <param name="arg2">Tuple 的第二个元素</param>
        /// <param name="arg3">Tuple 的第三个元素</param>
        /// <typeparam name="T1">Tuple 第一个元素的类型</typeparam>
        /// <typeparam name="T2">Tuple 第二个元素的类型</typeparam>
        /// <typeparam name="T3">Tuple 第三个元素的类型</typeparam>
        /// <returns></returns>
        public static void Add<T1, T2, T3>(this List<(T1, T2, T3)> list, T1 arg1, T2 arg2, T3 arg3)
        {
            list.Add((arg1, arg2, arg3));
        }

        /// <summary>
        /// 解构 <c>KeyValuePair&lt;T1, T2&gt;</c>
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> pair, out T1 key, out T2 value)
        {
            key = pair.Key;
            value = pair.Value;
        }

        /// <summary>
        /// 方便为Dictionary提供：key获取value的接口
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue TryGetValueFromPool<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, bool setDefaultIfMissing = false)
            where TValue : class, IReference, new()
        {
            TValue value;
            if (!dict.TryGetValue(key, out value) && setDefaultIfMissing)
            {
                value = ReferencePool.Acquire<TValue>();
                dict[key] = value;
            }
            return value;
        }

        public static void ClearToPool<TKey, TValue>(this Dictionary<TKey, TValue> dict)
              where TValue : IReference
        {
            foreach (var item in dict.Values)
            {
                ReferencePool.Release(item);
            }
            dict.Clear();
        }

        /// <summary>
        /// 方便为Dictionary提供：key获取value的接口
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, bool setDefaultIfMissing = false) where TValue : new()
        {
            TValue value;
            if (!dict.TryGetValue(key, out value) && setDefaultIfMissing)
            {
                value = new TValue();
                dict[key] = value;
            }
            return value;
        }

        public static List<T> ExSort<T>(this List<T> list)
        {
            list.Sort();
            return list;
        }

        public static List<T> ExSort<T>(this List<T> list, IComparer<T> comparer)
        {
            list.Sort(comparer);
            return list;
        }

        public static List<T> ExSort<T>(this List<T> list, Comparison<T> comparison)
        {
            list.Sort(comparison);
            return list;
        }

        /// <summary>
        /// 移除最后一个，并返回移除的元素
        /// </summary>
        /// <param name="list"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T RemoveLast<T>(this List<T> list)
        {
            var count = list.Count;
            if (count > 0)
            {
                var item = list[count - 1];
                list.RemoveAt(count - 1);
                return item;
            }
            return default(T);
        }
    }
}