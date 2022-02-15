/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	游戏状态基类，游戏中的状态继承于FSMState类
*********************************************************************/

using System;
using System.Collections.Generic;

namespace iFramework
{
    public class FSMState
    {
        public virtual void OnInit() { }
        public virtual void OnEnter(object userData) { }
        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds) { }
        public virtual void OnLeave() { }
        public virtual void OnDestroy() { }

        /// <summary>
        /// 获取初始子状态
        /// </summary>
        /// <param name="startType"></param>
        /// <param name="userData"></param>
        /// <returns>初始子状态类型和初始参数</returns>
        protected virtual (Type startType, object userData) GetStartSubState(object userData)
        {
            return default;
        }

        private FSM _myFsm;
        private FSM _subFsm;
        private Dictionary<Type, Func<FSMMsg, bool>> _handlers = new Dictionary<Type, Func<FSMMsg, bool>>();
        private Dictionary<Type, Type> _tansitions = new Dictionary<Type, Type>();

        #region Interface
        /// <summary>
        /// 添加状态自动跳转，根据收到的状态事件自动跳转到设置的状态
        /// 如果同一个消息已经手动添加了响应函数，会优先调用该响应函数，自动跳转被忽略
        /// </summary>
        /// <typeparam name="MsgT"></typeparam>
        /// <typeparam name="StateT"></typeparam>
        public void AddTransition<MsgT, StateT>() where MsgT : FSMMsg where StateT : FSMState
        {
            AddTransition(typeof(MsgT), typeof(StateT));
        }

        public void AddTransition(Type msgType, Type stateType)
        {
            if (!typeof(FSMMsg).IsAssignableFrom(msgType) || !typeof(FSMState).IsAssignableFrom(stateType))
            {
                UnityEngine.Debug.LogErrorFormat("Message Type or State Type is not correct: {0} {1}", msgType, stateType);
                return;
            }
            if (_tansitions.ContainsKey(msgType))
            {
                UnityEngine.Debug.LogError("Message transition already exist: " + msgType.Name);
                return;
            }

            _tansitions.Add(msgType, stateType);
        }

        public void RemoveTransition<MsgT>() where MsgT : FSMMsg
        {
            RemoveTransition(typeof(MsgT));
        }

        public void RemoveTransition(Type msgType)
        {
            if (!typeof(FSMMsg).IsAssignableFrom(msgType))
            {
                UnityEngine.Debug.LogErrorFormat("Message Type is not correct: {0}", msgType);
                return;
            }
            if (!_handlers.ContainsKey(msgType))
            {
                UnityEngine.Debug.LogError("Can not remove handler, not exist : " + msgType.Name);
                return;
            }

            _tansitions.Remove(msgType);
        }
        #endregion

        #region Internal
        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="T">目标状态类型（只能切换到状态树中同一父状态的状态）</typeparam>
        /// <param name="userData">传给目标状态的自定义数据，在目标状态的OnEnter方法中传入</param>
        protected void ChangeState<T>(object userData = null) where T : FSMState
        {
            if (_myFsm == null)
            {
                throw new Exception("FSM is invalid.");
            }

            _myFsm.ChangeState<T>(userData);
        }

        protected void ChangeState(Type type, object userData = null)
        {
            if (_myFsm == null)
            {
                throw new Exception("FSM is invalid.");
            }

            _myFsm.ChangeState(type, userData);
        }

        /// <summary>
        /// 初始化当前状态的子状态
        /// </summary>
        /// <typeparam name="StartState">子状态中默认启动的状态类型</typeparam>
        /// <param name="states">子状态对象列表</param>
        protected void InitSubState<StartState>(params FSMState[] states) where StartState : FSMState
        {
            _subFsm = new FSM(string.Empty);
            _subFsm.Initialize<StartState>(states);
        }

        /// <summary>
        /// 初始化当前状态的子状态
        /// </summary>
        /// <typeparam name="StartState">子状态中默认启动的状态类型</typeparam>
        /// <param name="states">子状态对象列表</param>
        protected void InitSubState(params FSMState[] states)
        {
            _subFsm = new FSM(string.Empty);
            _subFsm.Initialize(states);
        }

        /// <summary>
        /// 初始化当前状态的子状态
        /// </summary>
        /// <typeparam name="StartState">子状态中默认启动的状态类型</typeparam>
        /// <param name="states">子状态对象列表</param>
        protected void InitSubState(IEnumerable<FSMState> states)
        {
            _subFsm = new FSM(string.Empty);
            _subFsm.Initialize(states);
        }
        /// <summary>
        /// 添加状态时间响应函数
        /// </summary>
        /// <typeparam name="T">所需响应的消息类型</typeparam>
        /// <param name="handler">响应函数，该函数返回bool值，代表是否处理了该消息。
        /// 返回false，该消息会继续沿状态树向下传递。</param>
        protected void AddMsgHandler<T>(Func<T, bool> handler) where T : FSMMsg
        {
            Type msgType = typeof(T);
            if (_handlers.ContainsKey(msgType))
            {
                UnityEngine.Debug.LogError("Message handler already exist: " + msgType.Name);
                return;
            }

            if (handler == null)
            {
                UnityEngine.Debug.LogError("Message handler can not be null.");
            }

            _handlers.Add(msgType, (msg) => { return handler((T)msg); });
        }

        protected void RemoveMsgHandler<T>() where T : FSMMsg
        {
            Type msgType = typeof(T);
            if (!_handlers.ContainsKey(msgType))
            {
                UnityEngine.Debug.LogError("Can not remove handler, not exist : " + msgType.Name);
                return;
            }

            _handlers.Remove(msgType);
        }

        protected void ClearMsgHandlers()
        {
            _handlers.Clear();
        }

        protected void ClearMsgTransitions()
        {
            _tansitions.Clear();
        }

        protected bool SendMessage(FSMMsg msg)
        {
            msg.TrySetSubFirst(false);
            return _myFsm.SendMessage(msg);
        }
        protected void PostMessage(FSMMsg msg)
        {
            msg.TrySetSubFirst(false);
            _myFsm.PostMessage(msg);
        }


        internal bool HandleMsg(FSMMsg msg)
        {
            if (msg.SubFirst)
            {
                if (_subFsm != null)
                {
                    if (_subFsm.HandleMsg(msg)) return true;
                }
            }

            Func<FSMMsg, bool> handler;
            if (_handlers.TryGetValue(msg.GetType(), out handler))
            {
                return handler(msg);
            }

            Type nextState;
            if (_tansitions.TryGetValue(msg.GetType(), out nextState))
            {
                ChangeState(nextState, msg);
                return true;
            }

            if (!msg.SubFirst)
            {
                if (_subFsm != null)
                {
                    if (_subFsm.HandleMsg(msg)) return true;
                }
            }

            return false;
        }

        internal void Init(FSM fsm)
        {
            _myFsm = fsm;
            OnInit();
        }
        internal void Enter(object userData)
        {
            OnEnter(userData);

            if (_subFsm != null)
            {
                var start = GetStartSubState(userData);
                if (start.startType != null)
                {
                    _subFsm.Start(start.startType, start.userData);
                }
                else
                {
                    _subFsm.Start();
                }
            }
        }
        internal void Update(float elapseSeconds, float realElapseSeconds)
        {
            OnUpdate(elapseSeconds, realElapseSeconds);
            _subFsm?.Update(elapseSeconds, realElapseSeconds);
        }
        internal void Leave()
        {
            _subFsm?.Stop();
            OnLeave();
        }
        internal void Destroy()
        {
            _subFsm?.Destory();
            ClearMsgHandlers();
            ClearMsgTransitions();
            OnDestroy();
        }
        #endregion
    }
}