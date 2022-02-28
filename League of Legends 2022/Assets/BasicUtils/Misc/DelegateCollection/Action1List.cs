using System;
using System.Collections.Generic;

namespace iFramework
{
    public class Action1List<T> : DelegateCollection<Action<T>, List<Action<T>>>
    {
        /// <summary>
        /// 执行所有的委托<br/>
        /// </summary>
        /// <param name="res">参数</param>
        public void Invoke(T res)
        {
            var param = ReferencePool.Acquire<ActionParam>();
            param.param = res;

            InternalInvoke(param);

            ReferencePool.Release(param);
        }

        protected override List<Action<T>> CreateContainer()
        {
            return new List<Action<T>>(1);
        }

        protected override void InvokeDelegate(Action<T> action, DelegateParam p)
        {
            var param = p as ActionParam;
            action(param.param);
        }

        class ActionParam : DelegateParam, IReference
        {
            public T param;

            public void Clear()
            {
                param = default;
            }
        }
    }
}