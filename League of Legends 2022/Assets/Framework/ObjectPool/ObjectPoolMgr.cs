/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	对象池实现
*********************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace iFramework
{
    internal class ObjectPoolMgr : IObjectPoolMgr, IFrameworkModule
    {
        private List<ObjectPool> _pools;
        internal static IResourceMgr resMgr;
        internal static ObjectPoolCleanStrategy cleanStrategy;

        public ObjectPoolMgr()
        {
            _pools = new List<ObjectPool>();
            Application.lowMemory += OnLowMemory;

        }

        public void Initialize(IResourceMgr mgr)
        {
            resMgr = mgr;
            cleanStrategy = new ObjectPoolCleanStrategy();
        }

        public IObjectPool CreatePool(Transform parent, string name = null)
        {
            GameObject go = new GameObject();
            go.transform.parent = parent;
            ObjectPool pool = go.AddComponent<ObjectPool>();
            go.name = name ?? "ObjectPool";
            pool.DestoryedEvent += OnObjectPoolDestory;

            _pools.Add(pool);
            return pool;
        }

        public IObjectPool CreateOffScreenPool(Transform parent, string name = null)
        {
            GameObject go = new GameObject();
            go.transform.parent = parent;
            ObjectPool pool = go.AddComponent<OffScreenObjectPool>();
            go.name = name ?? "ObjectPool";
            pool.DestoryedEvent += OnObjectPoolDestory;

            _pools.Add(pool);
            return pool;
        }

        public void DestoryPool(IObjectPool pool)
        {
            ObjectPool op = pool as ObjectPool;
            if (_pools.Contains(op))
            {
                _pools.Remove(op);
                op.DestoryedEvent -= OnObjectPoolDestory;
                Object.Destroy(op.gameObject);
            }
        }
        int IFrameworkModule.Priority => 1;

        void IFrameworkModule.Shutdown()
        {
            resMgr = null;
            Application.lowMemory -= OnLowMemory;

            for (int i = 0; i < _pools.Count; i++)
            {
                _pools[i].DestoryedEvent -= OnObjectPoolDestory;
                Object.Destroy(_pools[i]);
            }
            _pools.Clear();
        }

        private int _cleanCatchPoolIndex;
        private float _cleanInterval;
        void IFrameworkModule.Update(float elapseSeconds, float realElapseSeconds)
        {
            _cleanInterval -= realElapseSeconds;
            if (_cleanInterval < 0)
            {
                _cleanInterval = cleanStrategy.catchCleanInterval;
                cleanStrategy.UpdateStrategy();

                if (_pools.Count == 0) return;
                if (_cleanCatchPoolIndex >= _pools.Count) _cleanCatchPoolIndex = 0;
                int index = _cleanCatchPoolIndex;
                bool cleaned = false;
                while (!cleaned)
                {
                    cleaned = _pools[index++].CleanOneCatch();
                    if (index >= _pools.Count) index = 0;
                    if (_cleanCatchPoolIndex == index) break;
                }
                _cleanCatchPoolIndex = index;
            }
        }

        private void OnLowMemory()
        {
            for (int i = 0; i < _pools.Count; i++)
            {
                _pools[i].ClearAllCatch();
            }
            cleanStrategy.OnLowMemory();
        }

        private void OnObjectPoolDestory(ObjectPool pool)
        {
            _pools.Remove(pool);
        }
    }
}