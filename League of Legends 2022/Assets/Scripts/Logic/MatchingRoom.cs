using System.Collections.Generic;

/// <summary>
/// 匹配房间
/// </summary>
public sealed class MatchingRoom : RaceRoom, IMatchingUnit
{
    int IMatchingUnit.MaxCount => _capacity;
    IReadOnlyCollection<long> IMatchingUnit.Users => MatchUsers;

    public void Initialize()
    {
        MatchingMgr.I.MatchingController.Add(this);
    }

    public void Dispose()
    {
        MatchingMgr.I.MatchingController.Remove(this);
    }

    void IMatchingUnit.Add(long user)
    {
        AddUser(user);
    }

    void IMatchingUnit.MatchingComplete(bool suc)
    {
        MatchingMgr.I.OnMathingCompleted(this);
    }
}