using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LanguageUtilis
{
    public struct LanguageConfig
    {
        public string locale1;
        public string locale2;
    }

    readonly static Dictionary<SystemLanguage, string> _systemLocales = new Dictionary<SystemLanguage, string>
    {
        // 首选两位语言代码，再看情况是否需要添加书写或地区代码
        { SystemLanguage.Afrikaans,           "af" },
        { SystemLanguage.Arabic,              "af" },
        { SystemLanguage.Basque,              "eu" },
        { SystemLanguage.Belarusian,          "by" },
        { SystemLanguage.Bulgarian,           "bg" },
        { SystemLanguage.Catalan,             "ca" },
        { SystemLanguage.Chinese,             "cn" },
        { SystemLanguage.ChineseSimplified,   "cn" },
        { SystemLanguage.ChineseTraditional,  "zh" },
        { SystemLanguage.Czech,               "cs" },
        { SystemLanguage.Danish,              "da" },
        { SystemLanguage.Dutch,               "nl" },
        { SystemLanguage.English,             "en" },
        { SystemLanguage.Estonian,            "et" },
        { SystemLanguage.Faroese,             "fo" },
        { SystemLanguage.Finnish,             "fi" },
        { SystemLanguage.French,              "fr" },
        { SystemLanguage.German,              "de" },
        { SystemLanguage.Greek,               "el" },
        { SystemLanguage.Hebrew,              "iw" },
        { SystemLanguage.Hungarian,           "hu" },
        { SystemLanguage.Icelandic,           "is" },
        { SystemLanguage.Indonesian,          "in" },
        { SystemLanguage.Italian,             "it" },
        { SystemLanguage.Japanese,            "ja" },
        { SystemLanguage.Korean,              "ko" },
        { SystemLanguage.Latvian,             "lv" },
        { SystemLanguage.Lithuanian,          "lt" },
        { SystemLanguage.Norwegian,           "no" },
        { SystemLanguage.Polish,              "pl" },
        { SystemLanguage.Portuguese,          "pt" },
        { SystemLanguage.Romanian,            "ro" },
        { SystemLanguage.Russian,             "ru" },
        { SystemLanguage.SerboCroatian,       "sh" },
        { SystemLanguage.Slovak,              "sk" },
        { SystemLanguage.Slovenian,           "sl" },
        { SystemLanguage.Spanish,             "es" },
        { SystemLanguage.Swedish,             "sv" },
        { SystemLanguage.Thai,                "th" },
        { SystemLanguage.Turkish,             "tr" },
        { SystemLanguage.Ukrainian,           "uk" },
        { SystemLanguage.Unknown,             "en" },
        { SystemLanguage.Vietnamese,          "vi" },
    };

    //key:local value:languages
    readonly static Dictionary<string, List<SystemLanguage>> _locals = new Dictionary<string, List<SystemLanguage>>();


    static LanguageUtilis()
    {
        foreach (var item in _systemLocales)
        {
            if (!_locals.TryGetValue(item.Value, out var languages))
            {
                languages = new List<SystemLanguage>();
                _locals[item.Value] = languages;
            }
            languages.Add(item.Key);
        }
    }

    public static bool ContainsLanguageConfig(SystemLanguage language)
    {
        return _systemLocales.ContainsKey(language);
    }

    /// <summary>
    /// 缩写方式1
    /// </summary>
    /// <param name="language"></param>
    /// <returns></returns>
    public static string GetLanguageLocale(SystemLanguage language)
    {
        return _systemLocales[language];
    }

    public static IList<SystemLanguage> GetLanguage(string local)
    {
        return _locals[local];
    }
}
