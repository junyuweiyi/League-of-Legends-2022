using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace iFramework
{
    internal class LocalizationMgr : ILocalizationMgr
    {
        public System.Action OnLocalizationLoad { get; set; }

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

        public void Initialize()
        {
        }

        struct ParserData
        {
            public string key;
            public string value;
            public string languageName;

            public void Apply(Dictionary<string, Dictionary<string, string>> dict)
            {
                dict[languageName][key] = value;
            }
        }


        Dictionary<string, Dictionary<string, string>> _i18ns = new Dictionary<string, Dictionary<string, string>>();
        public void Load(string locale)
        {
            Unload();
            if (_i18ns.Count == 0)
            {
                var i18nText = Resources.Load<TextAsset>("i18n_texts");

                List<string> languageNames = new List<string>();
                StringReader stringReader = new StringReader(i18nText.text);
                string aLine = null;
                while (true)
                {
                    aLine = stringReader.ReadLine();
                    if (aLine != null)
                    {
                        var lineUnits = aLine.Split(',');

                        if (aLine.StartsWith("Key"))
                        {
                            for (int i = 1; i < lineUnits.Length; i++)
                            {
                                languageNames.Add(lineUnits[i]);
                            }
                            foreach (var languageName in languageNames)
                            {
                                _i18ns[languageName] = new Dictionary<string, string>();
                            }
                            continue;
                        }

                        string key = lineUnits[0];
                        for (int i = 1; i < lineUnits.Length; i++)
                        {
                            ParserData parserData = new ParserData();
                            parserData.key = "LC_" + key;
                            parserData.value = lineUnits[i];
                            parserData.languageName = languageNames[i - 1];
                            parserData.Apply(_i18ns);
                        }

                    }
                    else
                    {
                        break;
                    }
                }
            }

            _keyValues = new Dictionary<string, string>(_i18ns[locale]);
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
            OnLocalizationLoad?.Invoke();
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