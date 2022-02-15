/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	
*********************************************************************/
using System;
using UnityEngine;

namespace iFramework
{
    public interface IObjectPool
    {
        /// <summary>
        /// 清空缓存对象
        /// </summary>
        void ClearAllCatch();

        /// <summary>
        /// 按提供的prefab对象创建新的对象
        /// </summary>
        /// <param name="obj">prefab</param>
        /// <param name="activeDuration">可自动unspawn创建的对象（参数> 0时），
        /// 用于特效等播放完后自动unspawn的对象</param>
        /// <returns></returns>
        GameObject Spawn(GameObject obj, float activeDuration = -1);
        GameObject Spawn(GameObject obj, Vector3 pos, float activeDuration = -1);
        GameObject Spawn(GameObject obj, Vector3 pos, Quaternion rot, float activeDuration = -1);
        GameObject Spawn(GameObject obj, Transform parent, float activeDuration = -1);
        GameObject Spawn(GameObject obj, Transform parent, Vector3 pos, float activeDuration = -1);
        GameObject Spawn(GameObject obj, Transform parent, Vector3 pos, Quaternion rot, float activeDuration = -1);

        void AsyncSpawn(GameObject obj, Action<GameObject> callback, float activeDuration = -1, int priority = 0);
        void AsyncSpawn(GameObject obj, Vector3 pos, Action<GameObject> callback, float activeDuration = -1, int priority = 0);
        void AsyncSpawn(GameObject obj, Vector3 pos, Quaternion rot, Action<GameObject> callback, float activeDuration = -1, int priority = 0);
        void AsyncSpawn(GameObject obj, Transform parent, Action<GameObject> callback, float activeDuration = -1, int priority = 0);
        void AsyncSpawn(GameObject obj, Transform parent, Vector3 pos, Action<GameObject> callback, float activeDuration = -1, int priority = 0);
        void AsyncSpawn(GameObject obj, Transform parent, Vector3 pos, Quaternion rot, Action<GameObject> callback, float activeDuration = -1, int priority = 0);

        GameObject Spawn(string assetName, float activeDuration = -1);
        GameObject Spawn(string assetName, Vector3 pos, float activeDuration = -1);
        GameObject Spawn(string assetName, Vector3 pos, Quaternion rot, float activeDuration = -1);
        GameObject Spawn(string assetName, Transform parent, float activeDuration = -1);
        GameObject Spawn(string assetName, Transform parent, Vector3 pos, float activeDuration = -1);
        GameObject Spawn(string assetName, Transform parent, Vector3 pos, Quaternion rot, float activeDuration = -1);

        void AsyncSpawn(string assetName, Action<GameObject> callback, float activeDuration = -1, int priority = 0);
        void AsyncSpawn(string assetName, Vector3 pos, Action<GameObject> callback, float activeDuration = -1, int priority = 0);
        void AsyncSpawn(string assetName, Vector3 pos, Quaternion rot, Action<GameObject> callback, float activeDuration = -1, int priority = 0);
        void AsyncSpawn(string assetName, Transform parent, Action<GameObject> callback, float activeDuration = -1, int priority = 0);
        void AsyncSpawn(string assetName, Transform parent, Vector3 pos, Action<GameObject> callback, float activeDuration = -1, int priority = 0);
        void AsyncSpawn(string assetName, Transform parent, Vector3 pos, Quaternion rot, Action<GameObject> callback, float activeDuration = -1, int priority = 0);

        /// <summary>
        /// 取消 callback 对应的所有加载请求
        /// </summary>
        /// <param name="callback"></param>
        void CancleAsyncSpawns(Action<GameObject> callback);

        void Unspawn(GameObject obj, float delay, bool resetParent = true);
        void Unspawn(GameObject obj, bool resetParent = true);
    }
}