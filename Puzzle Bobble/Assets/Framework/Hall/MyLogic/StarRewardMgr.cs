using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/// <summary>
/// 星星宝箱Mgr
/// </summary>
public class StarRewardMgr : GameMgr<StarRewardMgr>
{
    public event Action OnStarRewardDataChanged;

    //当前累计的星星，会再领奖后扣除
    public int CurrentStar { get; private set; }
    public int MaxStar => _maxStarSingleReward;

    public bool Rewardable => CurrentStar >= MaxStar;

    //已领奖次数
    int _rewardCount;
    //每次领奖励需要的星星
    int _maxStarSingleReward;
    //上次领奖的道具ID（下一次就不会把它加入奖励池）
    int _lastRewardItemID;

    public override void LoadData()
    {
        _rewardCount = PlayerPrefs.GetInt("StarRewardMgr.RewardStarCount", 0);
        _lastRewardItemID = PlayerPrefs.GetInt("StarRewardMgr.LastRewardItemID", 0);
        _maxStarSingleReward = GameConfigMgr.StarRewardBoxMaxStar;
        UpdateCurrentStar();
    }

    public override void SaveData()
    {
        PlayerPrefs.SetInt("StarRewardMgr.RewardStarCount", _rewardCount);
        PlayerPrefs.SetInt("StarRewardMgr.LastRewardItemID", _lastRewardItemID);
    }

    public void UpdateCurrentStar()
    {
        var totalStar = MgrDelegate.I.GetTotalStar();
        CurrentStar = totalStar - _rewardCount * _maxStarSingleReward;
        OnStarRewardDataChanged.Invoke();
    }

    public void Reward()
    {
        if (CurrentStar < _maxStarSingleReward)
            return;

        var rewardPool = new List<List<ItemRandom>>();
        foreach (var list in rewardPool)
        {
            //剔除上次已领过的奖_lastRewardItemID
            if (_lastRewardItemID != 0)
                CullReward(list, _lastRewardItemID);
        }
        var randomRewards = MgrDelegate.GenerateRandomRewards(rewardPool);
        MgrDelegate.I.ReceiveReward(randomRewards.Select(p => p.item));
        _lastRewardItemID = randomRewards.Find(p => p.Weight < 100)?.item.ID ?? 0;
        _rewardCount++;
        UpdateCurrentStar();
    }



    static List<ItemRandom> CullReward(List<ItemRandom> rewards, int cullItemID)
    {
        rewards.RemoveAll(p => p.item.ID == cullItemID);
        return rewards;
    }
}
