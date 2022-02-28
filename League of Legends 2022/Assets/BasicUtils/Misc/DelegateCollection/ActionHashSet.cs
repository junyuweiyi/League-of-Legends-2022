using System;
using System.Collections.Generic;

namespace iFramework
{
    public class ActionHashSet : DelegateCollection<Action, HashSet<Action>>
    {
        public void Invoke()
        {
            InternalInvoke(null);
        }

        protected override HashSet<Action> CreateContainer()
        {
            return new HashSet<Action>();
        }

        protected override void InvokeDelegate(Action action, DelegateParam p)
        {
            action();
        }
    }
}