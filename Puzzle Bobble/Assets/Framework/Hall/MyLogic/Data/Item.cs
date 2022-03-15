[System.Serializable]
public class Item
{
    public int ID;
    public int Count;

    public Item()
    {
        ID = 0;
        Count = 0;
    }

    public Item(int itemID, int itemCount)
    {
        ID = itemID;
        Count = itemCount;
    }
}