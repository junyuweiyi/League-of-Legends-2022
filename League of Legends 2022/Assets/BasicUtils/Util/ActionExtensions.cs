/********************************************************************
	created:	2020/09/17
	author:		maoqy
	
	purpose:	System.Action 的扩展函数
*********************************************************************/

using System;

using UnityEngine;

namespace iFramework
{
    public static class ActionExtensions
    {
        /// <summary>
        /// 安全地调用Action，捕获其中抛出的异常
        /// </summary>
        /// <param name="action">要调用的Action</param>
        /// <returns>是否调用成功</returns>
        public static bool InvokeSafely(this Action action)
        {
            try
            {
                action?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// 安全地调用Action&lt; T &gt;，捕获其中抛出的异常
        /// </summary>
        /// <param name="action">要调用的Action</param>
        /// <param name="arg">传递给Action的参数</param>
        /// <typeparam name="T">Action的参数类型</typeparam>
        /// <returns>是否调用成功</returns>
        public static bool InvokeSafely<T>(this Action<T> action, T arg)
        {
            try
            {
                action?.Invoke(arg);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// 安全地调用Action&lt; T1, T2 &gt;，捕获其中抛出的异常
        /// </summary>
        /// <param name="action">要调用的Action</param>
        /// <param name="arg1">传递给Action的参数</param>
        /// <typeparam name="T">Action的参数类型</typeparam>
        /// <returns>是否调用成功</returns>
        public static bool InvokeSafely<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            try
            {
                action?.Invoke(arg1, arg2);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// 安全地调用Action&lt; T1, T2, T3&gt;，捕获其中抛出的异常
        /// </summary>
        /// <param name="action">要调用的Action</param>
        /// <param name="arg1">传递给Action的参数</param>
        /// <typeparam name="T">Action的参数类型</typeparam>
        /// <returns>是否调用成功</returns>
        public static bool InvokeSafely<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            try
            {
                action?.Invoke(arg1, arg2, arg3);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
        /// <summary>
        /// 安全地调用Action&lt; T1, T2, T3, T4&gt;，捕获其中抛出的异常
        /// </summary>
        /// <param name="action">要调用的Action</param>
        /// <param name="arg1">传递给Action的参数</param>
        /// <typeparam name="T">Action的参数类型</typeparam>
        /// <returns>是否调用成功</returns>
        public static bool InvokeSafely<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            try
            {
                action?.Invoke(arg1, arg2, arg3, arg4);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// 安全地调用Func&lt; T1, T2 &gt;，捕获其中抛出的异常
        /// </summary>
        /// <param name="action">要调用的Func</param>
        /// <param name="arg">传递给Func的参数</param>
        /// <param name="defaultValue">发生异常时的返回值</param>
        /// <typeparam name="T1">Func的参数类型</typeparam>
        /// <typeparam name="T2">Func的返回类型</typeparam>
        /// <returns>Func调用的返回值</returns>
        public static T2 InvokeSafely<T1, T2>(this Func<T1, T2> action, T1 arg, T2 defaultValue = default(T2))
        {
            try
            {
                return action.Invoke(arg);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return defaultValue;
            }
        }
    }
}