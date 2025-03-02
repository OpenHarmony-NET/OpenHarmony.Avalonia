using Avalonia.Controls.Embedding;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.OpenGL.Egl;
using OpenHarmony.NDK.Bindings.Native;
using Silk.NET.OpenGLES;
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
        if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
            UseSoftRenderer = true;
        else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
            UseSoftRenderer = false;
        _touchDevice = new TouchDevice();
        _penDevice = new PenDevice();
        _mouseDevice = new MouseDevice();
    }

    public unsafe void InitOpenGlEnv()
    {
        egl = new EglInterface("libEGL.so");

        display = egl.GetDisplay(0);
        if (egl.Initialize(display, out var major, out var minor) == false)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl.Initialize fail");
            return;
        }

        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl init success");

        int[] attributes = [0x3033, 0x0004, 0x3024, 8, 0x3023, 8, 0x3022, 8, 0x3021, 8, 0x3040, 0x0004, 0x3038];
        if (egl.ChooseConfig(display, attributes, out var configs, 1, out var choosenConfig) == false)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl.ChooseConfig fail");
            return;
        }

        int[] winAttribs = [0x309D, 0x3089, 0x3038];
        surface = egl.CreateWindowSurface(display, configs, WindowHandle, winAttribs);
        if (surface == 0)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl.CreateWindowSurface fail");
            return;
        }


        int[] attrib3_list = [0x3098, 2, 0x3038];
        int sharedEglContext = 0;
        var context = egl.CreateContext(display, configs, sharedEglContext, attrib3_list);
        if (egl.MakeCurrent(display, surface, surface, context) == false)
        {
            Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "egl.MakeCurrent fail");
            return;
        }


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

        SingleViewLifetime = new SingleViewLifetime();
        builder.AfterApplicationSetup(CreateView).SetupWithLifetime(SingleViewLifetime);


        Root?.StartRendering();


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

    public unsafe override void OnSurfaceChanged()
    {
        base.OnSurfaceChanged();
        ulong width = 0, height = 0;
        TopLevelImpl.Resize();
        ace_ndk.OH_NativeXComponent_GetXComponentSize((OH_NativeXComponent*)XComponentHandle, (void*)WindowHandle, &width, &height);
        if (UseSoftRenderer == true && gl != null)
        {
            gl.Viewport(0, 0, (uint)width, (uint)height);
        }
    }
    private AppBuilder CreateAppBuilder() => AppBuilder.Configure<TApp>().UseOpenHarmony();

}
