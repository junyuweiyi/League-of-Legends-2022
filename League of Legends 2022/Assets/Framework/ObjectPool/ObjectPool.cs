/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	
*********************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace iFramework
{
    internal class ObjectPool : MonoBehaviour, IObjectPool
    {
        private const int _kFrameCountToSpawnAll = 30;
        private const int _kMinSpawnCountPerFrame = 2;
        internal Action<ObjectPool> DestoryedEvent;
        public List<Pool> poolList = new List<Pool>();
        private Dictionary<string, GameObject> _assets = new Dictionary<string, GameObject>();
        private Queue<SpawnCommand> _commands = new Queue<SpawnCommand>();
        private Dictionary<Action<GameObject>, PooledHashSet<SpawnCommand>> _callback2Commands = new Dictionary<Action<GameObject>, PooledHashSet<SpawnCommand>>();
        private int _spawnPreFrame;

        public GameObject Spawn(string assetName, float activeDuration = -1)
        {
            return Spawn(assetName, null, activeDuration);
        }
        public GameObject Spawn(string assetName, Vector3 pos, float activeDuration = -1)
        {
            return Spawn(assetName, null, pos, activeDuration);
        }
        public GameObject Spawn(string assetName, Vector3 pos, Quaternion rot, float activeDuration = -1)
        {
            return Spawn(assetName, null, pos, rot, activeDuration);
        }
        public GameObject Spawn(string assetName, Transform parent, float activeDuration = -1)
        {
            return Spawn(GetAsset(assetName), parent, activeDuration);
        }
        public GameObject Spawn(string assetName, Transform parent, Vector3 pos, float activeDuration = -1)
        {
            return Spawn(GetAsset(assetName), parent, pos, activeDuration);
        }
        public GameObject Spawn(string assetName, Transform parent, Vector3 pos, Quaternion rot, float activeDuration = -1)
        {
            return Spawn(GetAsset(assetName), parent, pos, rot, activeDuration);
        }
        public void AsyncSpawn(string assetName, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            AsyncSpawn(assetName, null, callback, activeDuration, priority);
        }
        public void AsyncSpawn(string assetName, Vector3 pos, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            AsyncSpawn(assetName, null, pos, callback, activeDuration, priority);
        }
        public void AsyncSpawn(string assetName, Vector3 pos, Quaternion rot, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            AsyncSpawn(assetName, null, pos, rot, callback, activeDuration, priority);
        }
        public void AsyncSpawn(string assetName, Transform parent, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            AsyncSpawn(assetName, parent, false, false, default, default, callback, activeDuration, priority);
        }
        public void AsyncSpawn(string assetName, Transform parent, Vector3 pos, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            AsyncSpawn(assetName, parent, true, false, pos, default, callback, activeDuration, priority);
        }
        public void AsyncSpawn(string assetName, Transform parent, Vector3 pos, Quaternion rot, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            AsyncSpawn(assetName, parent, true, true, pos, rot, callback, activeDuration, priority);
        }
        void AsyncSpawn(string assetName, Transform parent, bool setPos, bool setRot, Vector3 pos, Quaternion rot, Action<GameObject> callback, float activeDuration, int priority)
        {
            GameObject asset;
            if (_assets.TryGetValue(assetName, out asset))
            {
                EnqueueCommand(asset, parent, setPos, setRot, pos, rot, callback, activeDuration, "", priority);
            }
            else
            {
                // 预留接口，后面看情况是否将 Queue 改为 Heap
                // 小于 0，排队加载
                if (priority < 0)
                {
                    EnqueueCommand(null, parent, setPos, setRot, pos, rot, callback, activeDuration, assetName, priority);
                }
                else // 默认直接加载 Asset 后 Spawn
                {
                    var cmd = CreateSpawnCommand(assetName, null, parent, setPos, setRot, pos, rot, callback, activeDuration);
                    ObjectPoolMgr.resMgr.LoadAssetAsync<GameObject>(assetName).AddCompleted((handler) =>
                    {
                        if (handler.Result != null)
                        {
                            if (!_assets.ContainsKey(assetName))
                            {
                                _assets.Add(assetName, handler.Result);
                            }

                            if (!cmd.cancled)
                            {
                                cmd.prefab = handler.Result;
                                EnqueueCommand(cmd);
                                return;
                            }
                        }

                        RecycleSpawnCommand(cmd);
                    });
                }
            }
        }

        public GameObject Spawn(GameObject obj, float activeDuration = -1)
        {
            return Spawn(obj, null, activeDuration);
        }

        public GameObject Spawn(GameObject obj, Vector3 pos, float activeDuration = -1)
        {
            return Spawn(obj, null, pos, activeDuration);
        }

        public GameObject Spawn(GameObject obj, Vector3 pos, Quaternion rot, float activeDuration = -1)
        {
            return Spawn(obj, null, pos, rot, activeDuration);
        }

        public GameObject Spawn(GameObject obj, Transform parent, float activeDuration = -1)
        {
            return Spawn(obj, parent, false, false, default, default, activeDuration);
        }

        public GameObject Spawn(GameObject obj, Transform parent, Vector3 pos, float activeDuration = -1)
        {
            return Spawn(obj, parent, true, false, pos, default, activeDuration);
        }

        public GameObject Spawn(GameObject obj, Transform parent, Vector3 pos, Quaternion rot, float activeDuration = -1)
        {
            return Spawn(obj, parent, true, true, pos, rot, activeDuration);
        }

        protected virtual GameObject Spawn(GameObject obj, Transform parent, bool setPos, bool setRot, Vector3 pos, Quaternion rot, float activeDuration = -1)
        {
            if (obj == null)
            {
                Debug.LogWarning("NullReferenceException: obj unspecified");
                return null;
            }

            int ID = GetPoolID(obj);

            if (ID == -1)
            {
                ID = NewPool(obj);
            }

            GameObject spawnedObj;

            if (setPos && setRot)
            {
                spawnedObj = poolList[ID].Spawn(parent, pos, rot);
            }
            else if (setPos)
            {
                spawnedObj = poolList[ID].Spawn(parent, pos);
            }
            else
            {
                spawnedObj = poolList[ID].Spawn(parent);
            }

            if (activeDuration > 0) StartCoroutine(UnspawnRoutine(spawnedObj, activeDuration, true));

            return spawnedObj;
        }

        GameObject Spawn(SpawnCommand command)
        {
            var go = Spawn(command.prefab, command.parent, command.setPos, command.setRot, command.pos, command.rot, command.activeDuration);
            command.callback?.InvokeSafely(go);
            return go;
        }

        public void AsyncSpawn(GameObject obj, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            AsyncSpawn(obj, null, callback, activeDuration, priority);
        }
        public void AsyncSpawn(GameObject obj, Vector3 pos, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            AsyncSpawn(obj, null, pos, callback, activeDuration, priority);
        }
        public void AsyncSpawn(GameObject obj, Vector3 pos, Quaternion rot, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            AsyncSpawn(obj, null, pos, rot, callback, activeDuration, priority);
        }

        public void AsyncSpawn(GameObject obj, Transform parent, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            EnqueueCommand(obj, parent, false, false, default, default, callback, activeDuration, "", priority);
        }
        public void AsyncSpawn(GameObject obj, Transform parent, Vector3 pos, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            EnqueueCommand(obj, parent, true, false, pos, default, callback, activeDuration, "", priority);
        }
        public void AsyncSpawn(GameObject obj, Transform parent, Vector3 pos, Quaternion rot, Action<GameObject> callback, float activeDuration = -1, int priority = 0)
        {
            EnqueueCommand(obj, parent, true, true, pos, rot, callback, activeDuration, "", priority);
        }

        void EnqueueCommand(GameObject obj, Transform parent, bool setPos, bool setRot, Vector3 pos, Quaternion rot, Action<GameObject> callback, float activeDuration, string assetName, int priority)
        {
            var cmd = CreateSpawnCommand(assetName, obj, parent, setPos, setRot, pos, rot, callback, activeDuration);
            EnqueueCommand(cmd);
        }

        void EnqueueCommand(SpawnCommand cmd)
        {
            _commands.Enqueue(cmd);
            _spawnPreFrame = -1;
        }

        public void CancleAsyncSpawns(Action<GameObject> callback)
        {
            if (_callback2Commands.TryGetValue(callback, out var list))
            {
                _callback2Commands.Remove(callback);

                foreach (var cmd in list.Set)
                {
                    cmd.cancled = true;
                }

                list.Release();
            }
        }

        public void Unspawn(GameObject obj, float delay, bool resetParent = true)
        {
            StartCoroutine(UnspawnRoutine(obj, delay, resetParent));
        }

        public virtual void Unspawn(GameObject obj, bool resetParent = true)
        {
            for (int i = 0; i < poolList.Count; i++)
            {
                if (poolList[i].Unspawn(obj, resetParent)) return;
            }
            Destroy(obj);
        }

        void Update()
        {
            if (_commands.Count == 0) return;

            if (_spawnPreFrame < 0)
            {
                _spawnPreFrame = Math.Max(_commands.Count / _kFrameCountToSpawnAll, _kMinSpawnCountPerFrame);
            }
            int count = Math.Min(_commands.Count, _spawnPreFrame);
            for (int i = 0; i < count;)
            {
                if (_commands.Count == 0)
                    break;

                SpawnCommand command = _commands.Dequeue();
                if (command.cancled)
                {
                    RecycleSpawnCommand(command);
                    continue;
                }

                var prefab = command.prefab;
                if (prefab is null)
                {
                    var assetName = command.assetName;
                    ObjectPoolMgr.resMgr.LoadAssetAsync<GameObject>(command.assetName).AddCompleted((handler) =>
                    {
                        if (handler.Result != null)
                        {
                            if (!_assets.ContainsKey(assetName))
                            {
                                _assets.Add(assetName, handler.Result);
                            }

                            if (!command.cancled)
                            {
                                command.prefab = handler.Result;
                                Spawn(command);
                                RecycleSpawnCommand(command);
                                return;
                            }
                        }

                        RecycleSpawnCommand(command);
                    });
                }
                else
                {
                    Spawn(command);
                    RecycleSpawnCommand(command);
                }

                i++;
            }
        }

         protected IEnumerator UnspawnRoutine(GameObject spawnedObj, float activeDuration, bool resetParent)
        {
            yield return new WaitForSeconds(activeDuration);
            Unspawn(spawnedObj, resetParent);
        }

        private int NewPool(GameObject obj, int count = 1)
        {
            int ID = GetPoolID(obj);

            if (ID != -1) poolList[ID].MatchObjectCount(count);
            else
            {
                Pool pool = new Pool();
                pool.prefab = obj;
                pool.parentT = transform;
                pool.MatchObjectCount(count);
                poolList.Add(pool);
                ID = poolList.Count - 1;
            }

            return ID;
        }

        protected int GetPoolID(GameObject obj)
        {
            for (int i = 0; i < poolList.Count; i++)
            {
                if (poolList[i].prefab == obj) return i;
            }
            return -1;
        }

        private int _cleanCatchPoolIndex;
        public bool CleanOneCatch()
        {
            if (poolList.Count == 0) return false;
            if (_cleanCatchPoolIndex >= poolList.Count) _cleanCatchPoolIndex = 0;
            int index = _cleanCatchPoolIndex;
            bool cleaned = false;
            while (!cleaned)
            {
                cleaned = poolList[index].CleanOneCatch();
                if (cleaned && poolList[index].GetTotalObjectCount() == 0)
                {
                    ReleaseAsset(poolList[index].prefab);
                    poolList.RemoveAt(index);
                }
                index++;
                if (index >= poolList.Count) index = 0;
                if (_cleanCatchPoolIndex == index) break;
            }
            _cleanCatchPoolIndex = index;
            return cleaned;
        }

        public void ClearAllCatch()
        {
            for (int i = 0; i < poolList.Count; i++)
            {
                poolList[i].ClearAllCatch();
                if (poolList[i].GetTotalObjectCount() == 0)
                {
                    ReleaseAsset(poolList[i].prefab);
                    poolList.RemoveAt(i);
                    i--;
                }
            }
        }

        void ReleaseAsset(GameObject obj)
        {
            foreach (var asset in _assets)
            {
                if (asset.Value == obj)
                {
                    ObjectPoolMgr.resMgr.Release<GameObject>(obj);
                    _assets.Remove(asset.Key);
                    break;
                }
            }
        }

        void OnDestroy()
        {
            for (int i = 0; i < poolList.Count; i++) poolList[i].ClearAll();
            poolList.Clear();
            DestoryedEvent?.Invoke(this);

            if (ObjectPoolMgr.resMgr != null)
            {
                foreach (var asset in _assets)
                {
                    ObjectPoolMgr.resMgr.Release<GameObject>(asset.Value);
                }
                _assets.Clear();
            }
            _commands.Clear();
            _callback2Commands.Clear();
        }

        private GameObject GetAsset(string assetName)
        {
            if (assetName == null) return null;

            GameObject asset;
            if (_assets.TryGetValue(assetName, out asset))
            {
                return asset;
            }

            asset = ObjectPoolMgr.resMgr.LoadAssetSync<GameObject>(assetName);
            if (asset != null)
            {
                _assets.Add(assetName, asset);
            }
            return asset;
        }

        SpawnCommand CreateSpawnCommand(string asset, GameObject obj, Transform trans,
                                            bool setP, bool setR, Vector3 p, Quaternion r,
                                            Action<GameObject> handler, float dur)
        {
            var cmd = ReferencePool.Acquire<SpawnCommand>();
            cmd.Initialize(asset, obj, trans, setP, setR, p, r, handler, dur);
            if (handler != null)
            {
                if (!_callback2Commands.TryGetValue(handler, out var pooledList))
                    _callback2Commands[handler] = pooledList = PooledHashSet<SpawnCommand>.Get();
                pooledList.Set.Add(cmd);
            }
            return cmd;
        }

        void RecycleSpawnCommand(SpawnCommand cmd)
        {
            if (cmd.callback != null)
            {
                if (_callback2Commands.TryGetValue(cmd.callback, out var pooledList))
                {
                    pooledList.Set.Remove(cmd);

                    if (pooledList.Set.Count == 0)
                    {
                        pooledList.Release();
                        _callback2Commands.Remove(cmd.callback);
                    }
                }
            }
            cmd.Release();
        }

        class SpawnCommand : IReference
        {
            public string assetName;
            public GameObject prefab;
            public Transform parent;
            public bool setPos;
            public bool setRot;
            public Vector3 pos;
            public Quaternion rot;
            public float activeDuration;
            public Action<GameObject> callback;
            public bool cancled;

            public void Initialize(string asset, GameObject obj, Transform trans,
                                            bool setP, bool setR, Vector3 p, Quaternion r,
                                            Action<GameObject> handler, float dur)
            {
                assetName = asset;
                prefab = obj;
                parent = trans;
                setPos = setP;
                setRot = setR;
                pos = p;
                rot = r;
                callback = handler;
                activeDuration = dur;
            }

            public void Clear()
            {
                prefab = null;
                parent = null;
                callback = null;
                cancled = false;
            }
        }
    }
}