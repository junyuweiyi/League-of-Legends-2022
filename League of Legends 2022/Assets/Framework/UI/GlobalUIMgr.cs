/********************************************************************
	created:	2021/01/27
	author:		maoqy
	
	purpose:	长期存在的UI
*********************************************************************/

using System;
using System.Collections.Generic;

namespace iFramework
{
    internal class GlobalUIMgr
    {
        private UIMgr _uiMgr;
        private List<ViewAgent> _globalUIViews = new List<ViewAgent>();
        public GlobalUIMgr(UIMgr uiMgr)
        {
            _uiMgr = uiMgr;
        }

        public bool Show(string key, IUIData initData)
        {
            if (initData == null) initData = new UIData() { HideBGMask = true, ParentCanvase = UICanvaseType.Top };
            ViewAgent agent = _uiMgr.GetAgent(key);
            if (_globalUIViews.Contains(agent))
                return false;

            agent.InitData = initData;
            _globalUIViews.Add(agent);
            agent.OpenType = UIOpenType.GlobalUI;

            agent.Show();
            return true;
        }

        public void Close(string key)
        {
            ViewAgent agent = _uiMgr.GetAgent(key);
            if (_globalUIViews.Remove(agent))
                agent.Close();
        }

        public void Close(ViewAgent agent)
        {
            if (_globalUIViews.Remove(agent))
                agent.Close();
        }

        public void CloseAll(bool destroyImmediate = false)
        {
            for (int i = 0; i < _globalUIViews.Count; i++)
            {
                _globalUIViews[i].Close(destroyImmediate);
            }
            _globalUIViews.Clear();
        }
    }
}
