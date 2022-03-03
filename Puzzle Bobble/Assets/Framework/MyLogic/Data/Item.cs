[System.Serializable]
public class Item
{
    public int ID;
    public int Count;

    public Item(int itemID, int itemCount)
    {
        ID = itemID;
        Count = itemCount;
    }
}