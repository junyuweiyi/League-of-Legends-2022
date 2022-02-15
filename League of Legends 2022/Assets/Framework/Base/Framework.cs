/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	框架基础管理类，提供框架功能模块的创建、销毁、更新等。
*********************************************************************/

using System;
using System.Collections.Generic;

namespace iFramework
{
    /// <summary>
    /// 游戏框架入口。
    /// </summary>
    public static class Framework
    {
        private static readonly LinkedList<IFrameworkModule> _modules = new LinkedList<IFrameworkModule>();
        private static readonly Dictionary<IFrameworkModule, string> _moduleNames = new Dictionary<IFrameworkModule, string>();

        /// <summary>
        /// 所有游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public static void Update(float elapseSeconds, float realElapseSeconds)
        {
            var moduleName = "";
            foreach (IFrameworkModule module in _modules)
            {
                if (UnityEngine.Debug.isDebugBuild)
                {
                    if (!_moduleNames.TryGetValue(module, out moduleName))
                    {
                        _moduleNames[module] = moduleName = "[Framework]" + module.GetType().Name + ".Update";
                    }
                }

                UnityEngine.Profiling.Profiler.BeginSample(moduleName);
                try
                {
                    module.Update(elapseSeconds, realElapseSeconds);
                }
                finally
                {
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
        }

        /// <summary>
        /// 关闭并清理所有游戏框架模块。
        /// </summary>
        public static void Shutdown()
        {
            for (LinkedListNode<IFrameworkModule> module = _modules.Last; module != null; module = module.Previous)
            {
                module.Value.Shutdown();
            }

            _modules.Clear();
            _moduleNames.Clear();
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public static T GetModule<T>() where T : class
        {
            Type interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new Exception(Util.String.Format("You must get module by interface, but '{0}' is not.", interfaceType.FullName));
            }

            if (!interfaceType.FullName.StartsWith("iFramework.", StringComparison.Ordinal))
            {
                throw new Exception(Util.String.Format("You must get a Game Framework module, but '{0}' is not.", interfaceType.FullName));
            }

            string moduleName = Util.String.Format("{0}.{1}", interfaceType.Namespace, interfaceType.Name.Substring(1));
            Type moduleType = Type.GetType(moduleName);
            if (moduleType == null)
            {
                throw new Exception(Util.String.Format("Can not find Game Framework module type '{0}'.", moduleName));
            }

            return GetModule(moduleType) as T;
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要获取的游戏框架模块类型。</param>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        private static IFrameworkModule GetModule(Type moduleType)
        {
            foreach (IFrameworkModule module in _modules)
            {
                if (module.GetType() == moduleType)
                {
                    return module;
                }
            }

            return CreateModule(moduleType);
        }

        /// <summary>
        /// 创建游戏框架模块。
        /// </summary>
        /// <param name="moduleType">要创建的游戏框架模块类型。</param>
        /// <returns>要创建的游戏框架模块。</returns>
        private static IFrameworkModule CreateModule(Type moduleType)
        {
            IFrameworkModule module = (IFrameworkModule)Activator.CreateInstance(moduleType);
            if (module == null)
            {
                throw new Exception(Util.String.Format("Can not create module '{0}'.", moduleType.FullName));
            }

            AddModule(module);

            return module;
        }

        private static void AddModule(IFrameworkModule module)
        {
            LinkedListNode<IFrameworkModule> current = _modules.First;
            while (current != null)
            {
                if (module.Priority > current.Value.Priority)
                {
                    break;
                }

                current = current.Next;
            }

            if (current != null)
            {
                _modules.AddBefore(current, module);
            }
            else
            {
                _modules.AddLast(module);
            }
        }
    }
}
