using Avalonia.OpenHarmony;
using OpenHarmony.NDK.Bindings.Native;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AvaloniaApp;

namespace Entry;

public unsafe static class XComponentEntry
{
    public static Dictionary<IntPtr, XComponent> XComponents = [];
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void OnSurfaceCreated(OH_NativeXComponent* component, void* window)
    {
        try
        {
            ace_ndk.OH_NativeXComponent_RegisterOnFrameCallback(component, &OnSurfaceRendered);
            if (XComponents.TryGetValue((nint)component, out XComponent? xComponent) == true)
                return;
            xComponent = new AvaloniaXComponent<App>((nint)component, (nint)window);
            XComponents.Add((nint)component, xComponent);
            xComponent.OnSurfaceCreated();
        }
        catch (Exception ex)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", ex.Message);
            if (ex.StackTrace != null)
            {
                Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", ex.StackTrace);
            }
            if (ex.InnerException != null)
            {
                Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", ex.InnerException.Message);
                if (ex.InnerException.StackTrace != null)
                {
                    Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", ex.InnerException.StackTrace);
                }
            }
        }
    }


    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void OnSurfaceRendered(OH_NativeXComponent* component, ulong timestamp, ulong targetTimestamp)
    {
        try
        {
            if (XComponents.TryGetValue((nint)component, out XComponent? xComponent) == false)
            return;
            xComponent.OnSurfaceRendered(timestamp, targetTimestamp);
        }
        catch (Exception ex)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", ex.Message);
            if (ex.StackTrace != null)
            {
                Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", ex.StackTrace);
            }
            if (ex.InnerException != null)
            {
                Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", ex.InnerException.Message);
                if (ex.InnerException.StackTrace != null)
                {
                    Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", ex.InnerException.StackTrace);
                }
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void OnSurfaceChanged(OH_NativeXComponent* component, void* window)
    {
        if (XComponents.TryGetValue((nint)component, out XComponent? xComponent) == false)
            return;
        xComponent.OnSurfaceChanged();
    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void OnSurfaceDestroyed(OH_NativeXComponent* component, void* window)
    {
        if (XComponents.TryGetValue((nint)component, out XComponent? xComponent) == false)
            return;
        xComponent.OnSurfaceDestroyed();
        XComponents.Remove((nint)component);

    }
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static void DispatchTouchEvent(OH_NativeXComponent* component, void* window)
    {
        if (XComponents.TryGetValue((nint)component, out XComponent? xComponent) == false)
            return;
        xComponent.DispatchTouchEvent();
    }
}
