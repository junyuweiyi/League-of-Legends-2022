using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class UIDailyRewardBox : MonoBehaviour
{
    public List<UIDailyRewardItem> dailyRewardItems;
    public Text receiveBtnText;

    void OnEnable()
    {
        UpdateContent();
    }

    void UpdateContent()
    {
        var dailyRewards = DailyRewardMgr.I.DailyRewards;
        for (int i = 0; i < dailyRewardItems.Count; i++)
        {
            dailyRewardItems[i].SetData(dailyRewards[i]);
        }
    }
}
