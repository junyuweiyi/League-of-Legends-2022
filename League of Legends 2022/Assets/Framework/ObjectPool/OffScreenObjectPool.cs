/********************************************************************
	created:	2021/10/21 16:26:40
	author:		LYF
	
	purpose:	该资源池回收对象时会先将对象移动至屏幕外, 超过一定时间后才会调用Unspawn
                   如果短时间，又要创建该对象直接移回屏幕即可
                   避免gameObject.SetActive()和transform.SetParent()的开销
*********************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace iFramework
{
    class OffScreenInfo : IReference
    {
        public GameObject gameObject;
        public float time;

        public void Initialize(GameObject obj, float curTime)
        {
            gameObject = obj;
            time = curTime;
        }

        public void Clear()
        {
            gameObject = null;
            time = 0;
        }
    }

    internal sealed class OffScreenObjectPool : ObjectPool
    {
        const float _kInterval = 3f;
        const int _kMaxCountPerFrame = 10;

        Dictionary<string, Queue<OffScreenInfo>> _offScreenObjects = new Dictionary<string, Queue<OffScreenInfo>>();

        void OffScreenBeforeUnspawn(string assetName, GameObject spawnedObj)
        {
            spawnedObj.transform.position = Vector3.down * 20000;
            var pooledOffInfo = ReferencePool.Acquire<OffScreenInfo>();
            pooledOffInfo.Initialize(spawnedObj, Time.realtimeSinceStartup);
            if (!_offScreenObjects.ContainsKey(assetName))
                _offScreenObjects.Add(assetName, new Queue<OffScreenInfo>());
            _offScreenObjects[assetName].Enqueue(pooledOffInfo);
            //Debug.LogFormat("EffectiveObjectPool offScreen Object:{0} time:{1}", spawnedObj, pooledOffInfo.time);
        }

        protected override GameObject Spawn(GameObject obj, Transform parent, bool setPos, bool setRot, Vector3 pos, Quaternion rot, float activeDuration = -1)
        {
            //Debug.LogFormat("EffectiveObjectPool Spawn Object:{0} time:{1} count:{2}", obj, Time.realtimeSinceStartup,
            //    _offScreenObjects.ContainsKey(obj.name) ? _offScreenObjects[obj.name].Count : 0);
            if (_offScreenObjects.ContainsKey(obj.name) &&
                _offScreenObjects[obj.name].Count > 0)
            {
                var offScreenInfo = _offScreenObjects[obj.name].Dequeue();
                var offScreenObject = offScreenInfo.gameObject;
                var trans = offScreenObject.transform;
                trans.SetParent(parent, false);
                var prefabTrans = poolList[GetPoolID(obj)].prefab.transform;
                if (setPos)
                    trans.position = pos;
                else
                    trans.localPosition = prefabTrans.localPosition;

                if (setRot)
                    trans.rotation = rot;
                else
                    trans.localRotation = prefabTrans.localRotation;

                ReferencePool.Release(offScreenInfo);
                if (activeDuration > 0) StartCoroutine(UnspawnRoutine(offScreenObject, activeDuration, true));
                return offScreenObject;
            }
            else
                return base.Spawn(obj, parent, setPos, setRot, pos, rot, activeDuration);
        }

        public override void Unspawn(GameObject obj, bool resetParent = true)
        {
            for (int i = 0; i < poolList.Count; i++)
            {
                if (poolList[i].activeList.Contains(obj))
                {
                    OffScreenBeforeUnspawn(poolList[i].prefab.name, obj);
                    return;
                }
            }
            Destroy(obj);
        }

        private void Update()
        {
            var nowTime = Time.realtimeSinceStartup;
            foreach (var kvp in _offScreenObjects)
            {
                var queue = kvp.Value;
                var count = _kMaxCountPerFrame;
                while (queue.Count > 0 && --count > 0)
                {
                    var offScreenInfo = queue.Peek();
                    if (nowTime - offScreenInfo.time > _kInterval)
                    {
                        //Debug.LogFormat("EffectiveObjectPool Unspawn Object:{0} time:{1} now:{2} currentCount:{3}",
                        //    offScreenInfo.gameObject, offScreenInfo.time, nowTime, queue.Count);
                        base.Unspawn(offScreenInfo.gameObject);
                        ReferencePool.Release(offScreenInfo);
                        queue.Dequeue();
                    }
                    else
                        break;
                }
            }
        }
    }
}

