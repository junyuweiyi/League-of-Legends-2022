using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserInfoMgr
{
    public static UserInfoMgr I = new UserInfoMgr();
    /// <summary>
    /// 当前关数
    /// </summary>
    public int CurrentLevel => LevelsMap._instance.GetMapLevels().Where(l => !l.IsLocked).Select(l => l.Number).Max();
}
