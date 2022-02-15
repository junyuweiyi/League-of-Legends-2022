/********************************************************************
	created:	2020/09/22
	author:		ZYL
	
	purpose:	
*********************************************************************/
using UnityEngine;

namespace iFramework
{
    public enum UICanvaseType
    {
        Auto,
        Normal,
        Top
    }

    /// <summary>
    /// UI初始化数据的接口<br/>
    /// 使用接口是因为要兼容 lua 传 table
    /// 各UI自定义的结构继承于 <c>UIData</c>，不要继承于这个接口<br/>
    /// </summary>
    public interface IUIData
    {
        bool AsyncLoad { get; set; }
        bool MuteBornAnim { get; set; }
        bool MuteDieAnim { get; set; }
        bool HideBGMask { get; set; }
        UICanvaseType ParentCanvase { get; set; }
    }
    /// <summary>
    /// UI初始化数据，各个UI根据需求继承实现
    /// </summary>
    public class UIData : IUIData
    {
        /// <summary>
        /// 是否异步加载，默认为 <c>false</c><br/>
        /// 子类可以重写默认值
        /// </summary>
        /// <value></value>
        public virtual bool AsyncLoad { get; set; }
        /// <summary>
        /// 是否关闭出生动画
        /// </summary>
        public virtual bool MuteBornAnim { get; set; }
        /// <summary>
        /// 是否关闭死亡动画
        /// </summary>
        public virtual bool MuteDieAnim { get; set; }
        /// <summary>
        /// 是否显示背景蒙板
        /// </summary>
        public virtual bool HideBGMask { get; set; }

        public virtual UICanvaseType ParentCanvase { get; set; }
    }

    /// <summary>
    /// UI窗口视图基类，每个具体UI继承该类，
    /// 挂在UI Prefab上，管理该UI窗口的显示。
    /// 派生出来的每个子类按需要添加如下静态变量，指定对应的prefab资源路径和加载方式。
    /// public static string prefabName = "TestUI/TestUI2";
    /// public static bool asyncLoad = false;
    /// 如果没有这两个变量，以类名构造资源路径："TestUI/TestUI", 默认同步加载
    /// </summary>
    public class UIView : MonoBehaviour, IUIViewCallback
    {
        // Framework使用，请勿修改
        internal int OriginalSortOrder { get; set; }
        internal UIMgr UIMgr { get; set; }
        internal ViewAgent ViewAgent { get; set; }

        void IUIViewCallback.OnShow(bool awaken, IUIData initData) => OnShow(awaken, initData);
        void IUIViewCallback.OnHide() => OnHide();
        void IUIViewCallback.OnClose() => OnClose();

        public virtual void OnMaskBGClick()
        {
            Close();
        }

        protected virtual void OnShow(bool awaken, IUIData initData) { }

        protected virtual void OnHide() { }

        protected virtual void OnClose() { }

        /// <summary>
        /// UIBindMarker 关联的事件绑定，会由工具生成重写代码，不要自己重写和调用！！
        /// </summary>
        protected internal virtual void BindUIEvents() { }
        /// <summary>
        /// UIBindMarker 关联的事件绑定，会由工具生成重写代码，不要自己重写和调用！！
        /// </summary>
        protected internal virtual void UnbindUIEvents() { }

        protected void Close()
        {
            UIMgr.Close(ViewAgent);
        }

        protected internal virtual void SetSortOrder(int order)
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError(GetType() + ":canvas not found.");
                return;
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;
        }

        protected internal virtual void SetOverrideSorting()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError(GetType() + ":canvas not found.");
                return;
            }
            canvas.overrideSorting = true;
        }

        protected internal int GetSortOrder()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError(GetType() + ":canvas not found.");
                return 0;
            }
            return canvas.sortingOrder;
        }
    }
}
