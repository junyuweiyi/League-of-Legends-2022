/********************************************************************
	created:	2020/09/27
	author:		maoqy
	
	purpose:	使用HashSet作为容器的委托列表
*********************************************************************/

using System;
using System.Collections.Generic;

namespace iFramework
{
    /// <summary>
    /// 委托列表<br/>
    /// 优化添加和移除事件时的性能<br/>
    /// 捕获委托执行异常，防止影响后续委托执行
    /// </summary>
    /// <typeparam name="T">委托类型</typeparam>
    /// <typeparam name="T2">委托的容器类型</typeparam>
    public abstract class DelegateCollection<T, T2> : DelegateCollection where T : Delegate where T2 : ICollection<T>
    {
        T2 _callbacks;
        bool _invoking = false;
        List<(DeferActionType actionType, T action)> _deferAction;

        public sealed override int Count { get { return _callbacks == null ? 0 : _callbacks.Count; } }

        public void Add(T action)
        {
            if (_callbacks == null)
                _callbacks = CreateContainer();

            if (_invoking)
            {
                if (_deferAction == null)
                    _deferAction = new List<(DeferActionType, T)>();
                _deferAction.Add(DeferActionType.Add, action);
            }
            else
            {
                _callbacks.Add(action);
            }
        }

        public void Remove(T action)
        {
            if (_callbacks == null)
                return;

            if (_invoking)
            {
                if (_callbacks.Contains(action))
                {
                    if (_deferAction == null)
                        _deferAction = new List<(DeferActionType, T)>();
                    _deferAction.Add(DeferActionType.Remove, action);
                }
            }
            else
            {
                _callbacks.Remove(action);
            }
        }

        protected void InternalInvoke(DelegateParam param)
        {
            if (_callbacks == null)
                return;

            var invoking = _invoking;
            _invoking = true;
            foreach (var action in _callbacks)
            {
                try
                {
                    InvokeDelegate(action, param);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
            _invoking = invoking;

            // 保证执行事件列表途中可以再次执行事件列表
            if (!_invoking)
                DoDeferredAction();
        }

        protected abstract void InvokeDelegate(T action, DelegateParam p);

        private void DoDeferredAction()
        {
            if (_deferAction != null)
            {
                foreach (var (actionType, action) in _deferAction)
                {
                    if (actionType == DeferActionType.Add) _callbacks.Add(action);
                    else if (actionType == DeferActionType.Remove) _callbacks.Remove(action);
                    else if (actionType == DeferActionType.Clear) _callbacks.Clear();
                }
                _deferAction.Clear();
            }
        }

        public sealed override void Clear()
        {
            if (_callbacks == null)
                return;

            if (_invoking)
            {
                if (_deferAction == null)
                    _deferAction = new List<(DeferActionType, T)>();
                _deferAction.Add(DeferActionType.Clear, null);
            }
            else
            {
                _callbacks.Clear();
                _deferAction?.Clear();
            }
        }

        abstract protected T2 CreateContainer();
    }

    enum DeferActionType
    {
        Add,
        Remove,
        Clear,
    }

    public abstract class DelegateCollection
    {
        public abstract int Count { get; }
        public abstract void Clear();
    }

    public class DelegateParam
    {
    }
}