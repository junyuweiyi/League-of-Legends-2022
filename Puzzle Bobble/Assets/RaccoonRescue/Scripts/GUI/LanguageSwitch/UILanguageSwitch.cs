using iFramework;

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UILanguageSwitch;
using static UnityEditor.PlayerSettings.Switch;

public class UILanguageSwitch : MonoBehaviour, ILanguageSelect
{
    public class LanguageSelectionCfg
    {
        public string Name;
        public string Code;
    }

    public interface ILanguageSelect
    {
        event Action<int> OnSelect;
        void Select(int number);
        bool IsSelected(int number);
    }


    public static List<LanguageSelectionCfg> cfgs = new List<LanguageSelectionCfg>()
    {
        new LanguageSelectionCfg(){  Name = "简体中文", Code = "cn" },
        new LanguageSelectionCfg(){  Name = "English" , Code = "en" },
    };

    public event Action<int> OnSelect;

    [SerializeField] ToggleGroup _itemContent;
    [SerializeField] Button _closeBtn, _confirmBtn;
    [SerializeField] LanguageSwitchItem _prefab;


    int _currentIndex = -1;


    void Awake()
    {
        //_closeBtn.onClick.AddListener(Close);
        LoadItems();
    }

    void LoadItems()
    {
        for (int i = 0; i < _itemContent.transform.childCount; i++)
        {
            _itemContent.transform.GetChild(i).gameObject.SetActive(false);
        }

        LanguageSwitchItem item;
        for (int i = 0; i < cfgs.Count; i++)
        {
            if (i >= _itemContent.transform.childCount)
            {
                var go = GameObject.Instantiate(_prefab, _itemContent.transform);
                go.GetComponentInChildren<Toggle>().group = _itemContent;
            }
            item = _itemContent.transform.GetChild(i).GetComponent<LanguageSwitchItem>();
            item.gameObject.SetActive(true);

            item.SetData(i, CreateItemUIData(cfgs[i]), this);

            if ((this as ILanguageSelect).IsSelected(i))
            {
                SelectLanguage(i);
            }
        }
    }

    static UILanguageSwitchItemData CreateItemUIData(LanguageSelectionCfg cfg)
    {
        var result = new UILanguageSwitchItemData();
        result.languageName = cfg.Name;
        return result;
    }

    void SelectLanguage(int index)
    {
        if (_currentIndex == index)
            return;
        _currentIndex = index;
        //OnSelect.Invoke(_currentIndex);
        SetSelectedLanguageAndRestartGame();
    }

    SystemLanguage GetSystemLanguage(int index)
    {
        string local = cfgs[index].Code;
        var languages = LanguageUtilis.GetLanguage(local);
        if (languages == null || languages.Count == 0)
            Debug.LogError("没有找到对应language local：" + local + ", 请检查配置");
        return languages[0];
    }

    void SetSelectedLanguageAndRestartGame()
    {
        var systemLanguage = GetSystemLanguage(_currentIndex);
        FW.Localization.Load(LanguageUtilis.GetLanguageLocale(systemLanguage));
        PlayerPrefs.SetInt("Language", (int)systemLanguage);
    }

    void ILanguageSelect.Select(int number)
    {
        SelectLanguage(number);
    }

    bool ILanguageSelect.IsSelected(int number)
{
        var language = (SystemLanguage)PlayerPrefs.GetInt("Language");
        return cfgs[number].Code == LanguageUtilis.GetLanguageLocale(language); ;
    }
}