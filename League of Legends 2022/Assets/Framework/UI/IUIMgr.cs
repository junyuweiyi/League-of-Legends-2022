
/********************************************************************
	created:	2020/09/22
	author:		ZYL
	
	purpose:	UI管理器
*********************************************************************/

using System;
using UnityEngine;

namespace iFramework
{
    public interface IGussianBlurSwitch
    {
        void TurnOn();
        void TurnOff();
        void ResetPasses();
        bool StaticBlur { set; }
        bool Dirty { set; }
    }
    public interface IUIMgr
    {
        void Initialize(IResourceMgr resMgr, string uiPath, IObjectPool pool, ILocalizationMgr localization, IEventMgr eventMgr, ITimerMgr timeMgr);
        /// <summary>
        /// 设置canvas的缩放比例，等价于缩小canvas的设计分辨率
        /// </summary>
        /// <param name="scale">把默认的1920x1080，缩小为1/scale</param>
        void SetCanvasScale(float scale);
        Camera GetUICamera();
        void SetGussianBlurSwitch(IGussianBlurSwitch blurSwitch);
        void TurnBlur(bool v);

        #region HUD相关
        /// <summary>
        /// 关闭当前HUD，打开指定HUD
        /// </summary>
        /// <typeparam name="T"></typeparam>       
        void ShowHUD(string key, IUIData initData = null);
        void CloseHUD(bool destroyImmediate = false);
        #endregion

        #region 全屏窗口相关
        /// <summary>
        /// 导航到下一个全屏窗口UI,可以通过NavigateBack返回当前UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void NavigateTo(string key, IUIData initData = null);
        /// <summary>
        /// 清空当前显示的窗口队列，切换到下一个全屏窗口UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void RedirectTo(string key, IUIData initData = null);
        void NavigateBack();
        /// <summary>
        /// 当前是否有窗口UI在显示中
        /// </summary>
        /// <returns></returns>
        bool HasWindowShown();
        void CloseAllWindows(bool showHud = true);
        #endregion

        #region 全局常驻UI
        bool ShowGlobalUI(string key, IUIData initData = null);
        void CloseGlobalUI(string key);
        #endregion

        #region 弹出窗口相关
        /// <summary>
        /// 弹窗UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initData"></param>
        /// <param name="waitInQueue">如果为true的话，要等待上一个关闭再弹出</param>
        /// <returns>窗口实例ID，用于窗口状态查询和关闭</returns>
        int Popup(string key, IUIData initData = null, bool waitInQueue = false);
        void ClosePopup(string key, int instanceID = 0);
        int TogglePopup(string key, int instanceID = 0, IUIData initData = null);
        void CloseAllPopups();
        bool HasPopupShown();
        #endregion

        /// <summary>
        /// 显示msgbox，支持同时显示多个msgbox。
        /// 可自定义确定和取消按钮的显示，如果都不显示，可点击屏幕任意位置关闭
        /// </summary>
        /// <param name="content"></param>
        /// <param name="callback"></param>
        MessageBox MsgBox(MsgBoxContent content, Action<MsgBoxEventID> callback = null);


        bool IsShown(string key, int instanceID = 0);
        void HideAll(bool hide);
        void CloseAll();

        bool TryCloseTopUI();
    }
}
