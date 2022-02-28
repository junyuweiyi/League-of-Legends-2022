using System;
using System.Collections.Generic;

namespace iFramework
{
    public class ActionList : DelegateCollection<Action, List<Action>>
    {
        /// <summary>
        /// 执行所有的委托<br/>
        /// </summary>
        public void Invoke()
        {
            InternalInvoke(null);
        }

        protected override List<Action> CreateContainer()
        {
            return new List<Action>(1);
        }

        protected override void InvokeDelegate(Action action, DelegateParam p)
        {
            action();
        }
    }
}