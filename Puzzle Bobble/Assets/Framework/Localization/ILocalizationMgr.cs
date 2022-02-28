
/********************************************************************
	created:	2020/09/23
	author:		maoqy
	
	purpose:	本地化管理器，根据当前设置的语言提供对应语言的文本
*********************************************************************/
using System;

namespace iFramework
{
    /// <summary>
    /// 本地化管理器，根据当前设置的语言提供对应语言的文本
    /// </summary>
    public interface ILocalizationMgr
    {
        /// <summary>
        /// 注册本地化文件加载或切换的事件<br/>
        /// 增加这个接口是因为使用delegate方式监听事件时，将函数转换为delegate会生成新对象<br/>
        /// 界面中的UILocalize可能会比较多，每个UILocalize都会监听本地化变更事件，监听时可能就会产生很多垃圾对象
        /// </summary>
        /// <param name="listener"></param>
        void RegisterLocalizationLoadListener(ILocalizationLoadListener listener);
        /// <summary>
        /// 取消注册本地化文件加载或切换的事件
        /// </summary>
        /// <param name="listener"></param>
        void UnregisterLocalizationLoadListener(ILocalizationLoadListener listener);

        /// <summary>
        /// 初始化，设置数据表管理器
        /// </summary>
        /// <param name="dataMgr"></param>
        void Initialize(IDataMgr dataMgr);
        /// <summary>
        /// 加载相应语言的文本文件
        /// </summary>
        /// <param name="locale">语言代码，需要与本地化文件中配置的语言相一致</param>
        void Load(string locale);
        /// <summary>
        /// 根据key获取文本
        /// </summary>
        /// <param name="key">文本的key</param>
        /// <param name="returnKeyIfNotFound">没找到文本就返回key</param>
        /// <param name="defaultValue">没找到并且returnKeyIfNotFound为false就返回这个值</param>
        /// <returns></returns>
        string Get(string key, bool returnKeyIfNotFound = true, string defaultValue = "");

        string Format(string key, object arg0);
        string Format(string key, object arg0, object arg1);
        string Format(string key, object arg0, object arg1, object arg2);
        string Format(string key, params object[] args);
    }
}