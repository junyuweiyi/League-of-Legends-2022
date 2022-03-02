using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InitScriptName;
using System;

public class UserInfoMgr
{
    public static UserInfoMgr I = new UserInfoMgr();

    public UserInfoMgr()
    {
        UnlimitedLifeTargetTimeStamp = double.Parse(PlayerPrefs.GetString("UnlimitedLifeTargetTimeStamp", "0"));
    }

    public double UnlimitedLifeTargetTimeStamp;
    public float RestLifeTimer => InitScript.RestLifeTimer;

    /// <summary>
    /// 当前关数
    /// </summary>
    public int CurrentLevel => LevelsMap._instance.GetMapLevels().Where(l => !l.IsLocked).Select(l => l.Number).Max();

    /// <summary>
    /// 重置体力
    /// </summary>
    public void ResetLife()
    {
        InitScript.Instance.RestoreLifes();
    }
    /// <summary>
    /// 增加体力
    /// </summary>
    /// <param name="count"></param>
    public void AddLife(int count)
    {
        InitScript.Instance.AddLife(count);
    }
    /// <summary>
    /// 扣除体力
    /// </summary>
    /// <param name="count"></param>
    public void SpendLife(int count)
    {
        if (IsUnlimitLife())//此期间无限体力
            return;
        InitScript.Instance.SpendLife(count);
    }

    bool IsUnlimitLife()
    {
        if (UnlimitedLifeTargetTimeStamp == 0)
            return false;
        var diff = GetNow() - UnlimitedLifeTargetTimeStamp;
        return diff <= 2 * 3600;//2小时
    }

    public void StartUnlimitedLife()
    {
        UnlimitedLifeTargetTimeStamp = GetNow();
        PlayerPrefs.SetString("UnlimitedLifeTargetTimeStamp", UnlimitedLifeTargetTimeStamp.ToString());
    }

    double GetNow()
    {
        TimeSpan st = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
        return st.TotalSeconds;
    }
}
