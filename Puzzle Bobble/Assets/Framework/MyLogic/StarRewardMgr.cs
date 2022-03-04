using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 星星宝箱Mgr
/// </summary>
public class StarRewardMgr: GameMgr<StarRewardMgr>
{
    public int CurrentStar => _currentStar;
    public int MaxStar => _maxStar;


    //当前累计的星星，会再领奖后扣除
    int _currentStar;
    //领奖励需要的星星
    int _maxStar;
    //上次领奖的道具ID（下一次就不会把它加入奖励池）
    int _lastRewardItemID;

    public override void LoadData()
    {
        _currentStar = PlayerPrefs.GetInt("StarRewardMgr.CurrentStar", 0);
        _lastRewardItemID = PlayerPrefs.GetInt("StarRewardMgr.LastRewardItemID", 0);
        _maxStar = GameConfigMgr.StarRewardBoxMaxStar;
    }

    public override void SaveData()
    {
        PlayerPrefs.SetInt("StarRewardMgr.CurrentStar", _currentStar);
        PlayerPrefs.SetInt("StarRewardMgr.LastRewardItemID", _lastRewardItemID);
    }

    public void Reward()
    {
        if (_currentStar < _maxStar)
            return;

        _currentStar -= _maxStar;

        var rewardPool = new List<List<ItemRandom>>();
        foreach (var list in rewardPool)
        {
            //剔除上次已领过的奖_lastRewardItemID
            if (_lastRewardItemID != 0)
                CullReward(list, _lastRewardItemID);
        }
        var randomRewards = MgrDelegate.GenerateRandomRewards(rewardPool);
        MgrDelegate.I.ReceiveReward(randomRewards.Select(p => p.Item));
        _lastRewardItemID = randomRewards.Find(p => p.Weight < 100)?.Item.ID ?? 0;
    }



    static List<ItemRandom> CullReward(List<ItemRandom> rewards, int cullItemID)
    {
        rewards.RemoveAll(p => p.Item.ID == cullItemID);
        return rewards;
    }
}
