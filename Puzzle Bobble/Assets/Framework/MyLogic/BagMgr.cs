using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ItemType
{
    Boost,
    Powerup,
}

public class BagMgr
{
    public static BagMgr I = new BagMgr();

    Dictionary<int, Item> _items;

    public BagMgr()
    {
        Decode();
    }

    void Decode()
    {
        string json = PlayerPrefs.GetString("Bag-Items");
        var list = JsonSerializable<List<Item>>.Decode(json);
        _items = list.ToDictionary(item => item.ID, item => item);
    }

    void Encode()
    {
        string json = JsonSerializable<List<Item>>.Encode(_items.Values.ToList());
        PlayerPrefs.SetString("Bag-Items", json);
    }

    public void AddItem(int itemID, int itemCount)
    {
        var item = GetItem(itemID);
        if(item == null)
        {
            item = new Item(itemID, 0);
            _items[item.ID] = item;
        }
        item.Count += itemCount;

        Encode();
    }

    public void AddItems(IEnumerable<Item> items)
    {
        foreach (var item in items)
        {
            AddItem(item.ID, item.Count);
        }
    }

    public void RemoveItem(int itemID, int count)
    {
        var item = GetItem(itemID);
        if (item == null)
        {
            return;
        }
        item.Count -= count;
        if (item.Count == 0)
            _items.Remove(itemID);

        Encode();
    }

    public List<Item> GetItems()
    {
        return _items.Values.ToList();
    }

    Item GetItem(int itemID)
    {
        _items.TryGetValue(itemID, out var item);
        return item;
    }
}