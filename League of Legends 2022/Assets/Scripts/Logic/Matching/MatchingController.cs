using System.Collections.Generic;

/// <summary>
/// 匹配控制器
/// </summary>
public class MatchingController
{
    readonly HashSet<IMatchingUnit> _matchingUnits = new HashSet<IMatchingUnit>();
    IMatchingRule _matchingRule;

    public void SetMatchingRule(IMatchingRule matchingRule)
    {
        _matchingRule = matchingRule;
    }

    public bool Matching(long user)
    {
        IMatchingUnit matchingUnit = DoMatching(user);
        if (matchingUnit == null)
            return false;
        if (matchingUnit.Users.Count == matchingUnit.MaxCount)
        {
            matchingUnit.MatchingComplete(true);
            _matchingUnits.Remove(matchingUnit);
        }
        return true;
    }

    IMatchingUnit DoMatching(long user)
    {
        foreach (var matchingUnit in _matchingUnits)
        {
            if (_matchingRule?.Matching(matchingUnit, user) ?? false)
            {
                return matchingUnit;
            }
        }
        return null;
    }

    public void Add(IMatchingUnit matchingUnit)
    {
        _matchingUnits.Add(matchingUnit);
    }

    public void Remove(IMatchingUnit matchingUnit)
    {
        _matchingUnits.Remove(matchingUnit);
    }
}

public interface IMatchingUnit
{
    int MaxCount { get; }
    IReadOnlyCollection<long> Users { get; }
    void Add(long user);
    void MatchingComplete(bool suc);
}

public interface IMatchingRule
{
    bool Matching(IMatchingUnit matchingUnit, long user);
}