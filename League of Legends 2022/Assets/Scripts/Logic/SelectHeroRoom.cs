using System.Collections.Generic;
using System.Linq;

public class SelectHeroUser
{
    public long UserID { get; set; }
    public int HeroID { get; set; }
    public int[] SummonerSkill { set; get; }
    public bool IsBlue { get; set; }
}

public class SelectHeroRoom : RaceRoom
{
    readonly Dictionary<long, SelectHeroUser> _users = new Dictionary<long, SelectHeroUser>();

    readonly List<long> _blueUsers = new List<long>();
    readonly List<long> _redUsers = new List<long>();
    int _timerID;

    public List<long> BlueUsers => _blueUsers;
    public List<long> RedUsers => _redUsers;

    public void Initialize()
    {
        _timerID = FW.Timer.Timeout(SelectHeroMgr.kSelectHeroDuration, OnSelectHeroTimerOut);
    }

    public void Dispose()
    {
        if (_timerID != 0)
        {
            FW.Timer.Stop(_timerID);
            _timerID = 0;
        }
    }

    void OnSelectHeroTimerOut()
    {
        SelectHeroMgr.I.Finish(this, !_users.Values.Any(p => p.HeroID == 0);
    }

    public void AddUsers(IEnumerable<long> blueUsers, IEnumerable<long> redUsers)
    {        
        _users.Clear();
        var mergeUsers = blueUsers.Union(redUsers).Select(p => new SelectHeroUser() { UserID = p, HeroID = 0, SummonerSkill = new int[2] });
        foreach (var user in _users)
        {
            user.Value.IsBlue = blueUsers.Contains(user.Key);
            _users.Add(user.Key, user.Value);
        }

        _blueUsers.Clear();
        _blueUsers.AddRange(blueUsers);
        _redUsers.Clear();
        _redUsers .AddRange(redUsers);
    }

    public void SelectHero(long userID, int heroID)
    {
        GetUser(userID).HeroID = heroID;
    }

    public void SelectSummonerSkill(long userID, int slot, int summonerSkillID)
    {
        GetUser(userID).SummonerSkill[slot] = summonerSkillID;
    }

    protected override void OnRemoveUser(long userID)
    {
        _users.Remove(userID);
        _blueUsers.Remove(userID);
        _redUsers.Remove(userID);
        NotifyPlayers("RemoveUser");
    }

    SelectHeroUser GetUser(long userID)
    {
        if (!_users.TryGetValue(userID, out var user))
        {
            user = new SelectHeroUser { UserID = userID };
            _users[userID] = user;
        }
        return user;
    }
}