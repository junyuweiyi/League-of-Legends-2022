﻿
using UnityEngine;

public class PlayerPrefsMapProgressManager : IMapProgressManager
{
    private string GetLevelKey(int number)
    {
        return string.Format("Level.{0:000}.StarsCount", number);
    }

    public int LoadLevelStarsCount(int level)
    {
        return PlayerPrefs.GetInt(GetLevelKey(level), 0);
    }

    public void SaveLevelStarsCount(int level, int starsCount)
    {
        PlayerPrefs.SetInt(GetLevelKey(level), starsCount);
    }

    public void ClearLevelProgress(int level)
    {
        PlayerPrefs.DeleteKey(GetLevelKey(level));
    }

    public int GetTotalLevelStar()
    {
        int result = 0;
        int level = 0;
        while (PlayerPrefs.HasKey(GetLevelKey(level)))
        {
            result += LoadLevelStarsCount(level);
            level++;
        }
        return result;
    }
}
