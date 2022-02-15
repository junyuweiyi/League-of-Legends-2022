/********************************************************************
	created:	2020/09/24
	author:		ZYL
	
	purpose:	
*********************************************************************/

using System;

namespace iFramework
{
    public class Timer
    {
        public int id;
        public float interval;
        public float duration;
        public Action timeoutCallback;
        public Action<int> intervalCallback1;
        public Action intervalCallback2;
        public Action updater;

        private float _interval = 0;
        private float _duration = 0;
        private int _intervalCount = 0;

        public bool Tick(float elapseSeconds)
        {
            if (updater != null)
            {
                updater();
                return false;
            }

            if (timeoutCallback != null) return TickTimeout(elapseSeconds);

            return TickInterval(elapseSeconds);
        }

        private bool TickTimeout(float elapseSeconds)
        {
            _interval += elapseSeconds;
            if (_interval >= interval)
            {
                timeoutCallback();
                return true;
            }
            return false;
        }

        private bool TickInterval(float elapseSeconds)
        {
            _interval += elapseSeconds;
            _duration += elapseSeconds;
            if (_interval >= interval)
            {
                _interval = 0;
                if (intervalCallback1 != null)
                {
                    intervalCallback1(_intervalCount++);
                }
                else
                {
                    intervalCallback2();
                }
                if (duration < 0) return false;
                if (_duration >= duration) return true;
            }

            return false;
        }
    }
}
