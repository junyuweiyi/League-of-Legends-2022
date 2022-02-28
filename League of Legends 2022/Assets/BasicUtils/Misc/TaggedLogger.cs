using System;
using System.Diagnostics;
using UnityEngine;
using iFramework;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;

public class TaggedLogger
{
    private const string kTagFormat = "{0}: {1}";

    public ILogger Logger { get; set; }
    public bool LogEnabled { get; set; }
    public LogLevel FilterLogType { get; set; }
    public string Tag { get; private set; }

    public TaggedLogger(string tag, ILogger logger = null)
    {
        Tag = tag;
        Logger = logger ?? Debug.unityLogger;
        LogEnabled = true;
        FilterLogType = LogLevel.Log;
    }

    public bool IsLogTypeAllowed(LogLevel logType)
    {
        if (LogEnabled)
        {
            if (logType == LogLevel.Exception)
            {
                return true;
            }
            if (FilterLogType != LogLevel.Exception)
            {
                return logType <= FilterLogType;
            }
        }
        return false;
    }

    public void Log(LogLevel logType, object message, Object context)
    {
        if (IsLogTypeAllowed(logType))
        {
            Logger.Log(LogLevelToUnityLogType(logType), Tag, message, context);
        }
    }

    public void Log(object message)
    {
        Log(LogLevel.Log, message, null);
    }

    public void Log(object message, Object context)
    {
        Log(LogLevel.Log, message, context);
    }

    public void Verbose(object message)
    {
        Log(LogLevel.Verbose, message, null);
    }

    public void Verbose(object message, Object context)
    {
        Log(LogLevel.Verbose, message, context);
    }

    public void LogFormat(LogLevel logType, Object context, string format, params object[] args)
    {
        if (IsLogTypeAllowed(logType))
        {
            var unityLogType = LogLevelToUnityLogType(logType);
            if (Logger.IsLogTypeAllowed(unityLogType))
                Logger.LogFormat(unityLogType, context, Util.String.Format(kTagFormat, Tag, format), args);
        }
    }

    public void LogFormat(string format, params object[] args)
    {
        LogFormat(LogLevel.Log, null, format, args);
    }

    public void LogFormat(Object context, string format, params object[] args)
    {
        LogFormat(LogLevel.Log, context, format, args);
    }

    public void VerboseFormat(string format, params object[] args)
    {
        LogFormat(LogLevel.Verbose, null, format, args);
    }

    public void VerboseFormat(Object context, string format, params object[] args)
    {
        LogFormat(LogLevel.Verbose, context, format, args);
    }

    public void LogError(object message)
    {
        Log(LogLevel.Error, message, null);
    }

    public void LogError(object message, Object context)
    {
        Log(LogLevel.Error, message, context);
    }

    public void LogErrorFormat(string format, params object[] args)
    {
        LogFormat(LogLevel.Error, null, format, args);
    }

    public void LogErrorFormat(Object context, string format, params object[] args)
    {
        LogFormat(LogLevel.Error, context, format, args);
    }

    public void LogException(Exception exception)
    {
        LogException(exception, null);
    }

    public void LogException(Exception exception, Object context)
    {
        if (LogEnabled)
        {
            Log(LogLevel.Exception, exception, context);
            // Logger.LogException(exception, context);
        }
    }

    public void LogWarning(object message)
    {
        Log(LogLevel.Warning, message, null);
    }

    public void LogWarning(object message, Object context)
    {
        Log(LogLevel.Warning, message, context);
    }

    public void LogWarningFormat(string format, params object[] args)
    {
        LogFormat(LogLevel.Warning, null, format, args);
    }

    public void LogWarningFormat(Object context, string format, params object[] args)
    {
        LogFormat(LogLevel.Warning, context, format, args);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void Assert(bool condition)
    {
        if (!condition)
        {
            Log(LogLevel.Assert, "Assertion failed", null);
        }
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void Assert(bool condition, Object context)
    {
        if (!condition)
        {
            Log(LogLevel.Assert, (object)"Assertion failed", context);
        }
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void Assert(bool condition, object message)
    {
        if (!condition)
        {
            Log(LogLevel.Assert, message, null);
        }
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Log(LogLevel.Assert, message, null);
        }
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void Assert(bool condition, object message, Object context)
    {
        if (!condition)
        {
            Log(LogLevel.Assert, message, context);
        }
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void Assert(bool condition, string message, Object context)
    {
        if (!condition)
        {
            Log(LogLevel.Assert, (object)message, context);
        }
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void AssertFormat(bool condition, string format, params object[] args)
    {
        if (!condition)
        {
            LogFormat(LogLevel.Assert, null, format, args);
        }
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void AssertFormat(bool condition, Object context, string format, params object[] args)
    {
        if (!condition)
        {
            LogFormat(LogLevel.Assert, context, format, args);
        }
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void LogAssertion(object message)
    {
        Log(LogLevel.Assert, message, null);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void LogAssertion(object message, Object context)
    {
        Log(LogLevel.Assert, message, context);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void LogAssertionFormat(string format, params object[] args)
    {
        LogFormat(LogLevel.Assert, null, format, args);
    }

    [Conditional("UNITY_ASSERTIONS")]
    public void LogAssertionFormat(Object context, string format, params object[] args)
    {
        LogFormat(LogLevel.Assert, context, format, args);
    }

    public static LogLevel LogLevelFromUnityLogType(UnityEngine.LogType unityLogType)
    {
        if (unityLogType <= UnityEngine.LogType.Log)
            return (LogLevel)unityLogType;
        if (unityLogType == UnityEngine.LogType.Exception)
            return LogLevel.Exception;
        return LogLevel.Log;
    }
    public static UnityEngine.LogType LogLevelToUnityLogType(LogLevel logType)
    {
        if (logType <= LogLevel.Log)
            return (UnityEngine.LogType)logType;
        if (logType == LogLevel.Exception)
            return UnityEngine.LogType.Exception;
        return UnityEngine.LogType.Log;
    }
}

public enum LogLevel
{
    //
    // 摘要:
    //     LogType used for Errors.
    Error,
    //
    // 摘要:
    //     LogType used for Asserts. (These could also indicate an error inside Unity itself.)
    Assert,
    //
    // 摘要:
    //     LogType used for Warnings.
    Warning,
    //
    // 摘要:
    //     LogType used for regular log messages.
    Log,
    // 优先级更低的，更详细的日志
    Verbose,
    //
    // 摘要:
    //     LogType used for Exceptions.
    Exception
}
