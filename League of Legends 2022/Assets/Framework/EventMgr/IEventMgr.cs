/********************************************************************
	created:	2020/10/12
	author:		maoqy
	
	purpose:	事件管理器
*********************************************************************/
using System;

namespace iFramework
{
    public abstract class EventArgs
    {
    }

    /// <summary>
    /// 事件管理器<br/>
    /// 应该尽量直接在对象间建立关系，而不是使用这个通用事件管理器<br/>
    /// 先注释了带参数的接口避免滥用
    /// </summary>
    public interface IEventMgr
    {
        /// <summary>
        /// 订阅一个事件通知
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理回调</param>
        void SubscribeNotify(int eventId, Action handler);
        /// <summary>
        /// 取消订阅事件通知
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理回调</param>
        void UnsubscribeNotify(int eventId, Action handler);
        /// <summary>
        /// 触发一个事件通知
        /// </summary>
        /// <param name="eventId">事件ID</param>
        void FireNotify(int eventId);

        /// <summary>
        /// 订阅一个事件
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理回调</param>
        void Subscribe(int eventId, Action<EventArgs> handler);
        /// <summary>
        /// 取消订阅一个事件
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理回调</param>
        void Unsubscribe(int eventId, Action<EventArgs> handler);
        /// <summary>
        /// 触发一个事件
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="args">事件参数</param>
        void Fire(int eventId, EventArgs args);
    }
}