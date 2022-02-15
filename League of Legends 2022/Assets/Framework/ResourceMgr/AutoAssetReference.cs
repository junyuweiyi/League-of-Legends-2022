/********************************************************************
	created:	2020/09/21
	author:		maoqy
	
	purpose:	自动释放的AssetReference，当没有任何引用指向这个对象导致这个对象被GC回收时，会释放这个对象加载的Asset
                另外提供一个同步加载资源的接口
*********************************************************************/

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace iFramework
{
    [System.Serializable]
    public class AutoAssetReference : AssetReference
    {
        private IResourceMgr _resourceMgr;
        private float _delayRelease;

        /// <summary>
        /// 自动释放的AssetReference，当没有任何引用指向这个对象导致这个对象被GC回收时，会释放这个对象加载的Asset<br/>
        /// 如果手动调用了<c>ReleaseAsset()</c>，那么资源不会重复释放<br/>
        /// 另外提供一个同步加载Asset的接口<br/>
        /// 尽量使用这个接口来加载Asset，而不使用ResourceMgr来加载，因为使用ResourceMgr加载Asset需要手动释放<br/>
        /// eg. 
        /// <code>
        /// var reference = new AutoAssetReference("SomeAddress");<br/>
        /// var obj = reference.LoadAssetSync&lt; GameObject &gt;();<br/>
        /// // 当<c>obj</c>使用完成后，扔掉<c>reference</c><br/>
        /// reference = null; // 也可以作为成员变量跟随宿主生命周期
        /// </code>
        /// </summary>
        /// <param name="key">用于加载这个Asset的key，比如address, label</param>
        /// <param name="resourceMgr">用于释放这个Asset的ResourceMgr</param>
        /// <param name="delayRelease">是否延迟释放，以达到缓存一段时间的目的 大于0：延迟释放，小于0：不延迟释放，等于0：依赖默认行为</param>
        /// <returns></returns>
        public AutoAssetReference(string key, IResourceMgr resourceMgr, float delayRelease = 0f) : base(key)
        {
            _resourceMgr = resourceMgr;
            _delayRelease = delayRelease;
        }

        public override bool RuntimeKeyIsValid()
        {
            return !string.IsNullOrEmpty(RuntimeKey.ToString());
        }

        /// <summary>
        /// 加载Asset的同步接口
        /// </summary>
        /// <typeparam name="TObject">返回的Asset的类型</typeparam>
        /// <returns>加载的Asset</returns>
        public TObject LoadAssetSync<TObject>()
        {
            if (OperationHandle.IsValid())
            {
                Debug.LogError("Attempting to load AssetReference that has already been loaded. Handle is exposed through getter OperationHandle");
                return (TObject)OperationHandle.Result;
            }
            else
            {
                var op = LoadAssetAsync<TObject>();
                try
                {
                    return op.WaitForCompletion();
                }
                catch (System.Exception)
                {
                    base.ReleaseAsset();
                    throw;
                }
            }
        }

        public override void ReleaseAsset()
        {
            if (!OperationHandle.IsValid())
            {
                Debug.LogWarning("Cannot release a null or unloaded asset.");
                return;
            }
            Addressables.ResourceManager.Acquire(OperationHandle);
            _resourceMgr.Release(OperationHandle, _delayRelease);
            base.ReleaseAsset();
        }

        // 经测试，对象被解引用后，会在几秒内被GC回收同时调用析构
        // 打开Incremental GC后，往往会在几帧之间就会被回收
        // 所以可以将AutoAssetReference解引用作为一个释放资源的常规手段
        ~AutoAssetReference()
        {
            if (OperationHandle.IsValid())
            {
                // 析构函数在GC线程执行，所以这里不能直接调用Unity相关函数
                // 析构函数里面调用Unity函数不会报错，但会执行失败
                // Debug.Log()可以执行
                _resourceMgr.PostDefferredReleaseHandle(OperationHandle, _delayRelease);
            }
        }
    }
}