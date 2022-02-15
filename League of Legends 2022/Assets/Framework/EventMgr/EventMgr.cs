/********************************************************************
	created:	2020/10/12
	author:		maoqy
	
	purpose:	事件管理器
*********************************************************************/

using System.Collections.Generic;
using System;

namespace iFramework
{
    internal class EventMgr : IEventMgr, IFrameworkModule
    {
        private Dictionary<int, ActionList> _notifyCollections = new Dictionary<int, ActionList>();
        private Dictionary<int, Action1List<EventArgs>> _eventCollections = new Dictionary<int, Action1List<EventArgs>>();

        public void SubscribeNotify(int eventId, Action handler)
        {
            if (_notifyCollections.TryGetValue(eventId, out var eventHandlerCollection))
            {
                eventHandlerCollection.Add(handler);
            }
            else
            {
                eventHandlerCollection = new ActionList();
                eventHandlerCollection.Add(handler);
                _notifyCollections.Add(eventId, eventHandlerCollection);
            }
        }

        public void UnsubscribeNotify(int eventId, Action handler)
        {
            if (_notifyCollections.TryGetValue(eventId, out var eventHandlerCollection))
            {
                eventHandlerCollection.Remove(handler);
            }
        }

        public void FireNotify(int eventId)
        {
            if (_notifyCollections.TryGetValue(eventId, out var eventHandlerCollection))
            {
                eventHandlerCollection.Invoke();
            }
        }

        public void Subscribe(int eventId, Action<EventArgs> handler)
        {
            if (_eventCollections.TryGetValue(eventId, out var eventHandlerCollection))
            {
                eventHandlerCollection.Add(handler);
            }
            else
            {
                eventHandlerCollection = new Action1List<EventArgs>();
                eventHandlerCollection.Add(handler);
                _eventCollections.Add(eventId, eventHandlerCollection);
            }
        }

        public void Unsubscribe(int eventId, Action<EventArgs> handler)
        {
            if (_eventCollections.TryGetValue(eventId, out var eventHandlerCollection))
            {
                eventHandlerCollection.Remove(handler);
            }
        }

        public void Fire(int eventId, EventArgs args)
        {
            if (_eventCollections.TryGetValue(eventId, out var eventHandlerCollection))
            {
                eventHandlerCollection.Invoke(args);
            }
        }

        int IFrameworkModule.Priority => 1;

        void IFrameworkModule.Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        void IFrameworkModule.Shutdown()
        {
            _notifyCollections.Clear();
            _eventCollections.Clear();
        }
    }
}