using System.Collections.ObjectModel;
using OpenHarmony.NDK.Bindings.Native;

namespace Avalonia.OpenHarmony;

public static class OHDebugHelper
{
    private const string CSharp = "CSharp";
    private static readonly ObservableCollection<LogRecord> _logs = new();
    public static ReadOnlyObservableCollection<LogRecord> Logs { get; } = new(_logs);
    public static int MaxLogCount { get; set; } = 1000;

    public static void Debug(string log)
    {
        AddLog(LogLevel.LOG_DEBUG, log);
    }

    public static void Error(string title, Exception exception)
    {
        AddLog(LogLevel.LOG_ERROR, $"{title}\n{exception}");
    }

    public static void Fatal(string log)
    {
        AddLog(LogLevel.LOG_FATAL, log);
    }

    public static void Info(string log)
    {
        AddLog(LogLevel.LOG_INFO, log);
    }

    public static void Warn(string log)
    {
        AddLog(LogLevel.LOG_WARN, log);
    }

    public static void AddLog(LogLevel logLevel, string log)
    {
        LogRecord logRecord = new(logLevel, log);
        if (_logs.Count > MaxLogCount) _logs.Clear();

        _logs.Add(logRecord);
        Hilog.OH_LOG_PRINT(LogType.LOG_APP, logLevel, CSharp, log);
    }
}

public record LogRecord(LogLevel LogLevel, string Message)
{
    public DateTime Time { get; } = DateTime.Now;
}