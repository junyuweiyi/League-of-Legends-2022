using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 每日宝箱
/// </summary>
public class DailyRewardMgr : GameMgr<DailyRewardMgr>
{
    readonly Dictionary<int, DailyReward> _dailyRewards = new Dictionary<int, DailyReward>();
    int _currentCanRewardDay;

    /// <summary>
    /// 当前是本周第几天
    /// </summary>
    public int CurrentDay;
    /// <summary>
    /// 当前可领奖的天
    /// </summary>
    public int CanRewardDay { get; private set; }
    /// <summary>
    /// 下次刷新时间 每日0点刷新
    /// </summary>
    public long RefrehTimeStamp { get; private set; }
    /// <summary>
    /// 奖励内容
    /// </summary>
    public List<DailyReward> DailyRewards => _dailyRewards.Values.ToList();


    public void Reward()
    {
        if (CanRewardDay == 0)
            return;

        var dailyReward = GetDailyReward(_currentCanRewardDay);
        if (dailyReward == null)
            return;

        MgrDelegate.I.ReceiveReward(dailyReward.rewards);
        dailyReward.received = true;
    }

    DailyReward GetDailyReward(int day)
    {
        _dailyRewards.TryGetValue(day, out var result);
        return result;
    }

    void UpdateDailyReward()
    {

    }
}

public class DailyReward
{
    public int day;
    public List<Item> rewards = new List<Item>();
    public bool received;
    public bool canReceive;
}