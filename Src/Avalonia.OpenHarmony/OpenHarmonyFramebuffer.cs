using Avalonia.Platform;
using PixelFormat = Avalonia.Platform.PixelFormat;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyFramebuffer : ILockedFramebuffer
{
    private readonly TopLevelImpl TopLevelImpl;

    public OpenHarmonyFramebuffer(TopLevelImpl topLevelImpl)
    {
        TopLevelImpl = topLevelImpl;
        Size = topLevelImpl.Size;
        RowBytes = 4 * Size.Width;
        Format = PixelFormat.Rgba8888;
        Dpi = new Vector(96, 96) * topLevelImpl.Scaling;
    }

    public nint Address => TopLevelImpl.Address;

    public PixelSize Size { get; }

    public int RowBytes { get; }

    public Vector Dpi { get; }

    public PixelFormat Format { get; }

    public void Dispose()
    {
    }
}