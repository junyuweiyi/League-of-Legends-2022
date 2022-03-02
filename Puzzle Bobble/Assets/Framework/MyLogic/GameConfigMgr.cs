using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfigMgr
{
    public static GameConfigMgr I = new GameConfigMgr();


    public int CapOfLife => LevelEditorBase.THIS.CapOfLife;
    public float TotalTimeForRestLifeHours => LevelEditorBase.THIS.TotalTimeForRestLifeHours;
    public float TotalTimeForRestLifeMin => LevelEditorBase.THIS.TotalTimeForRestLifeMin;
    public float TotalTimeForRestLifeSec => LevelEditorBase.THIS.TotalTimeForRestLifeSec;
    public int CostIfRefill => LevelEditorBase.THIS.CostIfRefill;
    public int CostTwoOursUnlimitedLife => LevelEditorBase.THIS.CostTwoOursUnlimitedLife;
    public int RevertLifeWhenWatchAD => LevelEditorBase.THIS.RevertLifeWhenWatchAD;
}
