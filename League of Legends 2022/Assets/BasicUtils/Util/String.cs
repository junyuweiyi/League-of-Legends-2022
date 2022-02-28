/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	字符串帮助类，使用StringBuilder，减少GC
*********************************************************************/

using System;
using System.Text;

namespace iFramework
{
    public static partial class Util
    {
        /// <summary>
        /// 字符相关的实用函数。
        /// </summary>
        public static class String
        {
            [ThreadStatic]
            private static StringBuilder s_CachedStringBuilder = null;

            /// <summary>
            /// 获取格式化字符串。
            /// </summary>
            /// <param name="format">字符串格式。</param>
            /// <param name="arg0">字符串参数 0。</param>
            /// <returns>格式化后的字符串。</returns>
            public static string Format(string format, object arg0)
            {
                if (format == null)
                {
                    throw new Exception("Format is invalid.");
                }

                CheckCachedStringBuilder();
                s_CachedStringBuilder.Length = 0;
                try
                {
                    s_CachedStringBuilder.AppendFormat(format, arg0);
                }
                catch (System.Exception e) // catch format error
                {
                    UnityEngine.Debug.LogError($"format {format} with error {e}");
                }
                return s_CachedStringBuilder.ToString();
            }

            /// <summary>
            /// 获取格式化字符串。
            /// </summary>
            /// <param name="format">字符串格式。</param>
            /// <param name="arg0">字符串参数 0。</param>
            /// <param name="arg1">字符串参数 1。</param>
            /// <returns>格式化后的字符串。</returns>
            public static string Format(string format, object arg0, object arg1)
            {
                if (format == null)
                {
                    throw new Exception("Format is invalid.");
                }

                CheckCachedStringBuilder();
                s_CachedStringBuilder.Length = 0;
                try
                {
                    s_CachedStringBuilder.AppendFormat(format, arg0, arg1);
                }
                catch (System.Exception e) // catch formt error
                {
                    UnityEngine.Debug.LogError($"format {format} with error {e}");
                }
                return s_CachedStringBuilder.ToString();
            }

            /// <summary>
            /// 获取格式化字符串。
            /// </summary>
            /// <param name="format">字符串格式。</param>
            /// <param name="arg0">字符串参数 0。</param>
            /// <param name="arg1">字符串参数 1。</param>
            /// <param name="arg2">字符串参数 2。</param>
            /// <returns>格式化后的字符串。</returns>
            public static string Format(string format, object arg0, object arg1, object arg2)
            {
                if (format == null)
                {
                    throw new Exception("Format is invalid.");
                }

                CheckCachedStringBuilder();
                s_CachedStringBuilder.Length = 0;
                //s_CachedStringBuilder.AppendFormat(format, arg0, arg1, arg2);
                try
                {
                    s_CachedStringBuilder.AppendFormat(format, arg0, arg1, arg2);
                }
                catch (System.Exception e) // catch formt error
                {
                    UnityEngine.Debug.LogError($"format {format} with error {e}");
                }
                return s_CachedStringBuilder.ToString();
            }

            /// <summary>
            /// 获取格式化字符串。
            /// </summary>
            /// <param name="format">字符串格式。</param>
            /// <param name="args">字符串参数。</param>
            /// <returns>格式化后的字符串。</returns>
            public static string Format(string format, params object[] args)
            {
                if (format == null)
                {
                    throw new Exception("Format is invalid.");
                }

                if (args == null)
                {
                    return format;
                }

                CheckCachedStringBuilder();
                s_CachedStringBuilder.Length = 0;
                try
                {
                    s_CachedStringBuilder.AppendFormat(format, args);
                }
                catch (System.Exception e) // catch formt error
                {
                    UnityEngine.Debug.LogError($"format {format} with error {e}");
                }
                //s_CachedStringBuilder.AppendFormat(format, args);
                return s_CachedStringBuilder.ToString();
            }

            public static string Dump(object obj, int depth = 3)
            {
                CheckCachedDumpWriter();
                s_CachedStringBuilder.Length = 0;
                ObjectDumper.Write(obj, depth, s_CachedDumpWriter);
                s_CachedDumpWriter.Flush();
                return s_CachedStringBuilder.ToString();
            }

            static System.IO.StringWriter s_CachedDumpWriter;

            private static void CheckCachedDumpWriter()
            {
                if (s_CachedDumpWriter == null)
                {
                    CheckCachedStringBuilder();
                    s_CachedDumpWriter = new System.IO.StringWriter(s_CachedStringBuilder);
                }
            }

            private static void CheckCachedStringBuilder()
            {
                if (s_CachedStringBuilder == null)
                {
                    s_CachedStringBuilder = new StringBuilder(1024);
                }
            }
        }
    }
}
