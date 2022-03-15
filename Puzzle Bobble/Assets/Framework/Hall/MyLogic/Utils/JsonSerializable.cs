using UnityEngine;

[System.Serializable]
public class JsonSerializable<T> where T : new()
{
    [SerializeField]
    T serializable;

    public static T Decode(string json)
    {
        var jsonSerializable = JsonUtility.FromJson<JsonSerializable<T>>(json);
        if (jsonSerializable != null)
            return jsonSerializable.serializable;
        return new T();
    }

    public static string Encode(T serializable)
    {
        var jsonSerializable = new JsonSerializable<T>();
        jsonSerializable.serializable = serializable;
        return JsonUtility.ToJson(jsonSerializable, true);
    }
}