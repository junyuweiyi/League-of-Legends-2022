
/********************************************************************
	created:	2020/09/17
	author:		maoqy
	
	purpose:	资源管理类，提供资源的更新，下载，加载，卸载等接口
                内部使用了Addressable系统，外部不要直接调用Addressable接口
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace iFramework
{
    internal class ResourceMgr : IResourceMgr, IFrameworkModule
    {
        bool _initialized = false;
        private Dictionary<string, string> _remoteInternalIds = new Dictionary<string, string>();
        private string _remoteBundleUrl; // Android/1.1.1/Android 这里是 1.1.1 处的位置

        /// <summary>
        /// 开始资源更新检查
        /// </summary>
        public Action OnCheckUpdateStart { get; set; }
        /// <summary>
        /// 资源更新完毕
        /// </summary>
        public Action OnCheckUpdateFinished { get; set; }

        /// <summary>
        /// 下载开始前的事件<br/>
        /// 会传入即将下载的大小<br/>
        /// 获取的 <c>IEnumerator&lt; bool &gt;</c> 指示了是否要下载<br/>
        /// 可以在这里弹一个对话框询问是否下载，通过 <c>IEnumerator&lt; bool &gt;</c> 通知下载是否继续
        /// </summary>
        public Func<long, IEnumerator<bool>> OnDownloadStart { get; set; }
        /// <summary>
        /// 资源下载完成的事件
        /// </summary>
        public Action<bool> OnDownloadFinished { get; set; }
        /// <summary>
        /// 资源下载进度事件
        /// </summary>
        public Action<float> OnDownloadProgress { get; set; }

        public Func<long, AsyncHandle<bool>> ShouldDownloadRemoteBundles { get; set; }
        public Action<DownloadStatus> OnDownloadRemoteBundlesProgress { get; set; }

        int IFrameworkModule.Priority => 1;

        public ResourceMgr()
        {
            Application.lowMemory += OnLowMemory;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        /// <summary>
        /// 初始化资源系统<br/>
        /// 这个接口必须在检查资源更新和加载资源之前调用
        /// </summary>
        /// <returns></returns>
        public AsyncOperationHandle<IResourceLocator> InitializeAsync()
        {
            var handle = Addressables.InitializeAsync();
            handle.AddCompleted(_ => _initialized = true);
            return handle;
        }

        public IEnumerator LoadRemoteCatalog(string catalogUrl, string bundleUrl, bool enableCache, Action<bool> onLoadFinished)
        {
            ContentCatalogProvider ccp = Addressables.ResourceManager.ResourceProviders
                .FirstOrDefault(rp => rp.GetType() == typeof(ContentCatalogProvider)) as ContentCatalogProvider;
            if (ccp != null)
                ccp.DisableCatalogUpdateOnStart = !enableCache; // enable cache and update

            var previousResourceLocators = Addressables.ResourceLocators.ToList();

            var loadOp = Addressables.LoadContentCatalogAsync(catalogUrl);
            yield return loadOp;
            var loadedLocator = loadOp.Result;
            Addressables.Release(loadOp);

            if (loadedLocator == null)
            {
                onLoadFinished?.InvokeSafely(false);
                yield break;
            }

            if (PickUpRemoteInternalIds(loadedLocator as ResourceLocationMap))
            {
                // 确定加载新Catalog成功之后才删除包内Catalog对应的Locator
                foreach (var locator in previousResourceLocators)
                {
                    Addressables.RemoveResourceLocator(locator);
                }

                _remoteBundleUrl = bundleUrl;
                Addressables.InternalIdTransformFunc = TransformRemoteInternalId;
                onLoadFinished?.InvokeSafely(true);
                yield break;
            }
            onLoadFinished?.InvokeSafely(false);
            yield break;
        }

        private bool PickUpRemoteInternalIds(ResourceLocationMap remoteLocator)
        {
            _remoteInternalIds.Clear();

            if (remoteLocator == null)
            {
                Debug.LogErrorFormat("PickUpRemoteInternalIds remote locator is null: {0}", remoteLocator);
                return false;
            }

            var localCatalogPath = Path.Combine(Addressables.RuntimePath, "catalog.json");

            ResourceLocationMap localLocator = null;
            try
            {
                var localCatalogJson = "";
                if (localCatalogPath.Contains("://")) // TODO 看情况是否需要改为异步
                {
                    UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(localCatalogPath);
                    www.SendWebRequest();
                    while (!www.isDone) { }
                    localCatalogJson = www.downloadHandler.text;
                }
                else
                {
                    localCatalogJson = File.ReadAllText(localCatalogPath);
                }

                var localCatalog = JsonUtility.FromJson<ContentCatalogData>(localCatalogJson);
                localLocator = localCatalog.CreateLocator();
            }
            catch (System.Exception e)
            {
                Debug.LogErrorFormat("PickUpRemoteInternalIds Error occurs when reading local catalog: {0}", localCatalogPath);
                Debug.LogException(e);
                return false;
            }

            if (localLocator == null)
            {
                Debug.LogErrorFormat("PickUpRemoteInternalIds local locator is null: {0}", localLocator);
                return false;
            }

            var localBundleHashes = new Dictionary<string, string>();

            foreach (var locations in localLocator.Locations.Values)
            {
                foreach (var loc in locations)
                {
                    if (loc.ResourceType == typeof(IAssetBundleResource))
                    {
                        var options = loc.Data as AssetBundleRequestOptions;
                        localBundleHashes[loc.InternalId] = options != null ? options.Hash + options.Crc : null;
                    }
                }
            }

            foreach (var locations in remoteLocator.Locations.Values)
            {
                foreach (var loc in locations)
                {
                    if (loc.ResourceType == typeof(IAssetBundleResource))
                    {
                        if (localBundleHashes.TryGetValue(loc.InternalId, out var localHash))
                        {
                            var remoteOptions = loc.Data as AssetBundleRequestOptions;
                            var remoteHash = remoteOptions != null ? remoteOptions.Hash + remoteOptions.Crc : null;
                            if (remoteHash != localHash)
                                _remoteInternalIds[loc.InternalId] = null;
                        }
                        else
                        {
                            _remoteInternalIds[loc.InternalId] = null;
                        }
                    }
                }
            }

            Debug.Log("remoteInternalIds " + _remoteInternalIds.Count);
            foreach (var i in _remoteInternalIds)
            {
                Debug.Log("rem " + i);
            }

            return true;
        }

        private string TransformRemoteInternalId(IResourceLocation location)
        {
            if (_remoteInternalIds.TryGetValue(location.InternalId, out var remoteInternalId))
            {
                return !string.IsNullOrEmpty(remoteInternalId)
                    ? remoteInternalId
                    : _remoteInternalIds[location.InternalId] = location.InternalId.Replace(Addressables.PlayerBuildDataPath, _remoteBundleUrl); // 将 StreamingPath 里面的路径替换为远程相同相对级别路径
            }

            return location.InternalId;
        }

        public IEnumerator DownloadRemoteBundles(IEnumerable keys, Action<bool> onDownloadFinished)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(keys);
            yield return sizeHandle;
            var downloadSize = sizeHandle.Result;
            Addressables.Release(sizeHandle);

            // 提示下载大小
            Debug.Log("下载大小 " + downloadSize);
            if (downloadSize > 0)
            {
                var shouldDownload = false;
                if (ShouldDownloadRemoteBundles != null)
                {
                    var askHandle = ShouldDownloadRemoteBundles(downloadSize);
                    yield return askHandle;
                    shouldDownload = askHandle.Result;
                }

                if (shouldDownload)
                {
                    var downloadHandle = Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union, false);
                    while (!downloadHandle.IsDone)
                    {
                        // 这里每帧都要创建一个 new HashSet<object>
                        var status = downloadHandle.GetDownloadStatus();

                        // 提示已下载 percent
                        Debug.LogFormat("已下载 {0}/{1} {2}", status.DownloadedBytes, status.TotalBytes, status.Percent);
                        OnDownloadRemoteBundlesProgress?.InvokeSafely(status);

                        yield return null;
                    }

                    var suc = downloadHandle.Status == AsyncOperationStatus.Succeeded;
                    Addressables.Release(downloadHandle);

                    Debug.LogFormat("下载结束 {0}", suc);
                    onDownloadFinished?.InvokeSafely(suc);
                    yield break;
                }

                onDownloadFinished?.InvokeSafely(false);
                yield break;
            }
            onDownloadFinished?.InvokeSafely(true);
            yield break;
        }

        /// <summary>
        /// 检查资源更新并下载<br/>
        /// 这之前需要先调用 <c>InitializeAsync()</c>
        /// </summary>
        /// <returns></returns>
        [Obsolete("不用这种方式，用手动加载Catalog的方式")]
        IEnumerator CheckUpdateAndDownload()
        {
            // 提示检查更新
            Debug.Log("检查更新");
            OnCheckUpdateStart?.InvokeSafely();

            if (!_initialized)
            {
                yield return InitializeAsync();
            }

            var checkForCatalogUpdatesHandle = Addressables.CheckForCatalogUpdates(false);
            yield return checkForCatalogUpdatesHandle;

            var catalogKeys = checkForCatalogUpdatesHandle.Result;
            Addressables.Release(checkForCatalogUpdatesHandle);

            Debug.Log("CheckForCatalogUpdates " + catalogKeys.Count);

            if (catalogKeys.Count > 0)
            {
                var updateCataLogsHandle = Addressables.UpdateCatalogs(catalogKeys, false);
                yield return updateCataLogsHandle;
                Addressables.Release(updateCataLogsHandle);
            }

            // Addressables.UpdateCatalogs()返回的Locator与Addressables.ResourceLocators实际上相同
            var allKeys = Addressables.ResourceLocators.SelectMany(l => l.Keys);
            Debug.Log("keys " + allKeys.Count());

            if (allKeys.Count() > 0)
            {
                var sizeHandle = Addressables.GetDownloadSizeAsync(allKeys);
                yield return sizeHandle;
                var downloadSize = sizeHandle.Result;
                Addressables.Release(sizeHandle);

                // 提示下载大小
                Debug.Log("下载大小 " + downloadSize);
                if (downloadSize > 0)
                {
                    var shouldDownload = true;
                    if (OnDownloadStart != null)
                    {
                        var e = OnDownloadStart(downloadSize);
                        yield return e;
                        shouldDownload = e.Current;
                    }

                    if (shouldDownload && downloadSize > 0)
                    {
                        var downloadHandle = Addressables.DownloadDependenciesAsync(allKeys, Addressables.MergeMode.Union, false);
                        while (!downloadHandle.IsDone)
                        {
                            // downloadHandle.GetDownloadStatus()
                            var percent = downloadHandle.PercentComplete;

                            // 提示已下载 percent
                            Debug.Log("已下载 " + percent);
                            OnDownloadProgress?.InvokeSafely(percent);

                            yield return null;
                        }

                        var suc = downloadHandle.Status == AsyncOperationStatus.Succeeded;
                        Addressables.Release(downloadHandle);

                        if (suc)
                        {
                            // 提示下载完成
                            Debug.Log("下载完成");
                            OnDownloadFinished?.InvokeSafely(true);
                        }
                        else
                        {
                            // 提示下载出错
                            Debug.Log("下载出错");
                            OnDownloadFinished?.InvokeSafely(false);
                        }
                    }
                }
            }
            OnCheckUpdateFinished?.InvokeSafely();
        }

        /// <summary>
        /// 同步加载资源<br/>
        /// 如果要从一个 Asset 里面加载一组 SubAsset 用这样的形式 <c>LoadAssetSync&lt;IList&lt; Sprite &gt;&gt;(spriteSheetName)</c>
        /// </summary>
        /// <param name="key">资源的Address</param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public TObject LoadAssetSync<TObject>(object key)
        {
            var op = Addressables.LoadAssetAsync<TObject>(key);
            try
            {
                return op.WaitForCompletion();
            }
            catch (System.Exception)
            {
                Release(op, -1);
                throw;
            }
        }

        public UnityEngine.Object LoadAssetSync(object key, Type type)
        {
            var asset = LoadAssetSync<UnityEngine.Object>(key);

            if (!(asset is null))
            {
                if (type == null || type.IsInstanceOfType(asset))
                    return asset;
                Debug.LogErrorFormat("Sync LoadAsset type mismatch {0} {1} {2}", key, type, asset);
                Release(asset);
            }
            return null;
        }

        /// <summary>
        /// Load a single asset
        /// See the [Loading Addressable Assets](../manual/LoadingAddressableAssets.html) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the asset.</typeparam>
        /// <param name="key">The key of the location of the asset.</param>
        /// <returns>Returns the load operation.</returns>
        public AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(object key)
        {
            return Addressables.LoadAssetAsync<TObject>(key);
        }

        /// <summary>
        /// Load all assets that match the provided key.
        /// See the [Loading Addressable Assets](../manual/LoadingAddressableAssets.html) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="key">Key for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation (per loaded asset).</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        /// When true, if any matching location fails, all loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        /// When false, if any matching location fails, the returned .Result will be an IList of size equal to the number of locations attempted.  Any failed location will
        /// correlate to a null in the IList, while successful loads will correlate to a TObject in the list. The .Status will still be Failed.
        /// When true, op does not need to be released if anything fails, when false, it must always be released.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(object key, Action<TObject> callback, bool releaseDependenciesOnFailure = true)
        {
            return Addressables.LoadAssetsAsync(key, callback, releaseDependenciesOnFailure);
        }

        /// <summary>
        /// Load multiple assets.
        /// Each key in the provided list will be translated into a list of locations.  Those many lists will be combined
        /// down to one based on the provided MergeMode.
        /// See the [Loading Addressable Assets](../manual/LoadingAddressableAssets.html) documentation for more details.
        /// </summary>
        /// <param name="keys">IEnumerable set of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        /// When true, if any matching location fails, all loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        /// When false, if any matching location fails, the returned .Result will be an IList of size equal to the number of locations attempted.  Any failed location will
        /// correlate to a null in the IList, while successful loads will correlate to a TObject in the list. The .Status will still be Failed.
        /// When true, op does not need to be released if anything fails, when false, it must always be released.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IEnumerable keys, Action<TObject> callback, Addressables.MergeMode mode, bool releaseDependenciesOnFailure = true)
        {
            return Addressables.LoadAssetsAsync(keys, callback, mode, releaseDependenciesOnFailure);
        }

        public void Release<TObject>(TObject obj, float delay = 0f)
        {
            if (delay > 0)
            {
                _delayReleaseObjects.Add(obj, Time.realtimeSinceStartup + delay);
            }
            else if (delay == 0 && _enableDefaultDelayRelease)
            {
                _delayReleaseObjects.Add(obj, Time.realtimeSinceStartup + _delayReleaseInterval.Value);
            }
            else
            {
                Addressables.Release(obj);
            }
        }

        public void Release(UnityEngine.Object obj, float delay = 0f)
        {
            Release<UnityEngine.Object>(obj, delay);
        }

        public void Release<TObject>(AsyncOperationHandle<TObject> handle, float delay = 0f)
        {
            Release((AsyncOperationHandle)handle, delay);
        }

        public void Release(AsyncOperationHandle handle, float delay = 0f)
        {
            if (delay > 0)
            {
                _delayReleaseHandles.Add(handle, Time.realtimeSinceStartup + delay);
            }
            else if (delay == 0 && _enableDefaultDelayRelease)
            {
                _delayReleaseHandles.Add(handle, Time.realtimeSinceStartup + _delayReleaseInterval.Value);
            }
            else
            {
                Addressables.Release(handle);
            }
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="instance">The GameObject instance to be released and destroyed.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public bool ReleaseInstance(GameObject instance)
        {
            return Addressables.ReleaseInstance(instance);
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="handle">The handle to the game object to destroy, that was returned by InstantiateAsync.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public bool ReleaseInstance(AsyncOperationHandle handle)
        {
            Addressables.Release(handle);
            return true;
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="handle">The handle to the game object to destroy, that was returned by InstantiateAsync.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public bool ReleaseInstance(AsyncOperationHandle<GameObject> handle)
        {
            Addressables.Release(handle);
            return true;
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](../manual/InstantiateAsync.html) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public AsyncOperationHandle<GameObject> InstantiateAsync(object key, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            return Addressables.InstantiateAsync(key, parent, instantiateInWorldSpace, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](../manual/InstantiateAsync.html) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public AsyncOperationHandle<GameObject> InstantiateAsync(object key, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
        {
            return Addressables.InstantiateAsync(key, position, rotation, parent, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](../manual/InstantiateAsync.html) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="instantiateParameters">Parameters for instantiation.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public AsyncOperationHandle<GameObject> InstantiateAsync(object key, InstantiationParameters instantiateParameters, bool trackHandle = true)
        {
            return Addressables.InstantiateAsync(key, instantiateParameters, trackHandle);
        }

        /// <summary>
        /// Load scene.
        /// See the [LoadSceneAsync](../manual/LoadSceneAsync.html) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request.</returns>
        public AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return Addressables.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="scene">The SceneInstance to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance scene, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(scene, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle handle, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(handle, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> handle, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(handle, autoReleaseHandle);
        }

        public void PostDefferredReleaseHandle(AsyncOperationHandle handle, float delay = 0f)
        {
            lock (_defferredReleaseHandles)
            {
                _defferredReleaseHandles.Add(handle, delay);
            }
        }

        public bool TryGetResourceLocations(string key, List<string> locations, Type type = null)
        {
            var op = Addressables.LoadResourceLocationsAsync(key, type);
            try
            {
                op.WaitForCompletion();

                if (locations == null)
                {
                    return op.Result.Any();
                }
                else
                {
                    locations.Clear();
                    locations.AddRange(op.Result.Select(ele => ele.PrimaryKey));
                    return locations.Count > 0;
                }
            }
            catch (System.Exception)
            {
                Release(op, -1);
                throw;
            }
        }

        public bool TryGetResourceLocations(string key, out List<string> locations, Type type = null)
        {
            locations = new List<string>();
            return TryGetResourceLocations(key, locations, type);
        }

        private List<KeyValuePair<AsyncOperationHandle, float>> _defferredReleaseHandles = new List<KeyValuePair<AsyncOperationHandle, float>>();

        private void ReleaseDefferredReleaseHandles()
        {
            if (_defferredReleaseHandles.Count > 0)
            {
                lock (_defferredReleaseHandles)
                {
                    if (_defferredReleaseHandles.Count > 0)
                    {
                        foreach (var handle in _defferredReleaseHandles)
                        {
                            if (handle.Key.IsValid())
                                Release(handle.Key, handle.Value);
                        }
                        _defferredReleaseHandles.Clear();
                    }
                }
            }
        }

        private List<KeyValuePair<object, float>> _delayReleaseObjects = new List<KeyValuePair<object, float>>();
        private List<KeyValuePair<AsyncOperationHandle, float>> _delayReleaseHandles = new List<KeyValuePair<AsyncOperationHandle, float>>();

        TimeIntervalProvider _delayReleaseInterval = new TimeIntervalProvider(30); // in seconds

        private void ReleaseExpiredAssets()
        {
            var curTime = Time.realtimeSinceStartup;

            for (var i = _delayReleaseObjects.Count - 1; i >= 0; i--)
            {
                if (_delayReleaseObjects[i].Value <= curTime)
                {
                    Release(_delayReleaseObjects[i].Key, -1);
                    _delayReleaseObjects.RemoveAt(i);
                }
            }
            for (var i = _delayReleaseHandles.Count - 1; i >= 0; i--)
            {
                if (_delayReleaseHandles[i].Value <= curTime)
                {
                    if (_delayReleaseHandles[i].Key.IsValid())
                        Release(_delayReleaseHandles[i].Key, -1);
                    _delayReleaseHandles.RemoveAt(i);
                }
            }
        }

        public void ForceReleaseDelayReleaseAssets()
        {
            // Debug.LogWarning("Warning memory low, force release cached assets");

            // TODO: 限定每帧释放个数，平滑释放
            foreach (var pair in _delayReleaseObjects)
                Release(pair.Key, -1);
            foreach (var pair in _delayReleaseHandles)
            {
                if (pair.Key.IsValid())
                    Release(pair.Key, -1);
            }

            _delayReleaseObjects.Clear();
            _delayReleaseHandles.Clear();
        }

        bool _enableDefaultDelayRelease = false;

        public void EnableDefaultDelayReleaseAsset(bool enable)
        {
            _enableDefaultDelayRelease = enable;
        }

        void IFrameworkModule.Update(float elapseSeconds, float realElapseSeconds)
        {
            ReleaseDefferredReleaseHandles();
            ReleaseExpiredAssets();
            _delayReleaseInterval.Update(realElapseSeconds);
        }

        void IFrameworkModule.Shutdown()
        {
            ClearAll();
            _remoteInternalIds.Clear();
            Application.lowMemory -= OnLowMemory;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        public void ClearAll()
        {
            // _remoteInternalIds.Clear(); // 不能清理这个字段，需要与 Addressable 里面的 Locator 保持一致
            ReleaseDefferredReleaseHandles();
            ForceReleaseDelayReleaseAssets();
        }

        void OnLowMemory()
        {
            ForceReleaseDelayReleaseAssets();
            _delayReleaseInterval.StartRecalculating();
        }

        void OnActiveSceneChanged(Scene old, Scene newScene)
        {
            // 切换场景时释放所有延迟释放的 Asset
            // ForceReleaseDelayReleaseAssets();
        }

        /// <summary>
        /// 目前暂定是低内存警告时立即释放所有延迟释放的Assets<br/>
        /// 之后延迟释放的时间从1秒开始翻倍逐渐增长
        /// </summary>
        class TimeIntervalProvider
        {
            public float Value { get; private set; }

            private float _initValue;
            private bool _recalculating;
            private float _elapsedTime;

            public TimeIntervalProvider(float initValue)
            {
                this.Value = _initValue = initValue;
                _recalculating = false;
                _elapsedTime = 0f;
            }

            public void StartRecalculating()
            {
                _recalculating = true;
                Value = 1;
                _elapsedTime = 0;
            }

            public void Update(float deltaTime)
            {
                if (_recalculating)
                {
                    _elapsedTime += deltaTime;
                    if (_elapsedTime >= Value)
                    {
                        _elapsedTime = 0;
                        Value *= 2;

                        if (Value >= _initValue)
                        {
                            Value = _initValue;
                            _recalculating = false;
                        }
                    }
                }
            }
        }
    }
}
