/********************************************************************
	created:	2020/09/27
	author:		maoqy
	
	purpose:	本地化脚本基类
*********************************************************************/

using UnityEngine;

/// <summary>
/// 本地化脚本基类，子类根据提供的键值实现具体的本地化行为
/// </summary>
public abstract class UILocalize : MonoBehaviour, iFramework.ILocalizationLoadListener
{
    [SerializeField]
    private string _key = "LC_";

    public string Key
    {
        get => _key;
        set
        {
            _key = value;
            Apply();
        }
    }

#if UNITY_EDITOR
    public void SetKeyWithoutApply(string value)
    {
        _key = value;
    }
#endif

    protected virtual void OnEnable()
    {
        FW.Localization.RegisterLocalizationLoadListener(this);
        Apply();
    }

    protected virtual void OnDisable()
    {
        FW.Localization.UnregisterLocalizationLoadListener(this);
    }

    void iFramework.ILocalizationLoadListener.OnLocalizationLoaded(string locale)
    {
        Apply();
    }

    protected abstract void OnApplyValue(string key, string value);

    [ContextMenu("Apply")]
    public void Apply()
    {
        var value = FW.Localization.Get(Key);
        OnApplyValue(Key, value);
    }

    protected T Ensure<T>(ref T t) where T : Component
    {
        return t ?? (t = GetComponent<T>());
    }
}
