/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace iFramework
{
    public class DataTable<T> where T : DataUnit, new()
    {
        public List<T> List
        {
            get
            {
                if (_sortedList == null)
                {
                    _sortedList = new List<T>(Data.Values);
                    _sortedList.Sort((a, b) => a.Key() - b.Key());
                }
                return _sortedList;
            }
        }
        public Dictionary<int, T> Data { get; } = new Dictionary<int, T>(64);

        private List<T> _sortedList;

        internal void Load(string dataFile)
        {
            //-------------------------

            Data.Clear();

            TextAsset asset = DataMgr.ResMgr.LoadAssetSync<TextAsset>(dataFile);
            MemoryStream stream = new MemoryStream(asset.bytes);
            DataMgr.ResMgr.Release(asset);

            T tempData;

            while (stream.ReadByte() != -1)
            {
                // 经过测试，发现 new T() 这种用法其实被编译为了 Activator.CreateInstance()
                tempData = new T();

                stream.Seek(-1, SeekOrigin.Current);
                tempData.Decode(stream);
                if (tempData.Key() >= 0)
                {
                    if (Data.ContainsKey(tempData.Key()))
                    {
                        Debug.LogError("Same key (" + tempData.Key() + ") already existed in file: " + dataFile);
                    }
                    else
                    {
                        Data.Add(tempData.Key(), tempData);
                    }
                }
            }
        }

        public T GetData(int key)
        {
            return Data.TryGetValue(key, out var ans) ? ans : default;
        }
    }
}