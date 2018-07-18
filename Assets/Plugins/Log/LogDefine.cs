using UnityEngine;

public class LogDefine<T>
{
    [System.Diagnostics.Conditional("VERBOSE_LOG")]
    public static void Verbose(string message, Object context)
    {
        Debug.Log(PostProcess(message), context);
    }

    [System.Diagnostics.Conditional("VERBOSE_LOG")]
    public static void Verbose(string message)
    {
        Debug.Log(PostProcess(message));
    }

    [System.Diagnostics.Conditional("VERBOSE_LOG")]
    public static void VerboseFormat(Object context, string format, params object[] args)
    {
        Log(string.Format(format, args), context);
    }

    [System.Diagnostics.Conditional("VERBOSE_LOG")]
    public static void VerboseFormat(string format, params object[] args)
    {
        Log(string.Format(format, args));
    }

    public static void Log(string message, Object context)
    {
        Debug.Log(PostProcess(message), context);
    }

    public static void Log(string message)
    {
        Debug.Log(PostProcess(message));
    }

    public static void LogFormat(Object context, string format, params object[] args)
    {
        Log(string.Format(format, args), context);
    }

    public static void LogFormat(string format, params object[] args)
    {
        Log(string.Format(format, args));
    }

    public static void LogWarning(string message, Object context)
    {
        Debug.LogWarning(PostProcess(message), context);
    }

    public static void LogWarning(string message)
    {
        Debug.LogWarning(PostProcess(message));
    }

    public static void LogWarningFormat(Object context, string format, params object[] args)
    {
        LogWarning(string.Format(format, args), context);
    }

    public static void LogWarningFormat(string format, params object[] args)
    {
        LogWarning(string.Format(format, args));
    }

    public static void LogError(string message, Object context)
    {
        Debug.LogError(PostProcess(message), context);
    }

    public static void LogError(string message)
    {
        Debug.LogError(PostProcess(message));
    }

    public static void LogErrorFormat(Object context, string format, params object[] args)
    {
        LogError(string.Format(format, args), context);
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        LogError(string.Format(format, args));
    }

    public static void LogException(System.Exception e)
    {
        Debug.LogException(e);
    }

    public static void LogException(System.Exception e, Object context)
    {
        Debug.LogException(e, context);
    }

    static LogDefine()
    {
        _name = typeof(T).Name;
        if (_name.EndsWith("Log"))
            _name = _name.Substring(0, _name.Length - 3);
    }

    static string PostProcess(string message)
    {
        return string.Format("[{0}] {1}", _name, message);
    }

    static string _name;
}