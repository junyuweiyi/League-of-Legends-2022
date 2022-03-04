using System.Collections.Generic;

/// <summary>
/// 方便Mgr直接互相调用
/// </summary>
public class MgrDelegate
{
    public static MgrDelegate I = new MgrDelegate();

    /// <summary>
    /// 领奖
    /// </summary>
    public void ReceiveReward(IEnumerable<Item> rewards)
    {
        BagMgr.I.AddItems(rewards);
    }


    public static List<ItemRandom> GenerateRandomRewards(IEnumerable<IReadOnlyList<ItemRandom>> itemRandomGroups)
    {
        var result = new List<ItemRandom>();
        foreach (var group in itemRandomGroups)
        {
            var random = WeightRandomUtils.Random(group);
            if (random == null)
                continue;
            result.Add(random as ItemRandom);
        }
        return result;
    }
}

public class ItemRandom : WeightRandomUtils.IWeightRandomObject
{
    public int Weight => 0;

    public Item Item;
}