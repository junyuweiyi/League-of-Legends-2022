/********************************************************************
	created:	2020/09/18
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System;
using UnityEngine.Profiling;

namespace iFramework
{
    public interface IDataTableCache
    {
        void StrongCache(bool cache);
    }
    public class DataTableCache<T> : IDataTableCache where T : DataUnit, new()
    {
        private WeakReference _weakObj;
        public DataTable<T> strongObj;

        private string _dataFile;
        private static string _profilingLabel = $"[Data]{typeof(T).Name} Load Cost";

        public DataTableCache(string dataFile)
        {
            _dataFile = dataFile;
            _weakObj = new WeakReference(Load(), false);
        }

        public DataTable<T> Get()
        {
            if (_weakObj.Target != null)
            {
                return _weakObj.Target as DataTable<T>;
            }
            else
            {
                DataTable<T> dt = Load();
                _weakObj.Target = dt;
                return dt;
            }
        }

        public void StrongCache(bool cache)
        {
            if (cache)
            {
                strongObj = Get();
            }
            else
            {
                strongObj = null;
            }
        }

        private DataTable<T> Load()
        {
            Profiler.BeginSample(_profilingLabel);
            DataTable<T> dt = new DataTable<T>();
            dt.Load(DataMgr.DataPath + _dataFile + ".bytes");
            Profiler.EndSample();
            return dt;
        }
    }
}
