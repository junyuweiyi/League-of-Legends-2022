/********************************************************************
	created:	2020/12/23
	author:		maoqy
	
	purpose:	
*********************************************************************/

using System;
using System.Collections.Generic;

namespace iFramework
{
    public sealed class Action3List<T1, T2, T3> : DelegateCollection<Action<T1, T2, T3>, List<Action<T1, T2, T3>>>
    {
        public void Invoke(T1 res, T2 res2, T3 res3)
        {
            var param = ReferencePool.Acquire<ActionParam>();
            param.param = res;
            param.param2 = res2;
            param.param3 = res3;

            InternalInvoke(param);

            ReferencePool.Release(param);
        }

        protected override List<Action<T1, T2, T3>> CreateContainer()
        {
            return new List<Action<T1, T2, T3>>(1);
        }

        protected override void InvokeDelegate(Action<T1, T2, T3> action, DelegateParam p)
        {
            var param = p as ActionParam;
            action(param.param, param.param2, param.param3);
        }

        class ActionParam : DelegateParam, IReference
        {
            public T1 param;
            public T2 param2;
            public T3 param3;

            public void Clear()
            {
                param = default;
                param2 = default;
                param3 = default;
            }
        }
    }
}