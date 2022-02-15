/********************************************************************
	created:	2020/09/22
	author:		ZYL
	
	purpose:	全屏窗口管理，维护窗口跳转栈
*********************************************************************/
using System;
using System.Collections.Generic;

namespace iFramework
{
    internal class WndMgr
    {
        private UIMgr _uiMgr;
        private List<ViewAgent> _wndViews = new List<ViewAgent>();
        internal WndMgr(UIMgr uiMgr)
        {
            _uiMgr = uiMgr;
        }

        public bool WndShown()
        {
            return _wndViews.Count > 0;
        }

        public void NavigateBack()
        {
            ViewAgent agent = PopTopWnd();
            if (agent != null)
            {
                agent.Close();
            }
            agent = GetTopWnd();
            if (agent != null)
            {
                agent.Show(true);
            }
        }

        public void NavigateTo(string key, IUIData initData)
        {
            ViewAgent agent = _uiMgr.GetAgent(key);
            ViewAgent top = GetTopWnd();

            // 已经打开，不处理
            if (top == agent)
            {
                UnityEngine.Debug.LogError("UI already opened! - " + key);
            }


            if (top != null)
            {
                top.Hide();
            }

            agent.InitData = initData;
            agent.OpenType = UIOpenType.Navigate;
            _wndViews.Add(agent);

            _uiMgr.EventMgr.FireNotify(WndOpeningEventArgs.EventId);

            agent.Show();
        }

        public void RedirectTo(string key, IUIData initData)
        {
            CloseAll();
            NavigateTo(key, initData);
        }

        private ViewAgent GetTopWnd()
        {
            if (_wndViews.Count > 0)
            {
                return _wndViews[_wndViews.Count - 1];
            }
            return null;
        }

        private ViewAgent PopTopWnd()
        {
            ViewAgent agent = null;
            if (_wndViews.Count > 0)
            {
                agent = _wndViews[_wndViews.Count - 1];
                _wndViews.RemoveAt(_wndViews.Count - 1);
            }
            return agent;
        }

        public void CloseAll(bool destroyImmediate = false)
        {
            int count = _wndViews.Count;
            for (int i = 0; i < count; i++)
                NavigateBack();
        }

        public void SecondUpdate()
        {
            foreach (var a in _wndViews)
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

        internal bool TrySetTopUI()
        {
            if (_wndViews.Count > 0)
            {
                var agent = _wndViews[_wndViews.Count - 1];
                if (agent.Shown)
                {
                    agent.TrySetToTop();
                    return true;
                }
            }
            return false;
        }

        internal bool TryCloseTopUI()
        {
            if (_wndViews.Count > 0)
            {
                _uiMgr.NavigateBack();
                return true;
            }
            return false;
        }
    }
}
