/********************************************************************
	created:	2020/09/27
	author:		ZYL
	
	purpose:	
*********************************************************************/

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace iFramework
{
    public class GameState : FSMState
    {
        readonly private List<Type> _mgrTypes = new List<Type>();

        readonly private List<Type> _uiControllerTypes = new List<Type>();
        readonly private List<UIController> _uiControllers = new List<UIController>();

        #region Interfaces
        /// <summary>
        /// 托管GameMgr，被托管的GameMgr会在本状态的OnEnter和OnLeave事件中被创建和销毁
        /// 创建顺序为调用参数的顺序，销毁为反序
        /// </summary>
        /// <typeparam name="T">被托管的GameMgr</typeparam>
        protected void HostGameMgrs<T>() where T : GameMgr
        {
            _mgrTypes.Add(typeof(T));
        }
        protected void HostGameMgrs<T1, T2>() where T1 : GameMgr where T2 : GameMgr
        {
            _mgrTypes.Add(typeof(T1)); _mgrTypes.Add(typeof(T2));
        }
        protected void HostGameMgrs<T1, T2, T3>() where T1 : GameMgr where T2 : GameMgr where T3 : GameMgr
        {
            _mgrTypes.Add(typeof(T1)); _mgrTypes.Add(typeof(T2)); _mgrTypes.Add(typeof(T3));
        }
        protected void HostGameMgrs<T1, T2, T3, T4>() where T1 : GameMgr where T2 : GameMgr where T3 : GameMgr where T4 : GameMgr
        {
            _mgrTypes.Add(typeof(T1)); _mgrTypes.Add(typeof(T2)); _mgrTypes.Add(typeof(T3)); _mgrTypes.Add(typeof(T4));
        }
        protected void HostGameMgrs<T1, T2, T3, T4, T5>() where T1 : GameMgr where T2 : GameMgr where T3 : GameMgr where T4 : GameMgr where T5 : GameMgr
        {
            _mgrTypes.Add(typeof(T1)); _mgrTypes.Add(typeof(T2)); _mgrTypes.Add(typeof(T3)); _mgrTypes.Add(typeof(T4)); _mgrTypes.Add(typeof(T5));
        }

        /// <summary>
        /// 托管UIController，被托管的UIController会在本状态的OnEnter和OnLeave事件中被创建和销毁
        /// 创建顺序为调用参数的顺序，销毁为反序
        /// </summary>
        /// <typeparam name="T">被托管的UIController</typeparam>
        protected void HostUIControllers<T>() where T : UIController
        {
            _uiControllerTypes.Add(typeof(T));
        }
        protected void HostUIControllers<T1, T2>() where T1 : UIController where T2 : UIController
        {
            _uiControllerTypes.Add(typeof(T1)); _uiControllerTypes.Add(typeof(T2));
        }
        protected void HostUIControllers<T1, T2, T3>() where T1 : UIController where T2 : UIController where T3 : UIController
        {
            _uiControllerTypes.Add(typeof(T1)); _uiControllerTypes.Add(typeof(T2)); _uiControllerTypes.Add(typeof(T3));
        }
        protected void HostUIControllers<T1, T2, T3, T4>() where T1 : UIController where T2 : UIController where T3 : UIController where T4 : UIController
        {
            _uiControllerTypes.Add(typeof(T1)); _uiControllerTypes.Add(typeof(T2)); _uiControllerTypes.Add(typeof(T3));
            _uiControllerTypes.Add(typeof(T4));
        }
        protected void HostUIControllers<T1, T2, T3, T4, T5>() where T1 : UIController where T2 : UIController where T3 : UIController where T4 : UIController where T5 : UIController
        {
            _uiControllerTypes.Add(typeof(T1)); _uiControllerTypes.Add(typeof(T2)); _uiControllerTypes.Add(typeof(T3));
            _uiControllerTypes.Add(typeof(T4)); _uiControllerTypes.Add(typeof(T5));
        }
        #endregion

#pragma warning disable UEA0008
        public override void OnEnter(object userData)
        {
            Debug.Log(GetType().Name + "-OnEnter");
            if (_mgrTypes.Count > 0)
            {
                for (int i = 0; i < _mgrTypes.Count; i++)
                {
                    var m = _mgrTypes[i].GetMethod("CreateMgr", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    m.Invoke(null, new object[] { false });
                }
                for (int i = 0; i < _mgrTypes.Count; i++)
                {
                    var instance = GetGameMgrInstance(_mgrTypes[i]);
                    if (instance)
                    {
                        UnityEngine.Profiling.Profiler.BeginSample(Util.String.Format("[GameMgr]{0}.Initialize", instance.GetType().Name));
                        try
                        {
                            // Debug.Log($"[GameMgr]Start Initialize {instance.GetType().Name}");
                            instance.Initialize();
                        }
                        finally
                        {
                            UnityEngine.Profiling.Profiler.EndSample();
                        }
                    }
                }
            }
            if (_uiControllerTypes.Count > 0)
            {
                for (var i = 0; i < _uiControllerTypes.Count; i++)
                {
                    var uiController = (UIController)Activator.CreateInstance(_uiControllerTypes[i]);
                    _uiControllers.Add(uiController);
                }
                for (var i = 0; i < _uiControllers.Count; i++)
                {
                    _uiControllers[i].Initialize();
                }
            }
        }

        public override void OnLeave()
        {
            Debug.Log(GetType().Name + "-OnLeave");
            DestroyUIControllers();
            DestoryGameMgrs();
        }
        public override void OnDestroy()
        {
            Debug.Log(GetType().Name + "-OnDestroy");
            DestroyUIControllers();
            DestoryGameMgrs();
        }
#pragma warning restore UEA0008

        void DestoryGameMgrs()
        {
            if (_mgrTypes.Count > 0)
            {
                for (int i = _mgrTypes.Count; i > 0; i--)
                {
                    var instance = GetGameMgrInstance(_mgrTypes[i - 1]);
                    if (instance)
                    {
                        UnityEngine.Profiling.Profiler.BeginSample(Util.String.Format("[GameMgr]{0}.Dispose", instance.GetType().Name));
                        try
                        {
                            instance.Dispose();
                        }
                        finally
                        {
                            UnityEngine.Profiling.Profiler.EndSample();
                        }
                    }
                }
                for (int i = _mgrTypes.Count; i > 0; i--)
                {
                    var m = _mgrTypes[i - 1].GetMethod("DestroyMgr", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                    m.Invoke(null, new object[] { false });
                }
            }
        }

        GameMgr GetGameMgrInstance(Type mgrType)
        {
            var field = mgrType.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            return field.GetValue(null) as GameMgr;
        }

        void DestroyUIControllers()
        {
            for (var i = _uiControllers.Count - 1; i >= 0; i--)
            {
                _uiControllers[i].Dispose();
            }

            _uiControllers.Clear();
        }
    }
}
