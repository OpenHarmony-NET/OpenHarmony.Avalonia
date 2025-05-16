using Avalonia.Logging;
using System.Text;
using OpenHarmony.NDK.Bindings.Native;
using Avalonia.Utilities;

namespace Avalonia.OpenHarmony
{
    public static class OHLogExtensions
    {
        public static AppBuilder LogToMySink(this AppBuilder builder,
            LogEventLevel level = LogEventLevel.Warning,
            params string[] areas)
        {
            Logger.Sink = new MyLogSink(level, areas);
            return builder;
        }
    }

    public class MyLogSink:ILogSink
    {
        private readonly LogEventLevel _level;
        private readonly IList<string>? _areas;
        public MyLogSink(LogEventLevel level, params string[] areas)
        {
            _level= level;
            _areas = areas?.Count() > 0 ? areas : null;
        }

        public bool IsEnabled(LogEventLevel level, string area)
        {
            return level >= _level && (_areas?.Contains(area) ?? true);
        }

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
        {
            if (IsEnabled(level, area))
                Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", Format<object, object, object>(area, messageTemplate, source, null));
        }

        public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
        {
            if (IsEnabled(level, area))
                Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", Format(area, messageTemplate, source, propertyValues));
        }

        private static string Format<T0, T1, T2>(
            string area,
            string template,
            object? source,
            object?[]? values)
        {
            var result = new StringBuilder();
            var r = new CharacterReader(template.AsSpan());
            var i = 0;

            result.Append('[');
            result.Append(area);
            result.Append("] ");

            while (!r.End)
            {
                var c = r.Take();

                if (c != '{')
                {
                    result.Append(c);
                }
                else
                {
                    if (r.Peek != '{')
                    {
                        result.Append('\'');
                        result.Append(values?[i++]);
                        result.Append('\'');
                        r.TakeUntil('}');
                        r.Take();
                    }
                    else
                    {
                        result.Append('{');
                        r.Take();
                    }
                }
            }

            FormatSource(source, result);
            return result.ToString();
        }

        private static string Format(
            string area,
            string template,
            object? source,
            object?[] v)
        {
            var result = new StringBuilder();
            var r = new CharacterReader(template.AsSpan());
            var i = 0;

            result.Append('[');
            result.Append(area);
            result.Append(']');

            while (!r.End)
            {
                var c = r.Take();

                if (c != '{')
                {
                    result.Append(c);
                }
                else
                {
                    if (r.Peek != '{')
                    {
                        result.Append('\'');
                        result.Append(i < v.Length ? v[i++] : null);
                        result.Append('\'');
                        r.TakeUntil('}');
                        r.Take();
                    }
                    else
                    {
                        result.Append('{');
                        r.Take();
                    }
                }
            }

            FormatSource(source, result);
            return result.ToString();
        }

        private static void FormatSource(object? source, StringBuilder result)
        {
            if (source is null)
                return;

            result.Append(" (");
            result.Append(source.GetType().Name);
            result.Append(" #");

            if (source is StyledElement se && se.Name is not null)
                result.Append(se.Name);
            else
                result.Append(source.GetHashCode());

            result.Append(')');
        }
    }
}
