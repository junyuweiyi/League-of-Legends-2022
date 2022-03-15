public class GameMgr<T> : IDataStorage where T : new()
{
    public static T I { get; } = new T();



    public virtual void LoadData() { }
    public virtual void SaveData() { }
}

public interface IDataStorage
{
    void LoadData();
    void SaveData();
}