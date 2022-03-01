using UnityEngine;
using UnityEngine.UI;

public class UILanguageSwitchItemData
{
    public string languageName;
}

public class LanguageSwitchItem : MonoBehaviour
{
    [SerializeField] Toggle _toggle;
    [SerializeField] Text _languageNameText;

    UILanguageSwitch.ILanguageSelect _select;
    int _index;

    void Awake()
    {
        _toggle.onValueChanged.AddListener(OnValueChanged);
    }

    public void SetData(
        int index,
        UILanguageSwitchItemData data,
        UILanguageSwitch.ILanguageSelect select)
    {
        _index = index;
        _select = select;
        bool isSelect = select.IsSelected(_index);
        _toggle.SetIsOnWithoutNotify(isSelect);
        _languageNameText.text = data.languageName;
    }

    void OnValueChanged(bool on)
    {
        if (on)
        {
            _select.Select(_index);
        }
    }
}