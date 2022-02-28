using iFramework;
using System;
using System.Collections.Generic;

/// <summary>
/// 匹配系统Mgr
/// </summary>
public class MatchingMgr : GameMgr<MatchingMgr>
{
    public event Action<MatchingRoom> OnMatchingComplete;

    public MatchingController MatchingController => _matchingController;

    readonly MatchingController _matchingController = new MatchingController();
    IMatchingRule _matchingRule;
    readonly List<MatchingRoom> _matchingRooms = new List<MatchingRoom>();
    readonly Dictionary<long, MatchingRoom> _userRoomMapper = new Dictionary<long, MatchingRoom>();

    public override void Initialize()
    {
        base.Initialize();
        SetMatchingRule(SimpleMatchingRule.Instance);
    }

    public override void Dispose()
    {
        base.Dispose();
        foreach (var room in _matchingRooms)
        {
            ReferencePool.Release(room);
        }
        _matchingRooms.Clear();
        _userRoomMapper.Clear();
    }

    void SetMatchingRule(IMatchingRule matchingRule)
    {
        _matchingRule = matchingRule;
        _matchingController.SetMatchingRule(matchingRule);
    }

    public void AddUsers(IEnumerable<long> users)
    {
        foreach (var user in users)
        {
            AddUser(user);
        }
    }

    public void AddUser(long user)
    {
        if (!_matchingController.Matching(user))
        {
            CreateMatchingRoom(user);
        }
    }

    public void RemoveUser(long user)
    {
        if (_userRoomMapper.TryGetValue(user, out var room))
        {
            room.RemoveUser(user);
            if (room.Count == 0)
            {
                RemoveMatchingRoom(room);
            }
        }
    }

    public void OnMathingCompleted(MatchingRoom matchingRoom)
    {
        matchingRoom.NotifyUsers("MatchComplete");
        OnMatchingComplete?.InvokeSafely(matchingRoom);
        //匹配成功，交给SelectHeroMgr处理
        SelectHeroMgr.I.StartSelectHero(matchingRoom.MatchUsers);
        RemoveMatchingRoom(matchingRoom);
    }

    MatchingRoom CreateMatchingRoom(long user)
    {
        var result = ReferencePool.Acquire<MatchingRoom>();
        result.Initialize();
        result.AddUser(user);
        _matchingRooms.Add(result);
        _userRoomMapper[user] = result;
        return result;
    }

    void RemoveMatchingRoom(MatchingRoom matchingRoom)
    {
        matchingRoom.Dispose();
        _matchingRooms.Remove(matchingRoom);
        List<long> toRemove = new List<long>();
        foreach (var kv in _userRoomMapper)
        {
            if (kv.Value == matchingRoom)
                toRemove.Add(kv.Key);
        }
        foreach (var user in toRemove)
        {
            _userRoomMapper.Remove(user);
        }
        ReferencePool.Release(matchingRoom);
    }
}