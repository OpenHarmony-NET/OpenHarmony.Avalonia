using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;

using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Input.Raw;
using Avalonia.Input.TextInput;
using Avalonia.OpenGL.Egl;
using Avalonia.OpenGL.Surfaces;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;

using OpenHarmony.NDK.Bindings.Native;

using Silk.NET.OpenGLES;

namespace Avalonia.OpenHarmony;

public class TopLevelImpl : ITopLevelImpl, EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo
{
    public readonly ClipboardImpl _clipboard = new();

    private readonly OpenHarmonyInputPane _openHarmonyInputPane;

    private readonly OpenHarmonyInputMethod _textInputMethod;

    public IGlPlatformSurface _gl;

    private WindowTransparencyLevel _transparencyLevel;

    public nint Address;
    public uint ebo;

    public List<OpenHarmonyFramebuffer> framebuffers = [];

    public GL? gl;
    public uint programId;

    public uint textureId;

    public uint vao;
    public uint vbo;
    private readonly OpenHarmonyStorageProvider _openHarmonyStorageProvider;

    public unsafe TopLevelImpl(IntPtr xcomponent, IntPtr window)
    {
        Window = window;
        XComponent = xcomponent;
        ulong width = 0, height = 0;
        Ace.OH_NativeXComponent_GetXComponentSize((OH_NativeXComponent*)xcomponent, (void*)window, &width, &height);
        float density = 1;
        display_manager.OH_NativeDisplayManager_GetDefaultDisplayScaledDensity(&density);
        Size = new PixelSize((int)width, (int)height);
        Scaling = density;
        _gl = new EglGlPlatformSurface(this);
        Surfaces = [_gl, new FramebufferManager(this)];
        gl = AvaloniaLocator.Current.GetService<GL>();
        if (gl != null)
        {
            Init();
            InitShader();
            InitOrUpdateTexture();
        }

        RenderTimer = AvaloniaLocator.Current.GetService<IRenderTimer>() as OpenHarmonyRenderTimer;
        OpenHarmonyPlatformThreading =
            AvaloniaLocator.Current.GetService<IPlatformThreadingInterface>() as OpenHarmonyPlatformThreading;
        _textInputMethod = new OpenHarmonyInputMethod(this);
        _openHarmonyInputPane = new OpenHarmonyInputPane(this);
        _openHarmonyStorageProvider = new OpenHarmonyStorageProvider();
    }

    public IntPtr Window { get; }
    public IntPtr XComponent { get; }

    public OpenHarmonyRenderTimer? RenderTimer { get; }

    public OpenHarmonyPlatformThreading? OpenHarmonyPlatformThreading { get; }

    public Size? FrameSize => null;


    public IInputRoot? InputRoot { get; private set; }
    public nint Handle => Window;

    public PixelSize Size { get; private set; }

    public double Scaling { get; private set; }


    public Size ClientSize => new Size(Size.Width, Size.Height) / Scaling;

    public double RenderScaling => Scaling;

    public IEnumerable<object> Surfaces { get; }

    public Action<RawInputEventArgs>? Input { get; set; }
    public Action<Rect>? Paint { get; set; }
    public Action<Size, WindowResizeReason>? Resized { get; set; }
    public Action<double>? ScalingChanged { get; set; }
    public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get; set; }

    public Compositor Compositor => AvaloniaLocator.CurrentMutable.GetService<Compositor>() ??
                                    throw new InvalidOperationException(
                                        "Android backend wasn't initialized. Make sure .UseAndroid() was executed.");

    public Action? Closed { get; set; }
    public Action? LostFocus { get; set; }

    public WindowTransparencyLevel TransparencyLevel
    {
        get => _transparencyLevel;
        private set
        {
            if (_transparencyLevel != value)
            {
                _transparencyLevel = value;
                TransparencyLevelChanged?.Invoke(value);
            }
        }
    }

    public AcrylicPlatformCompensationLevels AcrylicCompensationLevels => new(1, 1, 1);

    public double DesktopScaling => throw new NotImplementedException();

    IPlatformHandle? ITopLevelImpl.Handle => throw new NotImplementedException();

    public void SetInputRoot(IInputRoot inputRoot)
    {
        InputRoot = inputRoot;
    }

    public Point PointToClient(PixelPoint point)
    {
        return point.ToPoint(RenderScaling);
    }

    public PixelPoint PointToScreen(Point point)
    {
        return PixelPoint.FromPoint(point, RenderScaling);
    }

    public void SetCursor(ICursorImpl? cursor)
    {
    }

    public IPopupImpl? CreatePopup()
    {
        return null;
    }

    public void SetTransparencyLevelHint(IReadOnlyList<WindowTransparencyLevel> transparencyLevels)
    {
        // todo
    }

    public void SetFrameThemeVariant(PlatformThemeVariant themeVariant)
    {
        // todo
    }

    public object? TryGetFeature(Type featureType)
    {
        if (featureType == typeof(ITextInputMethodImpl)) return _textInputMethod;

        if (featureType == typeof(IClipboard)) return _clipboard;

        if (featureType == typeof(IInputPane)) return _openHarmonyInputPane;

        if (featureType == typeof(IStorageProvider)) return _openHarmonyStorageProvider;

        // todo
        return null;
    }

    public void Dispose()
    {
    }

    public unsafe void Render()
    {
        try
        {
            RenderTimer?.Render();
            Paint?.Invoke(new Rect(0, 0, Size.Width, Size.Height));
            OpenHarmonyPlatformThreading?.Tick();
        }
        catch (Exception e)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "csharp", e.Message);
            if (e.StackTrace != null) Hilog.OH_LOG_ERROR(LogType.LOG_APP, "csharp", e.StackTrace);

            if (e.InnerException != null)
            {
                Hilog.OH_LOG_ERROR(LogType.LOG_APP, "csharp", e.InnerException.Message);
                if (e.InnerException.StackTrace != null)
                    Hilog.OH_LOG_ERROR(LogType.LOG_APP, "csharp", e.InnerException.StackTrace);
            }

            throw;
        }

        // software render
        if (gl != null)
        {
            InitOrUpdateTexture();

            gl.ClearColor(Color.White);
            gl.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            gl.UseProgram(programId);
            var location = gl.GetUniformLocation(programId, "Texture_Buffer");
            gl.Uniform1(location, 1);
            gl.ActiveTexture(GLEnum.Texture1);
            gl.BindTexture(GLEnum.Texture2D, textureId);
            gl.BindVertexArray(vao);
            gl.DrawElements(GLEnum.Triangles, 6, GLEnum.UnsignedInt, (void*)0);
        }
    }

    public unsafe void InitOrUpdateTexture()
    {
        if (gl == null)
            return;
        if (Address == 0)
        {
            Address = Marshal.AllocHGlobal(sizeof(byte) * 4 * Size.Width * Size.Height);
            var span = new Span<byte>((void*)Address, 4 * Size.Width * Size.Height);
            for (var i = 0; i < span.Length; i++)
                span[i] = 255;
        }

        if (textureId == 0) textureId = gl.GenTexture();

        gl.BindTexture(GLEnum.Texture2D, textureId);
        gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba8, (uint)Size.Width, (uint)Size.Height, 0, GLEnum.Rgba,
            GLEnum.UnsignedByte, (void*)Address);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);
    }

    public unsafe void Resize()
    {
        ulong width = 0, height = 0;
        Ace.OH_NativeXComponent_GetXComponentSize((OH_NativeXComponent*)XComponent, (void*)Window, &width, &height);
        float density = 1;
        display_manager.OH_NativeDisplayManager_GetDefaultDisplayScaledDensity(&density);
        Size = new PixelSize((int)width, (int)height);
        Scaling = density;
        if (gl != null) InitOrUpdateTexture();

        Resized(Size.ToSize(Scaling), WindowResizeReason.User);
    }

    public void InitShader()
    {
        if (gl == null)
            return;
        var VertShaderSource = @"#version 300 es
precision highp float;

layout (location = 0) in vec3 Position;
layout (location = 1) in vec2 TexCoord;


out vec2 texCoord;

void main()
{
    texCoord = TexCoord;
    gl_Position = vec4(Position, 1.0f);
}
";
        var FragShaderSource = @"#version 300 es
precision highp float;
layout (location = 0) out vec4 Color;


uniform sampler2D Texture_Buffer;
in vec2 texCoord;
void main()
{

    vec4 color = texture(Texture_Buffer, texCoord);
    float gamma = 2.2;
    Color = vec4(pow(color.xyz, vec3(gamma)), 1.0f);
}
";

        var vert = gl.CreateShader(GLEnum.VertexShader);
        gl.ShaderSource(vert, VertShaderSource);
        gl.CompileShader(vert);
        gl.GetShader(vert, GLEnum.CompileStatus, out var code);
        if (code == 0)
        {
            var info = gl.GetShaderInfoLog(vert);
            Console.WriteLine(VertShaderSource);
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "vs shader", info);
            throw new Exception(info);
        }

        var frag = gl.CreateShader(GLEnum.FragmentShader);
        gl.ShaderSource(frag, FragShaderSource);
        gl.CompileShader(frag);
        gl.GetShader(frag, GLEnum.CompileStatus, out code);
        if (code == 0)
        {
            gl.DeleteShader(vert);
            var info = gl.GetShaderInfoLog(frag);
            Console.WriteLine(FragShaderSource);
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "fs shader", info);
            throw new Exception(info);
        }

        programId = gl.CreateProgram();
        gl.AttachShader(programId, vert);
        gl.AttachShader(programId, frag);
        gl.LinkProgram(programId);
        gl.GetProgram(programId, GLEnum.LinkStatus, out code);
        if (code == 0)
        {
            gl.DeleteShader(vert);
            gl.DeleteShader(frag);

            var info = gl.GetProgramInfoLog(programId);
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "program shader", info);
            throw new Exception(info);
        }

        gl.DeleteShader(vert);
        gl.DeleteShader(frag);
    }

    public unsafe void Init()
    {
        vao = gl.GenVertexArray();
        float[] vertices =
        [
            -1, 1, 0, 0, 0,
            -1, -1, 0, 0, 1,
            1, -1, 0, 1, 1,
            1, 1, 0, 1, 0
        ];
        uint[] indices =
        [
            0, 1, 2, 2, 3, 0
        ];
        vao = gl.GenVertexArray();
        vbo = gl.GenBuffer();
        ebo = gl.GenBuffer();
        gl.BindVertexArray(vao);
        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        fixed (float* p = vertices)
        {
            gl.BufferData(GLEnum.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), p, GLEnum.StaticDraw);
        }

        gl.BindBuffer(GLEnum.ElementArrayBuffer, ebo);
        fixed (uint* p = indices)
        {
            gl.BufferData(GLEnum.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), p, GLEnum.StaticDraw);
        }

        // Location
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)sizeof(float) * 5, (void*)0);
        // TexCoord
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(1, 2, GLEnum.Float, false, (uint)sizeof(float) * 5, (void*)sizeof(Vector3));
        gl.BindVertexArray(0);
    }

    internal void TextInput(string text)
    {
        if (Input != null)
        {
            var args = new RawTextInputEventArgs(
                OpenHarmonyKeyboardDevice.Instance,
                (ulong)DateTime.Now.Ticks,
                InputRoot!, text);

            Input(args);
        }
    }
}
