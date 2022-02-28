/********************************************************************
	created:	2020/09/27
	author:		maoqy
	
	purpose:	Unity组件相关扩展函数
*********************************************************************/

using UnityEngine;

namespace iFramework
{
    public static class ComponentExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject self) where T : Component
        {
            var com = self.GetComponent<T>();
            if (!com) com = self.AddComponent<T>();
            return com;
        }
        public static T GetOrAddComponent<T>(this Component self) where T : Component
        {
            var com = self.GetComponent<T>();
            if (!com) com = self.gameObject.AddComponent<T>();
            return com;
        }
    }
}