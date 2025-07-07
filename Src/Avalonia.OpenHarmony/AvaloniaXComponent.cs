using System.Runtime.InteropServices;

using Avalonia.Controls.Embedding;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.OpenGL.Egl;

using OpenHarmony.NDK.Bindings.Native;

using Silk.NET.OpenGLES;

namespace Avalonia.OpenHarmony;

/// <summary>
/// 提供一个将 Avalonia 应用程序嵌入到 OpenHarmony XComponent 的实现。
/// 此类处理 OpenGL ES 环境初始化、Avalonia 视图的创建、生命周期管理以及输入事件的分发。
/// </summary>
/// <typeparam name="TApp">要运行的 Avalonia 应用程序类型，必须继承自 <see cref="Application"/> 并有一个无参构造函数。</typeparam>
public class AvaloniaXComponent<TApp> : XComponent where TApp : Application, new()
{
    // 输入设备
    private readonly MouseDevice _mouseDevice;
    private readonly PenDevice _penDevice;
    private readonly TouchDevice _touchDevice;

    // EGL 和 OpenGL ES 相关字段
    private nint _display;
    private EglInterface? _egl;
    private GL? _gl;
    private nint _surface;

    /// <summary>
    /// Avalonia 控件的根容器。
    /// </summary>
    public EmbeddableControlRoot? Root;

    /// <summary>
    /// 管理应用程序的生命周期，适用于单视图场景。
    /// </summary>
    public SingleViewLifetime? SingleViewLifetime;

    /// <summary>
    /// Avalonia 的顶层实现，处理渲染和输入。
    /// </summary>
    public TopLevelImpl? TopLevelImpl;

    /// <summary>
    /// 一个标志，用于指示是否应使用软件渲染器。
    /// 注意：此标志的当前实现逻辑可能与名称不完全匹配，因为它会触发 OpenGL 环境的初始化。
    /// </summary>
    public bool UseSoftRenderer = false;

    /// <summary>
    /// 初始化 <see cref="AvaloniaXComponent{TApp}"/> 类的新实例。
    /// </summary>
    /// <param name="xComponentHandle">原生 XComponent 的句柄。</param>
    /// <param name="windowHandle">窗口句柄。</param>
    public AvaloniaXComponent(nint xComponentHandle, nint windowHandle) : base(xComponentHandle, windowHandle)
    {
        _touchDevice = new TouchDevice();
        _penDevice = new PenDevice();
        _mouseDevice = new MouseDevice();
    }

    /// <summary>
    /// 初始化 OpenGL ES 环境。此方法设置 EGL 显示、配置、Surface和上下文。
    /// </summary>
    public void InitOpenGlEnv()
    {
        _egl = new EglInterface("libEGL.so");

        _display = _egl.GetDisplay(0);
        if (_egl.Initialize(_display, out _, out _) == false)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "CSharp", "_egl.Initialize fail");
            return;
        }

        Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "CSharp", "_egl init success");

        // EGL 配置属性，请求 RGBA8888 Surface和 OpenGL ES 2 兼容上下文
        int[] attributes = [0x3033, 0x0004, 0x3024, 8, 0x3023, 8, 0x3022, 8, 0x3021, 8, 0x3040, 0x0004, 0x3038];
        if (_egl.ChooseConfig(_display, attributes, out var configs, 1, out _) == false)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "CSharp", "_egl.ChooseConfig fail");
            return;
        }

        // 窗口Surface属性
        int[] winAttribs = [0x309D, 0x3089, 0x3038];
        _surface = _egl.CreateWindowSurface(_display, configs, WindowHandle, winAttribs);
        if (_surface == 0)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "CSharp", "_egl.CreateWindowSurface fail");
            return;
        }

        // EGL 上下文属性，指定 OpenGL ES 2.0
        int[] attrib3_list = [0x3098, 2, 0x3038];
        var sharedEglContext = 0;
        var context = _egl.CreateContext(_display, configs, sharedEglContext, attrib3_list);
        if (_egl.MakeCurrent(_display, _surface, _surface, context) == false)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "CSharp", "_egl.MakeCurrent fail");
            return;
        }

        // 获取 OpenGL ES 函数指针
        _gl = GL.GetApi(name =>
        {
            var ptr = Marshal.StringToHGlobalAnsi(name);
            var fun = _egl.GetProcAddress(ptr);
            Marshal.FreeHGlobal(ptr);
            return fun;
        });
    }

    /// <summary>
    /// 在 XComponent Surface创建时调用。
    /// 此方法初始化渲染环境（如果需要），配置并启动 Avalonia 应用程序。
    /// </summary>
    public override void OnSurfaceCreated()
    {
        if (UseSoftRenderer)
        {
            InitOpenGlEnv();
        }

        var builder = CreateAppBuilder();
        if (UseSoftRenderer)
        {
            builder.UseSoftwareRenderer();
            if (_gl != null)
            {
                AvaloniaLocator.CurrentMutable.Bind<GL>().ToConstant(_gl);
            }
        }

        SingleViewLifetime = new SingleViewLifetime();
        builder.AfterApplicationSetup(CreateView).SetupWithLifetime(SingleViewLifetime);

        Root?.StartRendering();
    }

    /// <summary>
    /// 在 XComponent Surface上渲染一帧时调用。
    /// 此方法触发 Avalonia 的渲染逻辑，并在使用硬件渲染时交换缓冲区。
    /// </summary>
    /// <param name="timestamp">当前帧的时间戳。</param>
    /// <param name="targetTimestamp">目标帧的时间戳。</param>
    public override void OnSurfaceRendered(ulong timestamp, ulong targetTimestamp)
    {
        if (TopLevelImpl == null)
        {
            return;
        }

        base.OnSurfaceRendered(timestamp, targetTimestamp);
        TopLevelImpl.Render();
        if (UseSoftRenderer && _egl != null)
        {
            _egl.SwapBuffers(_display, _surface);
        }
    }

    /// <summary>
    /// 创建 Avalonia 视图和相关的顶层实现。
    /// </summary>
    /// <param name="appBuilder">应用程序构建器。</param>
    private void CreateView(AppBuilder appBuilder)
    {
        if (SingleViewLifetime == null)
        {
            return;
        }

        TopLevelImpl = new TopLevelImpl(XComponentHandle, WindowHandle);
        Root = new EmbeddableControlRoot(TopLevelImpl);
        SingleViewLifetime.Root = Root;
        Root.Prepare();
    }

    /// <summary>
    /// 分发触摸事件。从原生 XComponent 获取触摸事件数据，并将其转换为 Avalonia 的输入事件。
    /// </summary>
    public override unsafe void DispatchTouchEvent()
    {
        if (TopLevelImpl == null || TopLevelImpl.Input == null || TopLevelImpl.InputRoot == null)
        {
            return;
        }

        OH_NativeXComponent_TouchEvent touchEvent = default;
        var result = Ace.OH_NativeXComponent_GetTouchEvent((OH_NativeXComponent*)XComponentHandle, (void*)WindowHandle,
            &touchEvent);
        if (result == (int)OH_NATIVEXCOMPONENT_RESULT.SUCCESS)
        {
            for (uint i = 0; i < touchEvent.numPoints; i++)
            {
                // 注意：toolType, tiltX, tiltY 当前未被使用。如果将来需要，可以取消注释或实现相关逻辑。
                // OH_NativeXComponent_TouchPointToolType toolType = default;
                // float tiltX = 0;
                // float tiltY = 0;
                // _ = Ace.OH_NativeXComponent_GetTouchPointToolType((OH_NativeXComponent*)xComponentHandle, i, &toolType);
                // _ = Ace.OH_NativeXComponent_GetTouchPointTiltX((OH_NativeXComponent*)xComponentHandle, i, &tiltX);
                // _ = Ace.OH_NativeXComponent_GetTouchPointTiltY((OH_NativeXComponent*)xComponentHandle, i, &tiltY);

                var id = touchEvent.touchPoints[(int)i].id;
                var type = touchEvent.touchPoints[(int)i].type switch
                {
                    OH_NativeXComponent_TouchEventType.OH_NATIVEXCOMPONENT_DOWN => RawPointerEventType.TouchBegin,
                    OH_NativeXComponent_TouchEventType.OH_NATIVEXCOMPONENT_UP => RawPointerEventType.TouchEnd,
                    OH_NativeXComponent_TouchEventType.OH_NATIVEXCOMPONENT_MOVE => RawPointerEventType.TouchUpdate,
                    OH_NativeXComponent_TouchEventType.OH_NATIVEXCOMPONENT_CANCEL => RawPointerEventType.TouchCancel,
                    _ => throw new NotImplementedException()
                };
                var position = new Point(touchEvent.touchPoints[(int)i].x, touchEvent.touchPoints[(int)i].y) /
                               TopLevelImpl.RenderScaling;
                var modifiers = RawInputModifiers.None;
                if (type == RawPointerEventType.TouchUpdate)
                {
                    modifiers |= RawInputModifiers.LeftMouseButton;
                }

                var args = new RawTouchEventArgs(_touchDevice, (ulong)touchEvent.touchPoints[(int)i].timeStamp,
                    TopLevelImpl.InputRoot, type, position, RawInputModifiers.LeftMouseButton, id);

                TopLevelImpl.Input?.Invoke(args);
            }
        }
        else
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "csharp", $"OH_NativeXComponent_GetTouchEvent fail, result={result}");
        }
    }

    /// <summary>
    /// 分发鼠标事件。从原生 XComponent 获取鼠标事件数据，并将其转换为 Avalonia 的 RawPointerEventArgs，
    /// 然后传递给顶层输入处理器。
    /// </summary>
    public override unsafe void DispatchMouseEvent()
    {
        if (TopLevelImpl == null || TopLevelImpl.Input == null || TopLevelImpl.InputRoot == null)
        {
            return;
        }

        OH_NativeXComponent_MouseEvent mouseEvent;
        var result = Ace.OH_NativeXComponent_GetMouseEvent((OH_NativeXComponent*)XComponentHandle, (void*)WindowHandle, &mouseEvent);

        if (result == (int)OH_NATIVEXCOMPONENT_RESULT.SUCCESS)
        {
            var type = mouseEvent.action switch
            {
                OH_NativeXComponent_MouseEventAction.OH_NATIVEXCOMPONENT_MOUSE_MOVE => RawPointerEventType.Move,
                OH_NativeXComponent_MouseEventAction.OH_NATIVEXCOMPONENT_MOUSE_PRESS => mouseEvent.button == OH_NativeXComponent_MouseEventButton.OH_NATIVEXCOMPONENT_LEFT_BUTTON ? RawPointerEventType.LeftButtonDown : RawPointerEventType.RightButtonDown,
                OH_NativeXComponent_MouseEventAction.OH_NATIVEXCOMPONENT_MOUSE_RELEASE => mouseEvent.button == OH_NativeXComponent_MouseEventButton.OH_NATIVEXCOMPONENT_LEFT_BUTTON ? RawPointerEventType.LeftButtonUp : RawPointerEventType.RightButtonUp,
                _ => throw new NotImplementedException($"Mouse event action {mouseEvent.action} is not implemented")
            };

            var position = new Point(mouseEvent.x, mouseEvent.y) / TopLevelImpl.RenderScaling;
            var modifiers = mouseEvent.button == OH_NativeXComponent_MouseEventButton.OH_NATIVEXCOMPONENT_LEFT_BUTTON ? RawInputModifiers.LeftMouseButton : RawInputModifiers.RightMouseButton;

            var args = new RawPointerEventArgs(_mouseDevice, (ulong)mouseEvent.timestamp, TopLevelImpl.InputRoot, type, position, modifiers);

            TopLevelImpl.Input?.Invoke(args);
        }
        else
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "csharp", $"OH_NativeXComponent_GetMouseEvent fail, result={result}");
        }
    }

    /// <summary>
    /// 分发悬停事件。
    /// </summary>
    /// <param name="isHover">指示是否处于悬停状态。</param>
    public override unsafe void DispatchHoverEvent(bool isHover)
    {
        // TODO 
        // Hilog.OH_LOG_DEBUG(LogType.LOG_APP, "csharp", $"DispatchHoverEvent isHover:{isHover}");
    }

    /// <summary>
    /// 在 XComponent Surface尺寸或属性发生变化时调用。
    /// 此方法会调整顶层视图的大小，并更新 OpenGL 视口。
    /// </summary>
    public override unsafe void OnSurfaceChanged()
    {
        base.OnSurfaceChanged();
        ulong width = 0, height = 0;
        TopLevelImpl?.Resize();
        _ = Ace.OH_NativeXComponent_GetXComponentSize((OH_NativeXComponent*)XComponentHandle, (void*)WindowHandle, &width,
            &height);
        if (UseSoftRenderer && _gl != null)
        {
            _gl.Viewport(0, 0, (uint)width, (uint)height);
        }
    }

    /// <summary>
    /// 创建并配置 Avalonia 应用程序构建器。
    /// 派生类可以重写此方法以提供自定义的应用程序配置。
    /// </summary>
    /// <returns>一个配置好的 <see cref="AppBuilder"/> 实例。</returns>
    protected virtual AppBuilder CreateAppBuilder()
    {
        return AppBuilder.Configure<TApp>().UseOpenHarmony();
    }
}
