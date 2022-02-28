using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 本地化Text组件
/// </summary>
[RequireComponent(typeof(Text))]
public class UILocalizeText : UILocalize
{
    private Text _text;

    protected override void OnApplyValue(string key, string value)
    {
        Ensure(ref _text).text = value;
    }
}
