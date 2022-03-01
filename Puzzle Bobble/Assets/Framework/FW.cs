using UnityEngine;
using iFramework;

public class FW
{
    #region Framework Managers
    public static IDataMgr DataMgr { get; private set; }
    public static IResourceMgr ResourceMgr { get; private set; }
    public static ILocalizationMgr Localization { get; private set; }
    #endregion

    static FW()
    {
        DataMgr = new DataMgr();
        ResourceMgr = new ResourceMgr();
        Localization = new LocalizationMgr();

        DataMgr.Initialize(ResourceMgr, "");
        Localization.Initialize();

        SystemLanguage language = SystemLanguage.Unknown;
        if (PlayerPrefs.HasKey("Language"))
        {
            language = (SystemLanguage)PlayerPrefs.GetInt("Language");
        }
        else
        {
            language = Application.systemLanguage;
            PlayerPrefs.SetInt("Language", (int)language);
        }
        var temp = language;
        language = GetValidLanguage(language);
        if (language != temp)
        {
            PlayerPrefs.SetInt("Language", (int)language);
        }
        FW.Localization.Load(LanguageUtilis.GetLanguageLocale(language));
    }

    static SystemLanguage GetValidLanguage(SystemLanguage language)
    {
        var cfgLanguages = UILanguageSwitch.cfgs;
        string local = LanguageUtilis.GetLanguageLocale(language);
        //如果这个语种在游戏允许范围内（策划会配置我们游戏允许的语种列表LanguageSelectionCfg）
        if (!string.IsNullOrEmpty(local) && cfgLanguages.Exists(p => p.Code == local))
            return language;
        //不允许则默认用英语
        return SystemLanguage.English;
    }
}
