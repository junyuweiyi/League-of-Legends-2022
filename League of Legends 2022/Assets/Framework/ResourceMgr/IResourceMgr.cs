
/********************************************************************
	created:	2020/09/17
	author:		maoqy
	
	purpose:	资源管理类，提供资源的更新，下载，加载，卸载等接口
                内部使用了Addressable系统，外部不要直接调用Addressable接口
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace iFramework
{
    /// <summary>
    /// 资源管理接口，提供资源的更新，下载，加载，卸载等接口<br/>
    /// 内部使用了Addressable系统，外部不要直接调用Addressable接口
    /// </summary>
    public interface IResourceMgr
    {
        /// <summary>
        /// 是否预下载远程Bundle
        /// </summary>
        /// <value></value>
        Func<long, AsyncHandle<bool>> ShouldDownloadRemoteBundles { get; set; }
        /// <summary>
        /// 预下载远程Bundle进度事件
        /// </summary>
        /// <value></value>
        Action<DownloadStatus> OnDownloadRemoteBundlesProgress { get; set; }

        /// <summary>
        /// 初始化资源系统<br/>
        /// 这个接口必须在检查资源更新和加载资源之前调用
        /// </summary>
        /// <returns></returns>
        AsyncOperationHandle<IResourceLocator> InitializeAsync();
        /// <summary>
        /// 根据远端Catalog，检查Bundle更新<br/>
        /// 需要注意，如果远程Catalog无法访问，会使用之前Cache的Catalog，不会返回失败
        /// </summary>
        /// <param name="catalogUrl">远端Catalog地址</param>
        /// <param name="bundleUrl">远端Bundle地址</param>
        /// <returns></returns>
        IEnumerator LoadRemoteCatalog(string catalogUrl, string bundleUrl, bool enableCache, Action<bool> onLoadFinished);
        /// <summary>
        /// 下载远端需要更新的Bundle
        /// </summary>
        /// <returns></returns>
        IEnumerator DownloadRemoteBundles(IEnumerable keys, Action<bool> onDownloadFinished);

        /// <summary>
        /// 同步加载资源<br/>
        /// 如果要从一个 Asset 里面加载一组 SubAsset 用这样的形式 <c>LoadAssetSync&lt;IList&lt; Sprite &gt;&gt;(spriteSheetName)</c>
        /// </summary>
        /// <param name="key">资源的Address</param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        TObject LoadAssetSync<TObject>(object key);

        /// <summary>
        /// 同步加载资源<br/>
        /// 如果要从一个 Asset 里面加载一组 SubAsset 用这样的形式 <c>LoadAssetSync&lt;IList&lt; Sprite &gt;&gt;(spriteSheetName)</c><br/>
        /// 这个接口主要是为了兼容 lua，因为 lua 不方便调用泛型接口
        /// </summary>
        /// <param name="key">资源的Address</param>
        /// <param name="type"></param>
        /// <returns></returns>
        UnityEngine.Object LoadAssetSync(object key, Type type = null);

        /// <summary>
        /// Load a single asset
        /// See the [Loading Addressable Assets](../manual/LoadingAddressableAssets.html) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the asset.</typeparam>
        /// <param name="key">The key of the location of the asset.</param>
        /// <returns>Returns the load operation.</returns>
        AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(object key);

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
        AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(object key, Action<TObject> callback, bool releaseDependenciesOnFailure = true);

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
        AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IEnumerable keys, Action<TObject> callback, Addressables.MergeMode mode, bool releaseDependenciesOnFailure = true);

        /// <summary>
        /// Release asset.
        /// </summary>
        /// <typeparam name="TObject">The type of the object being released</typeparam>
        /// <param name="obj">The asset to release.</param>
        /// <param name="delay">是否延迟释放，以达到缓存一段时间的目的 大于0：延迟释放，小于0：不延迟释放，等于0：依赖默认行为</param>
        void Release<TObject>(TObject obj, float delay = 0f);

        /// <summary>
        /// Release asset.<br/>
        /// 这个接口主要是为了兼容 lua，因为 lua 不方便调用泛型接口
        /// </summary>
        /// <param name="obj">The asset to release.</param>
        /// <param name="delay">是否延迟释放，以达到缓存一段时间的目的 大于0：延迟释放，小于0：不延迟释放，等于0：依赖默认行为</param>
        void Release(UnityEngine.Object obj, float delay = 0f);

        /// <summary>
        /// Release the operation and its associated resources.
        /// </summary>
        /// <typeparam name="TObject">The type of the AsyncOperationHandle being released</typeparam>
        /// <param name="handle">The operation handle to release.</param>
        /// <param name="delay">是否延迟释放，以达到缓存一段时间的目的 大于0：延迟释放，小于0：不延迟释放，等于0：依赖默认行为</param>
        void Release<TObject>(AsyncOperationHandle<TObject> handle, float delay = 0f);

        /// <summary>
        /// Release the operation and its associated resources.
        /// </summary>
        /// <param name="handle">The operation handle to release.</param>
        /// <param name="delay">是否延迟释放，以达到缓存一段时间的目的 大于0：延迟释放，小于0：不延迟释放，等于0：依赖默认行为</param>
        void Release(AsyncOperationHandle handle, float delay = 0f);

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="instance">The GameObject instance to be released and destroyed.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        bool ReleaseInstance(GameObject instance);

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="handle">The handle to the game object to destroy, that was returned by InstantiateAsync.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        bool ReleaseInstance(AsyncOperationHandle handle);

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="handle">The handle to the game object to destroy, that was returned by InstantiateAsync.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        bool ReleaseInstance(AsyncOperationHandle<GameObject> handle);

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](../manual/InstantiateAsync.html) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        AsyncOperationHandle<GameObject> InstantiateAsync(object key, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true);

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
        AsyncOperationHandle<GameObject> InstantiateAsync(object key, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true);

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](../manual/InstantiateAsync.html) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="instantiateParameters">Parameters for instantiation.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        AsyncOperationHandle<GameObject> InstantiateAsync(object key, InstantiationParameters instantiateParameters, bool trackHandle = true);

        /// <summary>
        /// Load scene.
        /// See the [LoadSceneAsync](../manual/LoadSceneAsync.html) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request.</returns>
        AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100);

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="scene">The SceneInstance to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        AsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance scene, bool autoReleaseHandle = true);

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle handle, bool autoReleaseHandle = true);

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> handle, bool autoReleaseHandle = true);

        /// <summary>
        /// 投递需要延迟释放的 handle<br/>
        /// 将会在下一个 <c>Update()</c> 执行时释放<br/>
        /// 主要用于当前不方便释放的情况
        /// </summary>
        /// <param name="handle">需要被释放的handle</param>
        /// <param name="delay">
        /// 是否延迟释放，以达到缓存一段时间的目的<br/>
        /// 指定了这个标记，将会延后一段时间释放，而不会在下一个<c>Update()</c>释放<br/>
        /// 大于0：延迟释放，小于0：不延迟释放，等于0：依赖默认行为
        /// </param>
        void PostDefferredReleaseHandle(AsyncOperationHandle handle, float delay = 0f);
        bool TryGetResourceLocations(string key, List<string> locations, Type type = null);
        bool TryGetResourceLocations(string key, out List<string> locations, Type type = null);

        /// <summary>
        /// 设置是否默认延迟释放
        /// </summary>
        /// <param name="enable"></param>
        void EnableDefaultDelayReleaseAsset(bool enable);

        /// <summary>
        /// 释放所有延迟释放的 Asset
        /// </summary>
        void ForceReleaseDelayReleaseAssets();

        /// <summary>
        /// 清理可以卸载的资源，为重启做准备
        /// </summary>
        void ClearAll();
    }
}