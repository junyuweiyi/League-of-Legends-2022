/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	
*********************************************************************/
using UnityEngine;
using System.Collections.Generic;

namespace iFramework
{
    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;

        public InactiveList inactiveList = new InactiveList();
        public HashSet<GameObject> activeList = new HashSet<GameObject>();
        public Transform parentT;

        private PoolActiveCountRecords _activeCountRecords = new PoolActiveCountRecords();

        public GameObject Spawn(Transform parent)
        {
            return Spawn(parent, false, false);
        }

        public GameObject Spawn(Transform parent, Vector3 pos)
        {
            return Spawn(parent, true, false, pos);
        }

        public GameObject Spawn(Transform parent, Vector3 pos, Quaternion rot)
        {
            return Spawn(parent, true, true, pos, rot);
        }

        GameObject Spawn(Transform parent, bool setPos, bool setRot, Vector3 pos = default, Quaternion rot = default)
        {
            var prefabTrans = prefab.transform;

            GameObject obj = null;
            Transform trans = null;
            while (obj == null && inactiveList.Count > 0)
            {
                obj = inactiveList.RemoveLast();
                if (obj != null)
                {
                    trans = obj.transform;
                    trans.SetParent(parent, false);
                    obj.SetActive(true);
                }
            }
            
            if (obj == null)
            {
                obj = Object.Instantiate(prefab);
                trans = obj.transform;
                if (!(parent is null))
                    trans.SetParent(parent, false);
                if (!obj.activeSelf)
                    obj.SetActive(true);
            }            

            trans.localScale = prefabTrans.localScale;

            if (setPos)
                trans.position = pos;
            else
                trans.localPosition = prefabTrans.localPosition;

            if (setRot)
                trans.rotation = rot;
            else
                trans.localRotation = prefabTrans.localRotation;

            activeList.Add(obj);

            _activeCountRecords.UpdateActiveCount(activeList.Count);

            return obj;
        }

        public bool Unspawn(GameObject obj, bool resetParent)
        {
            if (activeList.Contains(obj))
            {
                obj.SetActive(false);
                if (resetParent) obj.transform.SetParent(parentT, false);
                activeList.Remove(obj);
                inactiveList.Add(obj);
                return true;
            }
            if (inactiveList.Contains(obj))
            {
                return true;
            }
            return false;
        }


        public void MatchObjectCount(int count)
        {
            int currentCount = GetTotalObjectCount();
            for (int i = currentCount; i < count; i++)
            {
                GameObject obj = Object.Instantiate(prefab);
                obj.SetActive(false);
                obj.transform.SetParent(parentT, false);
                inactiveList.Add(obj);
            }
        }

        public int GetTotalObjectCount()
        {
            return inactiveList.Count + activeList.Count;
        }

        public void ClearAllCatch()
        {
            inactiveList.Clear();
        }

        public void ClearAll()
        {
            foreach (var active in activeList)
            {
                if (active != null) Object.Destroy(active);
            }
            inactiveList.Clear();
            activeList.Clear();
        }

        internal bool CleanOneCatch()
        {
            _activeCountRecords.UpdateActiveCount(activeList.Count);
            if (inactiveList.Count > 0)
            {
                int catchCount = (int)(_activeCountRecords.MaxActiveCount * ObjectPoolMgr.cleanStrategy.catchRate);
                if (catchCount < GetTotalObjectCount())
                {
                    GameObject obj = inactiveList.RemoveLast();
                    Object.Destroy(obj);
                    return true;
                }
            }
            return false;
        }

        public class InactiveList
        {
            List<GameObject> _list = new List<GameObject>();
            HashSet<GameObject> _set = new HashSet<GameObject>();

            public int Count => _list.Count;

            public void Add(GameObject obj)
            {
                _list.Add(obj);
                _set.Add(obj);
            }

            public bool Contains(GameObject obj)
            {
                return _set.Contains(obj);
            }

            public GameObject RemoveLast()
            {
                var removed = _list.RemoveLast();
                if (removed != null)
                    _set.Remove(removed);
                return removed;
            }

            public void Clear()
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    if (_list[i] != null) Object.Destroy(_list[i]);
                }
                _list.Clear();
                _set.Clear();
            }
        }
    }
}
