using OpenHarmony.NDK.Bindings.Native;
using System;
using System.Runtime.InteropServices;

namespace AvaloniaSilk.Gl
{
    public class Logger
    {
        public static void Log(string tag, string message)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.WriteLine($"[{tag}] {message}");
                return;
            }
            Hilog.OH_LOG_INFO(LogType.LOG_APP, tag, message);

        }
        public static void LogError(string tag, string message)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.WriteLine($"[{tag} error] {message}");
                return;
            }
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, tag, message);
        }
    }
}
