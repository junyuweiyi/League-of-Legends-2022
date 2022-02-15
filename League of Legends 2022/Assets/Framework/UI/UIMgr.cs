/********************************************************************
	created:	2020/09/22
	author:		ZYL
	
	purpose:	UI管理器实现类
*********************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.UI;

namespace iFramework
{
    internal class UIMgr : IUIMgr, IFrameworkModule
    {
        public IResourceMgr ResMgr { get; private set; }
        public IEventMgr EventMgr { get; private set; }
        public string UIPath { get; private set; }
        public UIRoot UIRoot { get; set; }

        private Dictionary<string, ViewAgent> _agents = new Dictionary<string, ViewAgent>();
        private ViewAgent _hudAgent;
        private WndMgr _wndMgr;
        private PopupMgr _popupMgr;
        private GlobalUIMgr _globalUIMgr;

        int IFrameworkModule.Priority => 1;
        float _lastSecondUpdateTime = 0;

        public void Initialize(IResourceMgr resMgr, string uiPath, IObjectPool pool, ILocalizationMgr localization, IEventMgr eventMgr, ITimerMgr timeMgr)
        {
            ResMgr = resMgr;
            EventMgr = eventMgr;
            UIPath = uiPath;
            _wndMgr = new WndMgr(this);
            _popupMgr = new PopupMgr(this);
            _globalUIMgr = new GlobalUIMgr(this);

            GameObject root = GameObject.Instantiate(Resources.Load<GameObject>("UIRoot"));
            UnityEngine.Object.DontDestroyOnLoad(root);
            UIRoot = root.GetComponent<UIRoot>();

            MessageBox.Pool = pool;
            MessageBox.Localization = localization;
            MessageBox.UIMgr = this;

            GameObject eventSystem = new GameObject("EventSystem");
            UnityEngine.Object.DontDestroyOnLoad(eventSystem);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        public void SecondUpdate()
        {
            _wndMgr.SecondUpdate();
            _popupMgr.SecondUpdate();
        }

        public void CloseAll()
        {
            CloseHUD(true);
            _wndMgr.CloseAll(true);
            _popupMgr.CloseAll(true);
            _globalUIMgr.CloseAll(true);
            _agents.Clear();
        }

        public void HideAll(bool hide)
        {
            HideHUD(hide);
            _popupMgr.HideAll(hide);
        }

        public void ShowHUD(string key, IUIData initData = null)
        {
            CloseHUD();
            if (initData == null) initData = new UIData() { HideBGMask = true };
            _hudAgent = GetAgent(key);
            _hudAgent.InitData = initData;
            _hudAgent.OpenType = UIOpenType.HUD;
            _hudAgent.Show();
        }

        private void HideHUD(bool hide)
        {
            if (_hudAgent != null && _hudAgent.Shown == hide)
            {
                if (hide) _hudAgent.Hide();
                else _hudAgent.Show(true);
            }
        }

        public void CloseHUD(bool destroyImmediate = false)
        {
            _hudAgent?.Close(destroyImmediate);
            _hudAgent = null;
        }

        public bool ShowGlobalUI(string key, IUIData initData = null)
        {
            return _globalUIMgr.Show(key, initData);
        }

        public void CloseGlobalUI(string key)
        {
            _globalUIMgr.Close(key);
        }

        public void NavigateBack()
        {
            _wndMgr.NavigateBack();
            if (!_wndMgr.WndShown())
            {
                HideHUD(false);
            }
        }

        public void NavigateTo(string key, IUIData initData = null)
        {
            _wndMgr.NavigateTo(key, initData);
            HideHUD(true);
        }

        public void RedirectTo(string key, IUIData initData = null)
        {
            _wndMgr.RedirectTo(key, initData);
            HideHUD(true);
        }

        public bool HasWindowShown()
        {
            return _wndMgr.WndShown();
        }

        public void CloseAllWindows(bool showHUD)
        {
            _wndMgr.CloseAll();
            if (showHUD)
            {
                HideHUD(false);
            }
        }

        public int Popup(string key, IUIData initData = null, bool waitInQueue = false)
        {
            return _popupMgr.Popup(key, initData, waitInQueue);
        }

        public void ClosePopup(string key, int instanceID)
        {
            _popupMgr.Close(key, instanceID);
        }

        public int TogglePopup(string key, int instanceID, IUIData initData = null)
        {
            if (IsShown(key, instanceID))
            {
                ClosePopup(key, instanceID);
                return -1;
            }
            else
            {
                return Popup(key, initData);
            }
        }

        public void CloseAllPopups()
        {
            _popupMgr.CloseAll();
        }

        public bool HasPopupShown()
        {
            return _popupMgr.PopupShown();
        }

        void IFrameworkModule.Update(float elapseSeconds, float realElapseSeconds)
        {
            if (UnityEngine.Time.realtimeSinceStartup - _lastSecondUpdateTime >= 1)
            {
                _lastSecondUpdateTime = UnityEngine.Time.realtimeSinceStartup;
                SecondUpdate();
            }
        }

        void IFrameworkModule.Shutdown()
        {
            CloseAll();
        }

        public ViewAgent GetAgent(string key)
        {
            ViewAgent agent;
            if (!_agents.TryGetValue(key, out agent))
            {
                agent = new ViewAgent(key, this);
                _agents.Add(key, agent);
            }
            return agent;
        }

        public MessageBox MsgBox(MsgBoxContent content, Action<MsgBoxEventID> callback)
        {
            return MessageBox.Show(content, callback);
        }

        public bool IsShown(string key, int instanceID = 0)
        {
            ViewAgent agent;
            if (_agents.TryGetValue(key, out agent))
            {
                if (instanceID == 0) return agent.Shown;
                if (instanceID == agent.InstanceID) return agent.Shown;
            }
            return false;
        }

        public void Close(ViewAgent agent)
        {
            switch (agent.OpenType)
            {
                case UIOpenType.HUD:
                    Debug.Assert(_hudAgent == agent);
                    CloseHUD();
                    break;
                case UIOpenType.Navigate:
                    NavigateBack();
                    break;
                case UIOpenType.Popup:
                    _popupMgr.Close(agent);
                    break;
                case UIOpenType.GlobalUI:
                    _globalUIMgr.Close(agent);
                    break;
            }
        }

        public void SetCanvasScale(float scale)
        {
            var refResolution = new Vector2(1920, 1080);
            refResolution /= scale;
            UIRoot.normalUICanvas.GetComponent<CanvasScaler>().referenceResolution = refResolution;
            UIRoot.topUICanvas.GetComponent<CanvasScaler>().referenceResolution = refResolution;
        }

        public Camera GetUICamera()
        {
            return UIRoot.uiCamera;
        }

        public void SetGussianBlurSwitch(IGussianBlurSwitch blurSwitch)
        {
            UIRoot.SetGussianBlurSwitch(blurSwitch);
        }

        public void TurnBlur(bool v)
        {
            UIRoot.TurnBlur(v);
        }

        public bool TrySetTopUI()
        {
            return MessageBox.TrySetTopUI() || _popupMgr.TrySetTopUI() || _wndMgr.TrySetTopUI();
        }

        public bool TryCloseTopUI()
        {
            return MessageBox.TryCloseTopUI() || _popupMgr.TryCloseTopUI() || _wndMgr.TryCloseTopUI();
        }
    }
}
