/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace iFramework
{
    internal class DataMgr : IDataMgr, IFrameworkModule
    {
        private Dictionary<string, IDataTableCache> _dataTables = new Dictionary<string, IDataTableCache>();
        static public IResourceMgr ResMgr { get; private set; }
        static public string DataPath { get; private set; }

        // these cfg is always strong cache
        static Dictionary<string, bool> _allStrongCacheCfg = new Dictionary<string, bool>();

        public DataMgr()
        {
            Application.lowMemory += OnLowMemory;

            _allStrongCacheCfg["ItemsSummaryCfg"] = true;
            _allStrongCacheCfg["MapResourceSummaryCfg"] = true;
            _allStrongCacheCfg["SkillEffectCfg"] = true;
            _allStrongCacheCfg["AllianceResearchCfg"] = true;
            _allStrongCacheCfg["MapEnemyCfg"] = true;
            _allStrongCacheCfg["ResearchSummaryCfg"] = true;
            _allStrongCacheCfg["BuildingSummaryCfg"] = true;
        }

        public void Initialize(IResourceMgr resMgr, string dataPath)
        {
            ResMgr = resMgr;
            DataPath = dataPath;
        }

        public DataTable<T> GetDataTable<T>(string dataFile = null) where T : DataUnit, new()
        {
            if (string.IsNullOrEmpty(dataFile)) dataFile = typeof(T).Name;
            IDataTableCache cache;
            if (!_dataTables.TryGetValue(dataFile, out cache))
            {
                cache = new DataTableCache<T>(dataFile);
                _dataTables.Add(dataFile, cache);


                if (_allStrongCacheCfg.ContainsKey(dataFile))
                {
                    // force strong cache
                    CacheData<T>(true);
                }
            }
            return (cache as DataTableCache<T>).Get();
        }

        public T GetData<T>(int id, string dataFile = null) where T : DataUnit, new()
        {
            DataTable<T> dt = GetDataTable<T>(dataFile);
            if (dt != null)
            {
                return dt.GetData(id);
            }
            return default;
        }

        public void CacheData<T>(bool cache, string dataFile = null) where T : DataUnit, new()
        {
            if (string.IsNullOrEmpty(dataFile)) dataFile = typeof(T).Name;
            IDataTableCache dsc;
            _dataTables.TryGetValue(dataFile, out dsc);
            if (dsc != null)
            {
                dsc.StrongCache(cache);
            }
            else if (cache)
            {
                dsc = new DataTableCache<T>(dataFile);
                _dataTables.Add(dataFile, dsc);
                dsc.StrongCache(true);
            }
        }

        int IFrameworkModule.Priority => 1;

        void IFrameworkModule.Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        void IFrameworkModule.Shutdown()
        {
            Application.lowMemory -= OnLowMemory;
            _dataTables.Clear();
        }

        private void OnLowMemory()
        {
        }
    }
}