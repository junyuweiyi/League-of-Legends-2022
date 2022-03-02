using System.Collections;
using System.Collections.Generic;
using InitScriptName;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public enum LifeBuyType
{
    ResetLife = 0,
    TwoOursUnlimitedLife,
    WatchADGetOneLife,
}

struct LifeBuyTypeInfo
{
    public string desc;
    public System.Func<int> price;
    public System.Action onBuy;
    public bool isGem;
}

public class UILifeBuyTypeItem : MonoBehaviour
{
    public LifeBuyType lifeBuyType;
    public Text descText;
    public Text priceText;
    public GameObject gemIcon;

    static Dictionary<LifeBuyType, LifeBuyTypeInfo> lifeBuyTypeDict = new Dictionary<LifeBuyType, LifeBuyTypeInfo>()
    {
        [LifeBuyType.ResetLife] = new LifeBuyTypeInfo() { desc = "LC_ResetLife", price = () => GameConfigMgr.I.CostIfRefill, isGem = true, onBuy = () => { LifeBuyTypeResetLife(); } },
        [LifeBuyType.TwoOursUnlimitedLife] = new LifeBuyTypeInfo() { desc = "LC_TwoOursUnlimitedLife", price = () => GameConfigMgr.I.CostTwoOursUnlimitedLife, isGem = false, onBuy = () => { LifeBuyTypeTwoOursUnlimitedLife(); } },
        [LifeBuyType.WatchADGetOneLife] = new LifeBuyTypeInfo() { desc = "LC_WatchADGetOneLife", onBuy = () => { LifeBuyTypeWatchADGetOneLife(); } },
    };

    private void Awake()
    {
        var info = GetLifeBuyTypeInfo(lifeBuyType);
        SetDesc();
        gemIcon.SetActive(info.isGem);
        if (lifeBuyType == LifeBuyType.WatchADGetOneLife)
        {
            priceText.text = FW.Localization.Get("LC_WatchAD");
        }
        else
        {
            priceText.text = info.isGem ? GetPrice().ToString() : "￥" + GetPrice();
        }
        FW.Localization.OnLocalizationLoad += SetDesc;
    }

    private void OnDestroy()
    {
        FW.Localization.OnLocalizationLoad -= SetDesc;
    }

    void SetDesc()
    {
        descText.text = FW.Localization.Get(GetLifeBuyTypeInfo(lifeBuyType).desc);
    }

    int GetPrice()
    {
        return GetLifeBuyTypeInfo(lifeBuyType).price?.Invoke() ?? 0;
    }

    public void OnBuyBtnClick()
    {
        var info = GetLifeBuyTypeInfo(lifeBuyType);
        if (info.isGem)
        {
            //RMB暂时不考虑
            InitScript.Instance.SpendGems(info.price?.Invoke() ?? 0);
        }
        info.onBuy();
        GameObject.Find("MenuLifeShop").GetComponent<AnimationManager>().CloseMenu();
    }

    static LifeBuyTypeInfo GetLifeBuyTypeInfo(LifeBuyType lifeBuyType)
    {
        return lifeBuyTypeDict[lifeBuyType];
    }

    static void LifeBuyTypeResetLife()
    {
        UserInfoMgr.I.ResetLife();
    }

    static void LifeBuyTypeTwoOursUnlimitedLife()
    {
        UserInfoMgr.I.StartUnlimitedLife();
    }

    static void LifeBuyTypeWatchADGetOneLife()
    {
        UserInfoMgr.I.AddLife(1);
    }
}
