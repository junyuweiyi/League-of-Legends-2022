using UnityEngine;

public interface IResourceMgr
{
    T LoadAssetSync<T>(string file) where T : Object;
    void Release(Object obj);
}
