using System.Collections.Generic;

public class RaceRoom : BaseRoom
{
    public const int kDefaultCapacity = 6;

    readonly List<long> _blueUsers;
    readonly List<long> _redUsers;

    public List<long> BlueUsers => _blueUsers;
    public List<long> RedUsers => _redUsers;

    public RaceRoom() : base(kDefaultCapacity)
    {
        _blueUsers = new List<long>(_capacity / 2);
        _redUsers = new List<long>(_capacity / 2);
    }

    public RaceRoom(int capacity) : base(capacity)
    {
        _blueUsers = new List<long>(_capacity / 2);
        _redUsers = new List<long>(_capacity / 2);
    }

    protected override void OnAddUser(long user)
    {
        if (_blueUsers.Count > _redUsers.Count)
        {
            _redUsers.Add(user);
        }
        else
        {
            _blueUsers.Add(user);
        }
    }

    protected override void OnRemoveUser(long user)
    {
        _redUsers.Remove(user);
        _blueUsers.Remove(user);
    }

    public override void Clear()
    {
        base.Clear();
        _blueUsers.Clear();
        _redUsers.Clear();
    }
}
