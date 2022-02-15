/********************************************************************
	created:	2020/09/24
	author:		ZYL
	
	purpose:	
*********************************************************************/

using System;
using System.Collections.Generic;

namespace iFramework
{
    internal class TimerMgr : ITimerMgr, IFrameworkModule
    {
        private Dictionary<int, Timer> _timers = new Dictionary<int, Timer>();
        private List<Timer> _toAdd = new List<Timer>();
        private List<int> _toRemove = new List<int>();
        private int _timerID = 1;

        int IFrameworkModule.Priority => 1;

        public int Interval(float interval, Action<int> callback, float duration = -1)
        {
            Timer timer = new Timer()
            {
                interval = interval,
                duration = duration,
                intervalCallback1 = callback
            };

            AddTimer(timer);

            return timer.id;
        }

        public int Interval(float interval, Action callback, float duration = -1)
        {
            Timer timer = new Timer()
            {
                interval = interval,
                duration = duration,
                intervalCallback2 = callback
            };

            AddTimer(timer);

            return timer.id;
        }

        void AddTimer(Timer timer)
        {
            lock (_toAdd)
            {
                timer.id = _timerID++;
                _toAdd.Add(timer);
            }
        }

        public void Stop(int id)
        {
            // 加锁，让 GC 线程可以调用
            lock (_toRemove)
            {
                _toRemove.Add(id);
            }
        }

        public void ClearAll()
        {
            _timers.Clear();

            lock (_toAdd)
            {
                _toAdd.Clear();
            }

            lock (_toRemove)
            {
                _toRemove.Clear();
            }
        }

        public int Timeout(float seconds, Action callback)
        {
            Timer timer = new Timer()
            {
                interval = seconds,
                timeoutCallback = callback
            };

            AddTimer(timer);

            return timer.id;
        }

        public int Updater(Action update)
        {
            Timer timer = new Timer()
            {
                updater = update
            };

            AddTimer(timer);

            return timer.id;
        }

        void IFrameworkModule.Shutdown()
        {
            ClearAll();
        }

        void IFrameworkModule.Update(float elapseSeconds, float realElapseSeconds)
        {
            DoAdd();
            DoRemove();

            foreach (var timer in _timers)
            {
                if (timer.Value.Tick(realElapseSeconds))
                {
                    Stop(timer.Value.id);
                }
            }
        }

        private void DoAdd()
        {
            lock (_toAdd)
            {
                if (_toAdd.Count == 0) return;

                for (int i = 0; i < _toAdd.Count; i++)
                {
                    _timers.Add(_toAdd[i].id, _toAdd[i]);
                }
                _toAdd.Clear();
            }
        }

        private void DoRemove()
        {
            lock (_toRemove)
            {
                if (_toRemove.Count == 0) return;

                for (int i = 0; i < _toRemove.Count; i++)
                {
                    _timers.Remove(_toRemove[i]);
                }
                _toRemove.Clear();
            }
        }
    }
}
