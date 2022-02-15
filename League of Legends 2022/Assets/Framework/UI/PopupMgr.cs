/********************************************************************
	created:	2020/09/23
	author:		ZYL
	
	purpose:	Popup窗口管理器
*********************************************************************/

using System;
using System.Collections.Generic;

namespace iFramework
{
    internal class PopupMgr
    {
        private UIMgr _uiMgr;
        private List<ViewAgent> _popupViews = new List<ViewAgent>();
        private Queue<(string, IUIData)> _allWait = new Queue<(string, IUIData)>();
        public PopupMgr(UIMgr uiMgr)
        {
            _uiMgr = uiMgr;
        }

        public int Popup(string key, IUIData initData, bool waitInQueue)
        {
            if (waitInQueue)
            {
                if (_popupViews.Count > 0) // 需要排队等待
                {
                    _allWait.Enqueue((key, initData));
                    return 0;
                }
            }
            // 如果已经在显示中，移除后重新添加，以更新渲染顺序
            Close(key);
            _uiMgr.EventMgr.FireNotify(PopupOpeningEventArgs.EventId);

            ViewAgent agent = _uiMgr.GetAgent(key);
            agent.InitData = initData;
            _popupViews.Add(agent);
            agent.OpenType = UIOpenType.Popup;

            agent.Show();
            ResortAll();
            return agent.NewInstanceID();
        }

        public void SecondUpdate()
        {
            foreach (var a in _popupViews)
            {
                if (a.UiView != null && a.UiView is IUISecondUpdate)
                {
                    var su = (IUISecondUpdate)(a.UiView);
                    try
                    {
                        su.OnSecondUpdate();
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"second update ui: {a.UiView.name} with e: {e}");
                    }
                }
            }
        }

        public void Close(string key, int instanceID = 0)
        {
            if (instanceID < 0)
                return;
            //
            ViewAgent agent = _uiMgr.GetAgent(key);
            if (instanceID > 0 && instanceID != agent.InstanceID) return;

            if (_popupViews.Remove(agent)) agent.Close(false, OnAfterClose);
        }

        void OnAfterClose()
        {
            // 处理排队的UI
            if (_allWait.Count > 0)
            {
                var (nextkey, nextUIData) = _allWait.Dequeue();
                Popup(nextkey, nextUIData, false);
            }
        }

        public void Close(ViewAgent agent, bool destroyImmediate = false)
        {
            if (_popupViews.Remove(agent))
                agent.Close(destroyImmediate, OnAfterClose);
        }

        public void HideAll(bool hide)
        {
            // 处理排队的UI
            _allWait.Clear();
            //
            for (int i = 0; i < _popupViews.Count; i++)
            {
                if (hide) _popupViews[i].Hide();
                else _popupViews[i].Show(true);
            }
        }

        public void CloseAll(bool destroyImmediate = false)
        {
            // 处理排队的UI
            _allWait.Clear();
            //

            List<ViewAgent> tmp = new List<ViewAgent>(_popupViews);
            foreach (var agent in tmp)
            {
                Close(agent, destroyImmediate);
            }

            _popupViews.Clear();
        }

        public bool PopupShown()
        {
            return _popupViews.Count > 0;
        }

        private void ResortAll()
        {
            for (int i = 0; i < _popupViews.Count; i++)
            {
                _popupViews[i].SetSortOrder(i + 100);
            }
        }

        internal bool TrySetTopUI()
        {
            if (_popupViews.Count > 0)
            {
                _popupViews[_popupViews.Count - 1].TrySetToTop();
                return true;
            }
            return false;
        }

        internal bool TryCloseTopUI()
        {
            if (_popupViews.Count > 0)
            {
                Close(_popupViews[_popupViews.Count - 1]);
                return true;
            }
            return false;
        }
    }
}
