using UnityEngine;

[RequireComponent(typeof(TMPro.TMP_Text))]
public class UILocalizeTMPText : UILocalize
{
    private TMPro.TMP_Text _text;

    protected override void OnApplyValue(string key, string value)
    {
        Ensure(ref _text).text = value;
    }
}
