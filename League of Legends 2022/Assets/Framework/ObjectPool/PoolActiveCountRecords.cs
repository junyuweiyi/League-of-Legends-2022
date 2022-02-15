/********************************************************************
	created:	2021/03/04
	author:		ZYL
	
	purpose:	记录对象池活跃对象数量
*********************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace iFramework
{
    internal class PoolActiveCountRecords
    {
        private List<Record> _historyRecords = new List<Record>();
        private Record _currentRecord = new Record();
        private int _maxActiveCount;

        public int MaxActiveCount
        {
            get
            {
                return Mathf.Max(_maxActiveCount, _currentRecord.maxActiveCount);
            }
        }

        public void UpdateActiveCount(int count)
        {
            if (_currentRecord.startTime < Time.time - ObjectPoolMgr.cleanStrategy.recordDeltaTime)
            {
                _historyRecords.Add(_currentRecord);
                if (_maxActiveCount < _currentRecord.maxActiveCount) _maxActiveCount = _currentRecord.maxActiveCount;
                _currentRecord = new Record();
            }
            _currentRecord.UpdateActiveCount(count);
            UpdateRecords();
        }

        void UpdateRecords()
        {
            if (_historyRecords.Count == 0) return;
            if (_historyRecords[0].startTime < Time.time - ObjectPoolMgr.cleanStrategy.maxRecordTime)
            {
                _maxActiveCount = _currentRecord.maxActiveCount;
                int index = _historyRecords.Count;
                for (int i = 1; i < _historyRecords.Count; i++)
                {
                    if (_historyRecords[i].startTime > Time.time - ObjectPoolMgr.cleanStrategy.maxRecordTime)
                    {
                        if (_historyRecords[i].maxActiveCount >= _maxActiveCount)
                        {
                            _maxActiveCount = _historyRecords[i].maxActiveCount;
                            index = i;
                        }
                    }
                }
                _historyRecords.RemoveRange(0, index);
            }
        }

        class Record
        {
            public float startTime;
            public int maxActiveCount;

            public Record()
            {
                startTime = Time.time;
            }

            public void UpdateActiveCount(int count)
            {
                if (count > maxActiveCount) maxActiveCount = count;
            }
        }
    }
}