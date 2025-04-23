namespace Avalonia.OpenHarmony;

public static class OpenHarmonyApplicationExtensions
{
    public static AppBuilder UseOpenHarmony(this AppBuilder builder)
    {
        return builder
            .UseStandardRuntimePlatformSubsystem()
            .UseWindowingSubsystem(OpenHarmonyPlatform.Initialize, "OpenHarmony")
            .UseSkia();
    }

    public static AppBuilder UseSoftwareRenderer(this AppBuilder builder)
    {
        AvaloniaLocator.CurrentMutable.Bind<OpenHarmonyPlatformOptions>().ToConstant(new OpenHarmonyPlatformOptions
            { RenderingMode = [OpenHarmonyPlatformRenderingMode.Software] });
        return builder;
    }
}