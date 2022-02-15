using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace iFramework
{
    public interface IUIViewCallback : IEventSystemHandler
    {
        void OnShow(bool awaken, IUIData initData);

        void OnHide();

        void OnClose();
    }
}