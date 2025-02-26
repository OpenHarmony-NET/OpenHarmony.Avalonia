using Avalonia.Platform;
using OpenHarmony.NDK.Bindings.Native;
using Silk.NET.OpenGLES;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using PixelFormat = Avalonia.Platform.PixelFormat;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyFramebuffer : ILockedFramebuffer
{
    TopLevelImpl TopLevelImpl;
    public OpenHarmonyFramebuffer(TopLevelImpl topLevelImpl)
    {
        TopLevelImpl = topLevelImpl;
        Size = topLevelImpl.Size;
        RowBytes = 4 * Size.Width;
        Format = PixelFormat.Rgba8888;
        Dpi = new Vector(96, 96) * topLevelImpl.Scaling;

    }

    public nint Address => TopLevelImpl.Address;

    public PixelSize Size { get; private set; }

    public int RowBytes { get; private set; }

    public Vector Dpi { get; private set; }

    public PixelFormat Format { get; private set; }

    public void Dispose()
    {
    }
}
