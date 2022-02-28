/********************************************************************
	created:	2020/09/23
	author:		maoqy
	
	purpose:	本地化管理器，根据当前设置的语言提供对应语言的文本
*********************************************************************/

using System.Collections.Generic;
using UnityEngine;
using System;

namespace iFramework
{
    internal class LocalizationMgr : ILocalizationMgr
    {
        private IDataMgr _dataMgr;
        private Dictionary<string, string> _keyValues = new Dictionary<string, string>();

        private HashSet<ILocalizationLoadListener> _localizationLoadListeners = new HashSet<ILocalizationLoadListener>();


        public void RegisterLocalizationLoadListener(ILocalizationLoadListener listener)
        {
            _localizationLoadListeners.Add(listener);
        }

        public void UnregisterLocalizationLoadListener(ILocalizationLoadListener listener)
        {
            _localizationLoadListeners.Remove(listener);
        }

        public void Initialize(IDataMgr dataMgr)
        {
            _dataMgr = dataMgr;
        }
        public void Load(string locale)
        {
            Unload();

            var dataTable = _dataMgr.GetDataTable<I18NText>("I18NText_" + locale);
            foreach (var data in dataTable.Data.Values)
            {
                if (!_keyValues.ContainsKey(data.key))
                    _keyValues.Add(data.key, data.value);
                else
                    Debug.LogError("本地化表有重复 " + data.key + " " + data.value);
            }

            foreach (var listener in _localizationLoadListeners)
            {
                try
                {
                    listener.OnLocalizationLoaded(locale);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public string Get(string key, bool returnKeyIfNotFound = true, string defaultValue = "")
        {
            if (key != null && _keyValues.TryGetValue(key, out var value))
                return value;
            return returnKeyIfNotFound ? key : defaultValue;
        }

        public string Format(string key, object arg0)
        {
            var format = Get(key);
            return string.Format(format, arg0);
        }

        public string Format(string key, object arg0, object arg1)
        {
            var format = Get(key);
            return string.Format(format, arg0, arg1);
        }

        public string Format(string key, object arg0, object arg1, object arg2)
        {
            var format = Get(key);
            return string.Format(format, arg0, arg1, arg2);
        }

        public string Format(string key, params object[] args)
        {
            var format = Get(key);
            return string.Format(format, args);
        }

        private void Unload()
        {
            _keyValues.Clear();
        }
    }
}