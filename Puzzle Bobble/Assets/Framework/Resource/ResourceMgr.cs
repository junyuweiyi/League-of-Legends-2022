using UnityEngine;
namespace iFramework
{
    internal class ResourceMgr : IResourceMgr
    {
        T IResourceMgr.LoadAssetSync<T>(string file)
        {
            return Resources.Load<T>(file);
        }

        void IResourceMgr.Release(Object obj)
        {
            Resources.UnloadAsset(obj);
        }
    }
}