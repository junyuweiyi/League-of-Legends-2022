/********************************************************************
	created:	2020/09/28
	author:		maoqy
	
	purpose:	通用异步操作包装，可用于协程，也可独自监听Completed事件
*********************************************************************/
using System.Collections;
using UnityEngine;
using System;

namespace iFramework
{
    /// <summary>
    /// 通用异步操作包装，可用于协程<br/>
    /// 可将异步回调接口通过这个类包装为可用于协程的接口<br/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncHandle<T> : IEnumerator
    {
        private Action1List<AsyncHandle<T>> _completed;

        /// <summary>
        /// 异步操作完成的事件<br/>
        /// 如果该操作已经完成，则新附加的事件立即调用
        /// </summary>
        public event Action<AsyncHandle<T>> Completed
        {
            add
            {
                if (IsDone)
                {
                    try
                    {
                        value(this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                else
                {
                    if (_completed == null)
                        _completed = new Action1List<AsyncHandle<T>>();
                    _completed.Add(value);
                }
            }
            remove => _completed?.Remove(value);
        }
        /// <summary>
        /// 异步操作是否已完成
        /// </summary>
        /// <value></value>
        public bool IsDone { get; private set; }
        /// <summary>
        /// 异步操作的结果
        /// </summary>
        /// <value></value>
        public T Result { get; private set; }

        System.Threading.EventWaitHandle _waitHandle;

        /// <summary>
        /// 返回一个可供async/await使用的Task<br/>
        /// 每次都是一个新的Task
        /// </summary>
        /// <value></value>
        public System.Threading.Tasks.Task<T> Task
        {
            get
            {
                if (IsDone)
                {
                    return System.Threading.Tasks.Task.FromResult(Result);
                }

                if (_waitHandle == null)
                    _waitHandle = new System.Threading.ManualResetEvent(false);
                _waitHandle.Reset();

                var waitHandle = _waitHandle;
                return System.Threading.Tasks.Task.Factory.StartNew(o =>
                {
                    var asyncHandle = o as AsyncHandle<T>;
                    if (asyncHandle == null)
                        return default(T);
                    waitHandle.WaitOne();
                    return asyncHandle.Result;
                }, this);
            }
        }

        public AsyncHandle()
        {
            Reset();
        }

        /// <summary>
        /// 重置，清理状态
        /// </summary>
        public void Reset()
        {
            IsDone = false;
            Result = default;
            _completed?.Clear();
        }

        /// <summary>
        /// 调用异步操作完成接口<br/>
        /// 外部使用方不要调用
        /// </summary>
        /// <param name="result"></param>
        public void InvokeCompleted(T result)
        {
            IsDone = true;
            Result = result;
            _completed?.Invoke(this);
            _completed?.Clear();

            if (_waitHandle != null)
                _waitHandle.Set();
        }

        object IEnumerator.Current => Result;
        bool IEnumerator.MoveNext() => !IsDone;
        void IEnumerator.Reset() => Reset();
    }

    public class AsyncHandle : IEnumerator
    {
        private Action1List<AsyncHandle> _completed;

        /// <summary>
        /// 异步操作完成的事件<br/>
        /// 如果该操作已经完成，则新附加的事件立即调用
        /// </summary>
        public event Action<AsyncHandle> Completed
        {
            add
            {
                if (IsDone)
                {
                    try
                    {
                        value(this);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                else
                {
                    if (_completed == null)
                        _completed = new Action1List<AsyncHandle>();
                    _completed.Add(value);
                }
            }
            remove => _completed?.Remove(value);
        }
        /// <summary>
        /// 异步操作是否已完成
        /// </summary>
        /// <value></value>
        public bool IsDone { get; private set; }

        System.Threading.EventWaitHandle _waitHandle;

        /// <summary>
        /// 返回一个可供async/await使用的Task<br/>
        /// 每次都是一个新的Task
        /// </summary>
        /// <value></value>
        public System.Threading.Tasks.Task Task
        {
            get
            {
                if (IsDone)
                {
                    return System.Threading.Tasks.Task.CompletedTask;
                }

                if (_waitHandle == null)
                    _waitHandle = new System.Threading.ManualResetEvent(false);
                _waitHandle.Reset();

                var waitHandle = _waitHandle;
                return System.Threading.Tasks.Task.Factory.StartNew(o =>
                {
                    var asyncHandle = o as AsyncHandle;
                    if (asyncHandle == null)
                        return;
                    waitHandle.WaitOne();
                    return;
                }, this);
            }
        }

        public AsyncHandle()
        {
            Reset();
        }

        /// <summary>
        /// 重置，清理状态
        /// </summary>
        public void Reset()
        {
            IsDone = false;
            _completed?.Clear();
        }

        /// <summary>
        /// 调用异步操作完成接口<br/>
        /// 外部使用方不要调用
        /// </summary>
        /// <param name="result"></param>
        public void InvokeCompleted()
        {
            IsDone = true;
            _completed?.Invoke(this);
            _completed?.Clear();

            if (_waitHandle != null)
                _waitHandle.Set();
        }

        object IEnumerator.Current => null;
        bool IEnumerator.MoveNext() => !IsDone;
        void IEnumerator.Reset() => Reset();

        public static AsyncHandle<T> FromResult<T>(T result)
        {
            var handle = new AsyncHandle<T>();
            handle.InvokeCompleted(result);
            return handle;
        }
    }
}