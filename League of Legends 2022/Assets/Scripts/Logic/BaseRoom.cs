using System.Collections.Generic;
using iFramework;

public class BaseRoom : IReference
{
    public bool IsFull => _matchUsers.Count == _capacity;
    public int Count => _matchUsers.Count;
    public List<long> MatchUsers => _matchUsers;

    readonly List<long> _matchUsers;

    protected int _capacity;

    public BaseRoom(int capacity)
    {
        _capacity = capacity;
        _matchUsers = new List<long>(_capacity);
    }

    public void AddUser(long user)
    {
        _matchUsers.Add(user);
        OnAddUser(user);
        if (_matchUsers.Count == _matchUsers.Capacity)
        {
            OnRoomFull();
        }
    }

    public void RemoveUser(long user)
    {
        _matchUsers.Remove(user);
        OnRemoveUser(user);
    }

    protected virtual void OnAddUser(long user) { }
    protected virtual void OnRemoveUser(long user) { }
    protected virtual void OnRoomFull() { }

    public virtual void Clear()
    {
        _matchUsers.Clear();
    }

    public void NotifyPlayers(string msg)
    {
        foreach (var player in _matchUsers)
        {
            //TODO
            //player.SendMsg();
        }
    }
}