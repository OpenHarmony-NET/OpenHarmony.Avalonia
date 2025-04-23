using Avalonia.Platform;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyRuntimePlatform : StandardRuntimePlatform
{
    public override RuntimePlatformInfo GetRuntimeInfo()
    {
        var isMobile = true;
        var isTv = false;
        var result = new RuntimePlatformInfo
        {
            IsMobile = isMobile && !isTv,
            IsDesktop = !isMobile && !isTv,
            IsTV = isTv
        };

        return result;
    }
}