using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using OpenHarmony.NDK.Bindings.Native;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyPlatformThreading : IPlatformThreadingInterface
{
    Thread LoopThread;
    public OpenHarmonyPlatformThreading()
    {
        LoopThread = Thread.CurrentThread;
    }
    public bool CurrentThreadIsLoopThread => LoopThread == Thread.CurrentThread;

    public event Action<DispatcherPriority?>? Signaled;

    public void Signal(DispatcherPriority priority)
    {
        EnsureInvokeOnMainThread(() => Signaled?.Invoke(null));
    }

    public IDisposable StartTimer(DispatcherPriority priority, TimeSpan interval, Action tick)
    {
        if (interval.TotalMilliseconds < 10)
            interval = TimeSpan.FromMilliseconds(10);

        var stopped = false;
        Timer? timer = null;
        timer = new Timer(_ =>
        {
            if (stopped)
                return;

            EnsureInvokeOnMainThread(() =>
            {
                try
                {
                    tick();
                }
                catch (Exception ex)
                {
                    Hilog.OH_LOG_ERROR(LogType.LOG_APP, "CSharp", ex.Message);
                    Hilog.OH_LOG_ERROR(LogType.LOG_APP, "CSharp", ex.StackTrace);
                    throw ex;
                }
                finally
                {
                    if (!stopped)
                        timer!.Change(interval, Timeout.InfiniteTimeSpan);
                }
            });
        },
        null, interval, Timeout.InfiniteTimeSpan);

        return new Disposable(() =>
        {
            stopped = true;
            timer.Dispose();
        });
    }

    private void EnsureInvokeOnMainThread(Action action)
    {
        lock (list)
        {
            list.Add(action);
        }
    }
    List<Action> list = [];
    List<Action> list2 = [];
    public void Tick()
    {
        lock (list)
        {
            (list2, list) = (list, list2);
        }
        foreach(var action in list2)
        {
            action();
        }
        list2.Clear();
    }
}

class Disposable : IDisposable
{
    private Action Action;
    public Disposable(Action action)
    {
        Action = action;
    }

    public void Dispose()
    {
        Action?.Invoke();
    }
}
