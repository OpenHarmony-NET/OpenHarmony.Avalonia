using Avalonia.Controls.Platform.Surfaces;
using Avalonia.Platform;

namespace Avalonia.OpenHarmony;

public class FramebufferManager : IFramebufferPlatformSurface
{
    private readonly TopLevelImpl TopLevelImpl;

    public FramebufferManager(TopLevelImpl topLevelImpl)
    {
        TopLevelImpl = topLevelImpl;
    }

    public IFramebufferRenderTarget CreateFramebufferRenderTarget()
    {
        return new FuncFramebufferRenderTarget(Lock);
    }

    public ILockedFramebuffer Lock()
    {
        return new OpenHarmonyFramebuffer(TopLevelImpl);
    }
}