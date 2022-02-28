using System.Collections.Generic;

public class RoomPlayerContainer<T> where T : BaseRoom
{
    public IEnumerable<T> Rooms => _rooms;

    readonly List<T> _rooms = new List<T>();
    readonly Dictionary<long, T> _playerRoomMapper = new Dictionary<long, T>();

    public void AddPlayer(long player, T room)
    {
        room.AddUser(player);
        _playerRoomMapper[player] = room;
    }

    public void RemovePlayer(long player)
    {
        if (_playerRoomMapper.TryGetValue(player, out var room))
        {
            room.RemoveUser(player);
            _playerRoomMapper.Remove(player);
            if (room.Count == 0)
            {
                _rooms.Remove(room);
            }
        }
    }

    public void AddRoom(T room)
    {
        if (_rooms.Contains(room))
            return;
        foreach (var player in room.MatchUsers)
        {
            AddPlayer(player, room);
        }
    }

    public void RemoveRoom(T room)
    {
        _rooms.Remove(room);
        List<long> toRemove = new List<long>();
        foreach (var kv in _playerRoomMapper)
        {
            if (kv.Value == room)
                toRemove.Add(kv.Key);
        }
        foreach (var player in toRemove)
        {
            _playerRoomMapper.Remove(player);
        }
    }

    public void Clear()
    {
        _rooms.Clear();
        _playerRoomMapper.Clear();
    }
}
