using Avalonia.Controls.Embedding;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.OpenGL.Egl;
using OpenHarmony.Sdk.Native;
using Silk.NET.OpenGLES;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Avalonia.OpenHarmony;

public class AvaloniaXComponent<TApp> : XComponent where TApp : Application, new()
{
    public SingleViewLifetime? SingleViewLifetime;
    public EmbeddableControlRoot? Root;
    public TopLevelImpl? TopLevelImpl;

    public bool UseSoftRenderer = true;
    GL? gl;
    nint display;
    nint surface;
    EglInterface? egl;

    private readonly TouchDevice _touchDevice;
    private readonly MouseDevice _mouseDevice;
    private readonly PenDevice _penDevice;
    public AvaloniaXComponent(nint XComponentHandle, nint WindowHandle) : base(XComponentHandle, WindowHandle)
    {
        _touchDevice = new TouchDevice();
        _penDevice = new PenDevice();
        _mouseDevice = new MouseDevice();
    }

    public unsafe void InitOpenGlEnv()
    {
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "InitOpenGlEnv PrivateMemorySize64 1: " + Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB");
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "InitOpenGlEnv VirtualMemorySize64 1: " + Process.GetCurrentProcess().VirtualMemorySize64 / 1024 / 1024 + "MB");

        egl = new EglInterface("libEGL.so");

        display = egl.GetDisplay(0);
        if (egl.Initialize(display, out var major, out var minor) == false)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl.Initialize fail");
            return;
        }

        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "InitOpenGlEnv PrivateMemorySize64 2: " + Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB");
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "InitOpenGlEnv VirtualMemorySize64 2: " + Process.GetCurrentProcess().VirtualMemorySize64 / 1024 / 1024 + "MB");
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl init success");

        int[] attributes = [0x3033, 0x0004, 0x3024, 8, 0x3023, 8, 0x3022, 8, 0x3021, 8, 0x3040, 0x0004, 0x3038];
        if (egl.ChooseConfig(display, attributes, out var configs, 1, out var choosenConfig) == false)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl.ChooseConfig fail");
            return;
        }

        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "InitOpenGlEnv memory 2: " + Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB");
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl init success");

        int[] winAttribs = [0x309D, 0x3089, 0x3038];
        surface = egl.CreateWindowSurface(display, configs, WindowHandle, winAttribs);
        if (surface == 0)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl.CreateWindowSurface fail");
            return;
        }

        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "InitOpenGlEnv memory 2: " + Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB");
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl init success");

        int[] attrib3_list = [0x3098, 2, 0x3038];
        int sharedEglContext = 0;
        var context = egl.CreateContext(display, configs, sharedEglContext, attrib3_list);
        if (egl.MakeCurrent(display, surface, surface, context) == false)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl.MakeCurrent fail");
            return;
        }

        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "InitOpenGlEnv memory 2: " + Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB");
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl init success");


        gl = GL.GetApi(name =>
        {
            var ptr = Marshal.StringToHGlobalAnsi(name);
            var fun = egl.GetProcAddress(ptr);
            Marshal.FreeHGlobal(ptr);
            return fun;
        });
    }

    public override void OnSurfaceCreated()
    {
        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "OnSurfaceCreated memory 1: " + Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB");
        try
        {
            if (UseSoftRenderer == true)
            {
                InitOpenGlEnv();
            }
            var builder = CreateAppBuilder();
            if (UseSoftRenderer)
            {
                builder.UseSoftwareRenderer();
                if (gl != null)
                {
                    AvaloniaLocator.CurrentMutable.Bind<GL>().ToConstant(gl);
                }
            }
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "OnSurfaceCreated memory 2: " + Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB");

            SingleViewLifetime = new SingleViewLifetime();
            builder.AfterApplicationSetup(CreateView).SetupWithLifetime(SingleViewLifetime);

            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "OnSurfaceCreated memory 3: " + Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB");

            Root?.StartRendering();

            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "OnSurfaceCreated memory 4: " + Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024 + "MB");

        } 
        catch (Exception e)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "PrivateMemorySize64: " + Process.GetCurrentProcess().PrivateMemorySize64 / 1024 /1024 + "MB");
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", "VirtualMemorySize64: " + Process.GetCurrentProcess().VirtualMemorySize64 / 1024 / 1024 + "MB");
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", e.Message);
            if (e.StackTrace != null)
            {
                Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", e.StackTrace);
            }
            if (e.InnerException != null)
            {
                Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", e.InnerException.Message);
                if (e.InnerException.StackTrace != null)
                {
                    Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", e.InnerException.StackTrace);
                }
            }


        }
    }

    public override void OnSurfaceRendered(ulong timestamp, ulong targetTimestamp)
    {
        if (TopLevelImpl == null)
            return;
        base.OnSurfaceRendered(timestamp, targetTimestamp);
        TopLevelImpl.Render();
        if (UseSoftRenderer == true && egl != null)
        {
            egl.SwapBuffers(display, surface);
        }
    }
    private void CreateView(AppBuilder appBuilder)
    {
        if (SingleViewLifetime == null)
            return;
        TopLevelImpl = new TopLevelImpl(XComponentHandle, WindowHandle);
        Root = new EmbeddableControlRoot(TopLevelImpl);
        SingleViewLifetime.Root = Root;
        Root.Prepare();

    }

    public unsafe override void DispatchTouchEvent()
    {
        if (TopLevelImpl == null)
            return;
        if (TopLevelImpl.Input == null)
            return;
        if (TopLevelImpl.InputRoot == null)
            return;
        OH_NativeXComponent_TouchEvent touchEvent = default;
        var result = ace_ndk.OH_NativeXComponent_GetTouchEvent((OH_NativeXComponent*)XComponentHandle, (void*)WindowHandle, &touchEvent);
        if (result == (int)OH_NATIVEXCOMPONENT_RESULT.SUCCESS)
        {
            for (uint i = 0; i < touchEvent.numPoints; i++)
            {
                OH_NativeXComponent_TouchPointToolType toolType = default;
                float tiltX = 0;
                float tiltY = 0;


                ace_ndk.OH_NativeXComponent_GetTouchPointToolType((OH_NativeXComponent*)XComponentHandle, i, &toolType);
                ace_ndk.OH_NativeXComponent_GetTouchPointTiltX((OH_NativeXComponent*)XComponentHandle, i, &tiltX);
                ace_ndk.OH_NativeXComponent_GetTouchPointTiltY((OH_NativeXComponent*)XComponentHandle, i, &tiltY);


                var id = touchEvent.touchPoints[(int)i].id;

                var type = touchEvent.touchPoints[(int)i].type switch
                {
                    OH_NativeXComponent_TouchEventType.OH_NATIVEXCOMPONENT_DOWN => RawPointerEventType.TouchBegin,
                    OH_NativeXComponent_TouchEventType.OH_NATIVEXCOMPONENT_UP => RawPointerEventType.TouchEnd,
                    OH_NativeXComponent_TouchEventType.OH_NATIVEXCOMPONENT_MOVE => RawPointerEventType.TouchUpdate,
                    OH_NativeXComponent_TouchEventType.OH_NATIVEXCOMPONENT_CANCEL => RawPointerEventType.TouchCancel,
                    _ => throw new NotImplementedException()
                };

                var position = new Point(touchEvent.touchPoints[(int)i].x, touchEvent.touchPoints[(int)i].y) / TopLevelImpl.RenderScaling;
                var modifiers = RawInputModifiers.None;
                if (type == RawPointerEventType.TouchUpdate)
                {
                    modifiers |= RawInputModifiers.LeftMouseButton;
                }
                var args = new RawTouchEventArgs(_touchDevice, (ulong)touchEvent.touchPoints[(int)i].timeStamp, TopLevelImpl.InputRoot, type, position, RawInputModifiers.LeftMouseButton, id);
                
                TopLevelImpl.Input?.Invoke(args);
            }
        }
        else
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "csharp", "OH_NativeXComponent_GetTouchEvent fail");
        }
        
    }

    private AppBuilder CreateAppBuilder() => AppBuilder.Configure<TApp>().UseOpenHarmony();

}
