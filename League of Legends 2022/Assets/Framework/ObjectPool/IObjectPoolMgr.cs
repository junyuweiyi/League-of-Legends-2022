/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	Gameobject对象池
*********************************************************************/

using UnityEngine;

namespace iFramework
{
    public interface IObjectPoolMgr
    {
        void Initialize(IResourceMgr resMgr);
        /// <summary>
        /// 创建对象池。游戏中建议按照对象缓存生命期策略创建多个对象池：如全局对象池（缓存全局常用的对象：UI特效等），
        /// 战斗关卡对象池（缓存战斗关卡中的怪物、战斗特效等，关卡结束后清空或销毁对象池）
        /// </summary>
        /// <param name="parent">对象池所挂载的父对象</param>
        /// <param name="name">对象池名称，仅为了在场景中查看时更直观方便</param>
        /// <returns></returns>
        IObjectPool CreatePool(Transform parent, string name = null);
        IObjectPool CreateOffScreenPool(Transform parent, string name = null);

        void DestoryPool(IObjectPool pool);
    }
}