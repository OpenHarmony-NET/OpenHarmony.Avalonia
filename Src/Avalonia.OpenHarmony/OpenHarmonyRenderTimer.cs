using Avalonia.Rendering;
using OpenHarmony.NDK.Bindings.Native;
using System.Diagnostics;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyRenderTimer : IRenderTimer
{
    private static readonly Stopwatch s_sw = Stopwatch.StartNew();
    public bool RunsInBackground => false;

    public event Action<TimeSpan>? Tick;

    public void Render()
    {
        var timespan = s_sw.Elapsed;
        s_sw.Restart();
        Tick?.Invoke(timespan);
    }
}
