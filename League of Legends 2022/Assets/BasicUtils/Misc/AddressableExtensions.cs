/********************************************************************
	created:	2021/08/03
	author:		maoqy
	
	purpose:	Addressable 系统相关的扩展函数
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressableExtensions
{
    /// <summary>
    /// 添加 Completed 事件<br/>
    /// <strong>不要直接 Completed += ，会导致抛异常 Reentering the Update method is not allowed...</strong>
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="value"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static AsyncOperationHandle<TObject> AddCompleted<TObject>(this AsyncOperationHandle<TObject> handle, Action<AsyncOperationHandle<TObject>> value)
    {
        if (handle.IsValid() && !handle.IsDone)
            handle.Completed += value;
        else
            value(handle);
        return handle;
    }
    /// <summary>
    /// 移除 Completed 事件<br/>
    /// <strong>不要直接 Completed += ，会导致抛异常 Reentering the Update method is not allowed...</strong>
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="value"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static AsyncOperationHandle<TObject> RemoveCompleted<TObject>(this AsyncOperationHandle<TObject> handle, Action<AsyncOperationHandle<TObject>> value)
    {
        if (handle.IsValid())
            handle.Completed -= value;
        return handle;
    }
    /// <summary>
    /// 添加 CompletedTypeless 事件<br/>
    /// <strong>不要直接 Completed += ，会导致抛异常 Reentering the Update method is not allowed...</strong>
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="value"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static AsyncOperationHandle<TObject> AddCompletedTypeless<TObject>(this AsyncOperationHandle<TObject> handle, Action<AsyncOperationHandle> value)
    {
        if (handle.IsValid() && !handle.IsDone)
            handle.CompletedTypeless += value;
        else
            value(handle);
        return handle;
    }
    /// <summary>
    /// 移除 CompletedTypeless 事件<br/>
    /// <strong>不要直接 Completed += ，会导致抛异常 Reentering the Update method is not allowed...</strong>
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="value"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static AsyncOperationHandle<TObject> RemoveCompletedTypeless<TObject>(this AsyncOperationHandle<TObject> handle, Action<AsyncOperationHandle> value)
    {
        if (handle.IsValid())
            handle.CompletedTypeless -= value;
        return handle;
    }
    /// <summary>
    /// 添加 Completed 事件<br/>
    /// <strong>不要直接 Completed += ，会导致抛异常 Reentering the Update method is not allowed...</strong>
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="value"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static AsyncOperationHandle AddCompleted(this AsyncOperationHandle handle, Action<AsyncOperationHandle> value)
    {
        if (handle.IsValid() && !handle.IsDone)
            handle.Completed += value;
        else
            value(handle);
        return handle;
    }
    /// <summary>
    /// 移除 Completed 事件<br/>
    /// <strong>不要直接 Completed += ，会导致抛异常 Reentering the Update method is not allowed...</strong>
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="value"></param>
    /// <typeparam name="TObject"></typeparam>
    /// <returns></returns>
    public static AsyncOperationHandle RemoveCompleted(this AsyncOperationHandle handle, Action<AsyncOperationHandle> value)
    {
        if (handle.IsValid())
            handle.Completed -= value;
        return handle;
    }
}
