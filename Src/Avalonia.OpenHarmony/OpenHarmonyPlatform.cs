using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.OpenGL.Egl;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;
using Avalonia.Skia;
using Avalonia.Threading;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyPlatform
{
    public static OpenHarmonyPlatformOptions Options = new OpenHarmonyPlatformOptions()
        { RenderingMode = [OpenHarmonyPlatformRenderingMode.Software] };

    public static void Initialize()
    {
        var options = AvaloniaLocator.Current.GetService<OpenHarmonyPlatformOptions>() ??
                      new OpenHarmonyPlatformOptions();
        AvaloniaLocator.CurrentMutable
            .Bind<PlatformHotkeyConfiguration>().ToSingleton<PlatformHotkeyConfiguration>()
            .Bind<FontManager>().ToConstant(new FontManager(new CustomFontManagerImpl()))
            .Bind<IRuntimePlatform>().ToSingleton<OpenHarmonyRuntimePlatform>()
            .Bind<IRenderTimer>().ToSingleton<OpenHarmonyRenderTimer>()
            .Bind<ICursorFactory>().ToSingleton<CursorFactory>()
            .Bind<IPlatformThreadingInterface>().ToSingleton<OpenHarmonyPlatformThreading>()
            .Bind<IKeyboardDevice>().ToSingleton<OpenHarmonyKeyboardDevice>()
            .Bind<IPlatformSettings>().ToSingleton<OpenHarmonyPlatformSettings>();

        var platformGraphics = InitializeGraphics(options);
        if (platformGraphics is not null)
        {
            AvaloniaLocator.CurrentMutable.Bind<IPlatformGraphics>().ToConstant(platformGraphics);
        }

        var compositor = new Compositor(platformGraphics);
        AvaloniaLocator.CurrentMutable.Bind<Compositor>().ToConstant(compositor);
    }

    private static IPlatformGraphics? InitializeGraphics(OpenHarmonyPlatformOptions options)
    {
        foreach (var renderingMode in options.RenderingMode)
        {
            if (renderingMode == OpenHarmonyPlatformRenderingMode.Egl)
            {
                return EglPlatformGraphics.TryCreate(() =>
                {
                    return new EglDisplay(new EglDisplayCreationOptions
                    {
                        Egl = new EglInterface("libEGL.so"),
                        SupportsMultipleContexts = true,
                        SupportsContextSharing = true
                    });
                });
            }
            else if (renderingMode == OpenHarmonyPlatformRenderingMode.Software)
            {
                return null;
            }
        }

        throw new Exception("no render mode");
    }
}

public sealed class OpenHarmonyPlatformOptions
{
    public IReadOnlyList<OpenHarmonyPlatformRenderingMode> RenderingMode { get; set; } =
        [OpenHarmonyPlatformRenderingMode.Egl, OpenHarmonyPlatformRenderingMode.Software];
}

public enum OpenHarmonyPlatformRenderingMode
{
    Software = 1,

    Egl = 2,
}