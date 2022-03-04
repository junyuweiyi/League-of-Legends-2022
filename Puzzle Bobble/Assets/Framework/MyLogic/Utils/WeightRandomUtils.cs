using System.Collections.Generic;

public static class WeightRandomUtils
{
    public interface IWeightRandomObject
    {
        int Weight { get; }
    }

    /// <summary>
    /// 带权重的随机
    /// </summary>
    /// <param name="sourceList">原始列表</param>
    /// <param name="resultCount">随机抽取条数</param>
    /// <returns></returns>
    public static IWeightRandomObject Random(IReadOnlyList<IWeightRandomObject> sourceList)
    {
        if (sourceList == null || sourceList.Count == 0)
        {
            return null;
        }

        //计算权重总和
        int totalWeights = 0;
        foreach (var weightRandomObject in sourceList)
            totalWeights += weightRandomObject.Weight + 1;  //权重+1，防止为0情况。

        //随机赋值权重
        System.Random ran = new System.Random(GetRandomSeed());
        //第一个int为list下标索引、第一个int为权重排序值
        var tempList = new List<KeyValuePair<int, int>>();
        for (int i = 0; i < sourceList.Count; i++)
        {
            // 从0到（总权重-1）的随机数
            int w = sourceList[i].Weight + ran.Next(0, totalWeights) - 1;
            tempList.Add(new KeyValuePair<int, int>(i, w));
        }

        //排序
        tempList.Sort((KeyValuePair<int, int> kvp1, KeyValuePair<int, int> kvp2) =>
            kvp2.Value - kvp1.Value);

        return sourceList[tempList[0].Key];
    }

    /// <summary>
    /// 随机种子值
    /// </summary>
    /// <returns></returns>
    static int GetRandomSeed()
    {
        byte[] bytes = new byte[4];
        System.Security.Cryptography.RNGCryptoServiceProvider rng =
            new System.Security.Cryptography.RNGCryptoServiceProvider();
        rng.GetBytes(bytes);
        return System.BitConverter.ToInt32(bytes, 0);
    }

}