/********************************************************************
	created:	2020/09/14
	author:		ZYL
	
	purpose:	网络事件处理
*********************************************************************/
using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using IMessage = Google.Protobuf.IMessage;

namespace iFramework
{
    internal class MsgHandler
    {
        public object owner;
        public Action<IMessage> msgHandler;
        public Action<uint, byte[]> buffHandler;

        public void Trigger(SCMsg msg)
        {
            if (owner != null)
            {
                if (msgHandler != null)
                {
                    Debug.Assert(msg.msg != null);
                    msgHandler(msg.msg);
                }
                else
                {
                    Debug.Assert(buffHandler != null);
                    Debug.Assert(msg.buffer != null);
                    buffHandler(msg.msgID, msg.buffer);
                }
            }
        }

        public void Reset()
        {
            owner = null;
            msgHandler = null;
            buffHandler = null;
        }
    }

    internal class NetEventDispatcher
    {
        public bool StopDispactch { get; set; }
        /// <summary>
        /// 连接事件对象
        /// </summary>
        private Action<NetworkConnectResult> ConnectEvent;

        /// <summary>
        /// 断开连接事件对象
        /// </summary>
        private Action DisconnectEvent;

        /// <summary>
        /// 网络包回调事件集合
        /// </summary>
        private Dictionary<uint, List<MsgHandler>> _msgEvents = new Dictionary<uint, List<MsgHandler>>();

        /// <summary>
        /// 一次性消息回调<br/>
        /// 同时发送等待的消息不会很多，所以一个列表就够了
        /// </summary>
        /// <returns></returns>
        private List<(uint msgID, Action<IMessage> handler)> _oneTimeMsgEvents = new List<(uint, Action<IMessage>)>();

        public NetEventDispatcher()
        {
            StopDispactch = false;
        }

        public void TriggerMsgEvent(SCMsg msg)
        {
#if ENABLE_MSG_LOG
            if (ProtoBuf.PType.MsgIDExist(msg.msgID))
            {
                var msgName = ProtoBuf.PType.MsgID2Name(msg.msgID);
                if (msgName != "AddMapEntityNtf" && msgName != "DelMapEntityNtf" && msgName != "UpdateMapEntityNtf" && msgName != "UpdateMapEntityPosNtf" && msgName != "DelMarchNtf" && msgName != "AddMarchNtf")
                {
                    Debug.LogFormat("Network: TriggerMsgEvent {0} {1}", msgName, msg.msgID);
                }
            }
#endif

            if (StopDispactch) return;

            var handled = false;

            for (var i = 0; i < _oneTimeMsgEvents.Count; i++)
            {
                var msgEvent = _oneTimeMsgEvents[i];
                if (msgEvent.msgID == msg.msgID)
                {
                    var handler = msgEvent.handler;
                    _oneTimeMsgEvents.RemoveAt(i);

                    handler.InvokeSafely(msg.msg);
                    handled = true;
                    break;
                }
            }

            if (_msgEvents.TryGetValue(msg.msgID, out var handlers))
            {
                if (handlers != null && handlers.Count > 0)
                {
                    for (var i = 0; i != handlers.Count; i++)
                    {
                        handlers[i].Trigger(msg);
                    }
                    handled = true;
                }
            }

            if (!handled)
            {
                Debug.LogFormat("Network msg not be handled: {0}", PType.MsgIDExist(msg.msgID) ? PType.MsgID2Name(msg.msgID) : msg.msgID.ToString());
            }
        }

        /// <summary>
        /// 触发连接事件
        /// </summary>
        /// <param name="result">连接的结果</param>
        public void TriggerConnectEvent(NetworkConnectResult result)
        {
            if (StopDispactch) return;

            ConnectEvent?.Invoke(result);
        }

        /// <summary>
        /// 触发断开连接的事件
        /// </summary>
        public void TriggerDisConnectEvent()
        {
            if (StopDispactch) return;

            DisconnectEvent?.Invoke();
        }

        public void RegisterMsg<T>(object owner, Action<T> handler)
        {
            RegisterMsg(typeof(T), owner, (msg) => handler((T)msg));
        }

        public void UnregisterMsg<T>(object owner)
        {
            UnregisterMsg(typeof(T), owner);
        }

        public void RegisterMsg(Type msgType, object owner, Action<IMessage> handler)
        {
            if (msgType == null || owner == null || handler == null)
            {
                Debug.LogError("RegisterMsg param error");
                return;
            }

            uint msgID = ProtoBuf.PType.MsgName2ID(msgType.Name);

            if (!_msgEvents.TryGetValue(msgID, out var handlerList))
            {
                handlerList = new List<MsgHandler>();
                _msgEvents.Add(msgID, handlerList);
            }

            // 是否已经注册
            if (handlerList.Count > 0 && handlerList.Exists(t => t.owner == owner))
            {
                Debug.LogError("RegisterMsg Failed, Already Registered: " + msgType.Name);
                return;
            }

            // 新建并添加到列表
            MsgHandler msgHandler = new MsgHandler()
            {
                owner = owner,
                msgHandler = handler
            };
            handlerList.Add(msgHandler);
        }

        public void UnregisterMsg(Type msgType, object owner)
        {
            if (msgType == null)
            {
                Debug.LogError("UnregisterMsg param error");
                return;
            }
            uint msgID = ProtoBuf.PType.MsgName2ID(msgType.Name);

            if (!_msgEvents.TryGetValue(msgID, out var handlers))
            {
                // Debug.LogError("UnregisterMsg error: handle not found");
                return;
            }

            for (var i = 0; i < handlers.Count; i++)
            {
                if (handlers[i].owner == owner)
                {
                    // 置空，等待异步删除
                    handlers[i].Reset();
                }
            }
        }

        public void RegisterMsg(object owner, uint msgID, Action<uint, byte[]> handler)
        {
            if (owner == null || handler == null)
            {
                Debug.LogError("RegisterMsg param error");
                return;
            }

            if (!_msgEvents.TryGetValue(msgID, out var handlerList))
            {
                handlerList = new List<MsgHandler>();
                _msgEvents.Add(msgID, handlerList);
            }

            // 是否已经注册
            if (handlerList.Count > 0 && handlerList.Exists(t => t.owner == owner))
            {
                Debug.LogError("RegisterMsg Failed, Already Registered: " + msgID);
                return;
            }

            // 新建并添加到列表
            MsgHandler msgHandler = new MsgHandler()
            {
                owner = owner,
                buffHandler = handler
            };
            handlerList.Add(msgHandler);
        }

        public void UnregisterMsg(object owner, uint msgID)
        {
            if (!_msgEvents.TryGetValue(msgID, out var handlers))
            {
                return;
            }

            for (var i = 0; i < handlers.Count; i++)
            {
                if (handlers[i].owner == owner)
                {
                    // 置空，等待异步删除
                    handlers[i].Reset();
                }
            }
        }

        public void UnregisterMsgs(object owner)
        {
            foreach (var (msgId, handlers) in _msgEvents)
            {
                for (var i = 0; i < handlers.Count; i++)
                {
                    if (handlers[i].owner == owner)
                    {
                        // 置空，等待异步删除
                        handlers[i].Reset();
                    }
                }
            }
        }

        /// <summary>
        /// 注册一次性消息回调 <br/>
        /// <strong>注意</strong>：消息回来之后一次只消耗一个回调，不会同类型的消息全部触发<br/>
        /// </summary>
        /// <param name="handler">消息回调</param>
        /// <typeparam name="T">回包类型</typeparam>
        public void RegisterOneTimeMsg<T>(Action<IMessage> handler)
        {
            RegisterOneTimeMsg(typeof(T), handler);
        }

        /// <summary>
        /// 注册一次性消息回调 <br/>
        /// <strong>注意</strong>：消息回来之后一次只消耗一个回调，不会同类型的消息全部触发<br/>
        /// </summary>
        /// <param name="msgType">回包类型</param>
        /// <param name="handler">消息回调</param>
        public void RegisterOneTimeMsg(Type msgType, Action<IMessage> handler)
        {
            if (msgType == null || handler == null)
            {
                Debug.LogErrorFormat("RegisterOneTimeMsg param error for {0}", msgType);
                return;
            }
            uint msgID = ProtoBuf.PType.MsgName2ID(msgType.Name);
            _oneTimeMsgEvents.Add(msgID, handler);
        }

        /// <summary>
        /// 移除一个一次性消息回调
        /// </summary>
        /// <param name="handler">消息回调</param>
        /// <typeparam name="T">回包类型</typeparam>
        public void UnregisterOneTimeMsg<T>(Action<IMessage> handler)
        {
            UnregisterOneTimeMsg(typeof(T), handler);
        }

        /// <summary>
        /// 移除一个一次性消息回调
        /// </summary>
        /// <param name="msgType">回包类型</param>
        /// <param name="handler">消息回调</param>
        public void UnregisterOneTimeMsg(Type msgType, Action<IMessage> handler)
        {
            uint msgID = ProtoBuf.PType.MsgName2ID(msgType.Name);
            if (!_oneTimeMsgEvents.Remove((msgID, handler)))
            {
                Debug.LogErrorFormat("UnregisterOneTimeMsg error for {0}: handle not found", msgType);
                return;
            }
        }

        public void ClearAllOneTimeMsgs()
        {
            _oneTimeMsgEvents.Clear();
        }

        public void RegisterConnectEvent(Action<NetworkConnectResult> handler)
        {
            ConnectEvent += handler;
        }

        public void UnregisterConnectEvent(Action<NetworkConnectResult> handler)
        {
            ConnectEvent -= handler;
        }

        public void RegisterDisconnectEvent(Action handler)
        {
            DisconnectEvent += handler;
        }

        public void UnregisterDisconnectEvent(Action handler)
        {
            DisconnectEvent -= handler;
        }

        private float _gcInterval;
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            _gcInterval -= realElapseSeconds;
            if (_gcInterval < 0)
            {
                _gcInterval = 10;//每10秒处理一次异步删除
                foreach (var handlers in _msgEvents)
                {
                    for (int i = 0; i < handlers.Value.Count; i++)
                    {
                        if (handlers.Value[i].owner == null)
                        {
                            handlers.Value.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
    }
}