/********************************************************************
	created:	2020/09/22
	author:		ZYL

	purpose:
*********************************************************************/
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.EventSystems;

namespace iFramework
{
    internal enum UIOpenType
    {
        Unknown,
        HUD,
        Navigate,
        Popup,
        GlobalUI,
    }
    class OnShowEventData : BaseEventData
    {
        public bool awaken;
        public IUIData initData;
        public OnShowEventData(EventSystem _) : base(_) { }
    }
    internal class ViewAgent
    {
        private AutoAssetReference _asset;
        private int _sortOrder;
        private UIMgr _uiMgr;
        private static int _lastInstanceID;
        private UIAnimation _uiAnimation;
        OnShowEventData _onShowEventData = new OnShowEventData(EventSystem.current);

        public UIView UiView { get; private set; }
        public IUIData InitData { get; set; }
        public bool Shown { get; private set; }
        public int InstanceID { get; private set; }
        public UIOpenType OpenType { get; set; }

        public ViewAgent(string assetName, UIMgr uiMgr)
        {
            _uiMgr = uiMgr;
            assetName = _uiMgr.UIPath + assetName + ".prefab";
            _asset = new AutoAssetReference(assetName, _uiMgr.ResMgr);
        }

        public void SetSortOrder(int order)
        {
            _sortOrder = order;
            if (UiView != null)
            {
                UiView.SetSortOrder(order + UiView.OriginalSortOrder);
            }
        }

        public int NewInstanceID()
        {
            InstanceID = ++_lastInstanceID;
            return InstanceID;
        }

        (UICanvaseType, bool) GetCanvaseInfo()
        {
            UICanvaseType canvse = UICanvaseType.Top;
            bool showMask = true;

            if (InitData != null)
            {
                showMask = !InitData.HideBGMask;
                if (InitData.ParentCanvase == UICanvaseType.Auto)
                {
                    canvse = showMask ? UICanvaseType.Top : UICanvaseType.Normal;
                }
                else
                {
                    canvse = InitData.ParentCanvase;
                }
            }
            return (canvse, showMask);
        }

        public void TrySetToTop()
        {
            (var canvasType, var showMask) = GetCanvaseInfo();
            if (UiView != null && canvasType == UICanvaseType.Top)
                _uiMgr.UIRoot.AddTopUI(UiView, showMask, true);
        }

        public void Show(bool awaken = false)
        {
            _uiAnimation?.StopPlay();
            Shown = true;
            if (UiView != null)
            {
                UiView.gameObject.SetActive(true);
                (var canvasType, var showMask) = GetCanvaseInfo();
                if (canvasType == UICanvaseType.Top)
                {
                    if (OpenType != UIOpenType.GlobalUI)
                        _uiMgr.UIRoot.AddTopUI(UiView, showMask, !awaken);
                    else
                        _uiMgr.UIRoot.AddTopUI(UiView);
                }
                else
                    _uiMgr.UIRoot.AddNormalUI(UiView);

                if (OpenType == UIOpenType.Navigate || OpenType == UIOpenType.HUD)
                    UiView.SetSortOrder(UiView.OriginalSortOrder + 1);
                if (InitData != null)
                    InitData.MuteBornAnim = false;

                _onShowEventData.awaken = awaken;
                _onShowEventData.initData = InitData;
                ExecuteEvents.Execute<IUIViewCallback>(UiView.gameObject, _onShowEventData, OnShowHandler);

                if ((!awaken || OpenType == UIOpenType.HUD) && _uiAnimation != null && !(InitData?.MuteBornAnim ?? false))
                {
                    _uiAnimation.PlayShowAnimations();
                }
            }
            else
            {
                Load();
            }
        }

        public void Hide()
        {
            _uiAnimation?.StopPlay();
            Shown = false;
            if (UiView != null)
            {
                _uiMgr.UIRoot.RemoveTopUI(UiView, false);
                _uiMgr.TrySetTopUI();
                if (OpenType == UIOpenType.HUD && _uiAnimation != null && !(InitData?.MuteDieAnim ?? false))
                {
                    ExecuteEvents.Execute<IUIViewCallback>(UiView.gameObject, null, OnHideHandler);
                    _uiAnimation.PlayHideAnimations(InitData, () =>
                    {
                        UiView.gameObject.SetActive(false);
                    });
                }
                else
                {
                    ExecuteEvents.Execute<IUIViewCallback>(UiView.gameObject, null, OnHideHandler);
                    UiView.gameObject.SetActive(false);
                }
                if (InitData != null)
                    InitData.MuteDieAnim = false;
            }
        }

        public void Close(bool destroyImmediate = false, System.Action onAfterClose = null)
        {
            _uiAnimation?.StopPlay();
            Shown = false;
            if (UiView != null)
            {
                _uiMgr.UIRoot.RemoveTopUI(UiView, false);
                _uiMgr.TrySetTopUI();

                if (!destroyImmediate && _uiAnimation != null && UiView.gameObject.activeSelf && !(InitData?.MuteDieAnim ?? false))
                {
                    if (OpenType == UIOpenType.Navigate || OpenType == UIOpenType.HUD)
                        UiView.SetSortOrder(UiView.OriginalSortOrder);
                    ExecuteEvents.Execute<IUIViewCallback>(UiView.gameObject, null, OnHideHandler);

                    _uiAnimation.PlayHideAnimations(InitData, () =>
                    {
                        ExecuteEvents.Execute<IUIViewCallback>(UiView.gameObject, null, OnCloseHandler);
                        UiView.gameObject.SetActive(false);
                        //TODO：目前关闭就释放资源，考虑增加缓存机制
                        Unload(destroyImmediate);

                        onAfterClose?.Invoke();
                    });
                }
                else
                {
                    if (UiView.gameObject.activeSelf)
                        ExecuteEvents.Execute<IUIViewCallback>(UiView.gameObject, null, OnHideHandler);
                    ExecuteEvents.Execute<IUIViewCallback>(UiView.gameObject, null, OnCloseHandler);
                    UiView.gameObject.SetActive(false);
                    //TODO：目前关闭就释放资源，考虑增加缓存机制
                    Unload(destroyImmediate);

                    onAfterClose?.Invoke();
                }
            }
        }

        public void Load()
        {
            if (_asset.IsValid()) return;

            if (InitData != null && InitData.AsyncLoad)
            {
                _asset.LoadAssetAsync<GameObject>().AddCompleted(OnLoaded);
            }
            else
            {
                InitUIView(_asset.LoadAssetSync<GameObject>());
            }
        }

        public void Unload(bool destroyImmediate = false)
        {
            if (UiView != null)
            {
                if (destroyImmediate)
                {
                    UnityEngine.Object.DestroyImmediate(UiView.gameObject);
                }
                else
                {
                    UnityEngine.Object.Destroy(UiView.gameObject);
                }

                UiView = null;
                _uiAnimation = null;
            }
            if (_asset.IsValid())
            {
                _asset.ReleaseAsset();
            }
        }

        private void OnLoaded(AsyncOperationHandle<GameObject> handle)
        {
            InitUIView(handle.Result);
        }

        private void InitUIView(GameObject go)
        {
            if (go != null)
            {
                (var canvasType, _) = GetCanvaseInfo();

                GameObject uiobj = UnityEngine.Object.Instantiate(go,
                    (canvasType == UICanvaseType.Normal) ? _uiMgr.UIRoot.normalUICanvas : _uiMgr.UIRoot.topUICanvas);
                UiView = uiobj.GetComponent<UIView>();
                UiView.SetOverrideSorting();
                UiView.OriginalSortOrder = UiView.GetSortOrder();
                UiView.SetSortOrder(_sortOrder + UiView.OriginalSortOrder);
                UiView.UIMgr = _uiMgr;
                UiView.ViewAgent = this;

                RectTransform uiTransform = uiobj.GetComponent<RectTransform>();
                uiTransform.anchorMin = Vector2.zero;
                uiTransform.anchorMax = Vector2.one;
                uiTransform.pivot = Vector2.one * .5f;
                uiTransform.sizeDelta = Vector2.zero;
                uiTransform.localScale = Vector3.one;

                UiView.BindUIEvents();
                _uiAnimation = uiobj.GetComponent<UIAnimation>();

                if (Shown) Show(false);
                else Hide();
            }
        }

        static void OnShowHandler(IUIViewCallback handler, BaseEventData arg)
        {
            if (arg is OnShowEventData e)
                handler.OnShow(e.awaken, e.initData);
        }
        static void OnHideHandler(IUIViewCallback handler, BaseEventData arg)
        {
            handler.OnHide();
        }
        static void OnCloseHandler(IUIViewCallback handler, BaseEventData arg)
        {
            handler.OnClose();
        }
    }
}
