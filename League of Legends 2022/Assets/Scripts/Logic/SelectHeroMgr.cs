using System.Collections.Generic;
using UnityEngine;
using iFramework;
using protocol;
using System;

/// <summary>
/// 选择英雄Mgr
/// </summary>
public class SelectHeroMgr : GameMgr<SelectHeroMgr>
{
    public event Action OnRoomClose;

    public const int kSelectHeroDuration = 10;

    readonly List<SelectHeroRoom> _selectHeroRooms = new List<SelectHeroRoom>();
    readonly Dictionary<long, SelectHeroRoom> _userRoomMapper = new Dictionary<long, SelectHeroRoom>();

    public override void Initialize()
    {
        base.Initialize();
        ServerNetMgr.I.RegisterMsg<SelectHeroReq>(this, OnSelectHeroReq);
        ServerNetMgr.I.RegisterMsg<SelectHeroSummonerSkillReq>(this, OnSelectHeroSummonerSkillReq);
        ServerNetMgr.I.OnUserDisconnect += OnUserDisconnect;
    }

    public override void Dispose()
    {
        base.Dispose();
        ServerNetMgr.I.UnregisterMsgs(this);
        ServerNetMgr.I.OnUserDisconnect -= OnUserDisconnect;
    }

    /// <summary>
    /// 进入英雄选择
    /// </summary>
    /// <param name="blueUsers"></param>
    /// <param name="redUsers"></param>
    public void Start(IEnumerable<long> blueUsers, IEnumerable<long> redUsers)
    {
        var room = CreateRoom(blueUsers, redUsers);
        foreach (var user in room.MatchUsers)
        {
            ServerNetMgr.I.SendMsg(user, new StartSelectHeroNtf());
        }
    }

    /// <summary>
    /// 英雄选择结束
    /// </summary>
    /// <param name="room"></param>
    public void Finish(SelectHeroRoom room, bool suc)
    {
        if(suc)
        {
            CloseRoom(room);
            //TODO 进入战斗流程
        }
        else
        {
            ReMachting(room);
        }
    }

    void CloseRoom(SelectHeroRoom room)
    {
        _selectHeroRooms.Remove(room);
        var toRemove = new List<long>();
        foreach (var kv in _userRoomMapper)
        {
            if (kv.Value == room)
                toRemove.Add(kv.Key);
        }
        foreach (var user in toRemove)
        {
            _userRoomMapper.Remove(user);
        }
        foreach (var tempUser in room.MatchUsers)
        {
            ServerNetMgr.I.SendMsg(tempUser, new CloseSelectHeroNtf());
        }
        room.Dispose();
        ReferencePool.Release(room);
        OnRoomClose.InvokeSafely();
    }

    SelectHeroRoom CreateRoom(IEnumerable<long> blueUsers, IEnumerable<long> redUsers)
    {
        var result = ReferencePool.Acquire<SelectHeroRoom>();
        result.Initialize();
        result.AddUsers(blueUsers, redUsers);
        _selectHeroRooms.Add(result);
        foreach (var user in result.MatchUsers)
        {
            _userRoomMapper[user] = result;
        }
        return result;
    }

    void SelectHero(long user, int heroID)
    {
        var room = GetRoom(user);
        if (room == null)
            return;

        room.SelectHero(user, heroID);
        ServerNetMgr.I.SendMsg(user, new SelectHeroAck() { Code = ErrCode.OK, HeroID = heroID });
        foreach (var tempUser in room.MatchUsers)
        {
            ServerNetMgr.I.SendMsg(tempUser, new SelectHeroNtf() { UserID = user, HeroID = heroID });
        }
    }

    void SelectSummonerSkill(long user, int slot, int summonerSkillID)
    {
        var room = GetRoom(user);
        if (room == null)
            return;

        room.SelectSummonerSkill(user, slot, summonerSkillID);
        ServerNetMgr.I.SendMsg(user, new SelectSummonerSkillAck { Code = ErrCode.OK, Slot = slot, SummonerSkillID = summonerSkillID });
        foreach (var tempUser in room.MatchUsers)
        {
            ServerNetMgr.I.SendMsg(tempUser, new SelectSummonerSkillNtf { UserID = user, Slot = slot, SummonerSkillID = summonerSkillID });
        }
    }

    SelectHeroRoom GetRoom(long user)
    {
        if (!_userRoomMapper.TryGetValue(user, out var room))
        {
            Debug.LogError("SelectHeroMgr没有找到该玩家所在Room, 请检查服务器代码");
            return null;
        }
        return room;
    }

    //重新进入匹配
    void ReMachting(SelectHeroRoom room)
    {
        List<long> usersInRoom = new List<long>(room.MatchUsers);
        //有人掉了直接退出选英雄
        CloseRoom(room);
        //重新进入匹配
        MatchingMgr.I.AddUsers(usersInRoom);
    }





    //****************************Net****************************
    void OnUserDisconnect(long user)
    {
        var room = GetRoom(user);
        if (room == null)
            return;

        room.RemoveUser(user);
        ReMachting(room);
    }

    void OnSelectHeroReq(long user, SelectHeroReq req)
    {
        SelectHero(user, req.HeroID);
    }

    void OnSelectHeroSummonerSkillReq(long user, SelectHeroSummonerSkillReq req)
    {
        SelectSummonerSkill(user, req.Slot, req.SummonerSkillID);
    }
}
