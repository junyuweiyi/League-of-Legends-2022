/********************************************************************
	created:	2020/09/24
	author:		ZYL
	
	purpose:	游戏系统管理器单件模板。
                各个游戏系统管理器统一由状态机负责创建和销毁，游戏其他
                地方通过单件模式访问。
                游戏管理器是挂接在场景对象上的组件，可以使用Unity引擎的
                Update、协程等功能
*********************************************************************/

using UnityEngine;
using System;

namespace iFramework
{
    public class GameMgr : MonoBehaviour
    {
        private static GameObject _mgrObj;

        public static GameObject MgrObj
        {
            get
            {
                if (_mgrObj == null)
                {
                    _mgrObj = new GameObject();
                    DontDestroyOnLoad(_mgrObj);
                    _mgrObj.name = "GameManagers";
                }
                return _mgrObj;
            }
        }

        /// <summary>
        /// 初始化 GameMgr，创建后执行
        /// </summary>
        public virtual void Initialize() { }
        /// <summary>
        /// 清理数据，销毁前执行
        /// </summary>
        public virtual void Dispose() { }
    }

    public class GameMgr<T> : GameMgr where T : GameMgr, new()
    {
        protected static T _instance = null;

        static string _initializeMarker = "[GameMgr]" + typeof(T).Name + ".Initialize";
        static string _disposeMarker = "[GameMgr]" + typeof(T).Name + ".Dispose";

        public static event Action<T> OnInstanceCreated;

        public TaggedLogger Logger
        {
            get => _logger ?? (_logger = new TaggedLogger(GetType().Name));
            set => _logger = value;
        }
        private TaggedLogger _logger;

        public static T I
        {
            get
            {
                return _instance;
            }
        }

        public static bool Valid
        {
            get { return _instance != null; }
        }

        /// <summary>
        /// 除游戏状态机外，其他地方请勿调用
        /// </summary>
        public static void CreateMgr(bool initialize = true)
        {
            if (_instance != null)
            {
                Debug.LogError("Game manager (" + typeof(T).Name + ") already created.");
            }
            else
            {
                _instance = MgrObj.AddComponent<T>();

                if (initialize)
                {
                    UnityEngine.Profiling.Profiler.BeginSample(_initializeMarker);
                    try
                    {
                        _instance.Initialize();
                    }
                    finally
                    {
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                }

                OnInstanceCreated?.InvokeSafely(_instance);
            }
        }

        /// <summary>
        /// 除游戏状态机外，其他地方请勿调用
        /// </summary>
        public static void DestroyMgr(bool dispose = true)
        {
            if (_instance != null)
            {
                if (dispose)
                {
                    UnityEngine.Profiling.Profiler.BeginSample(_disposeMarker);
                    try
                    {
                        _instance.Dispose();
                    }
                    finally
                    {
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                }

                Destroy(_instance);
                _instance = null;
            }
        }
    }
}
