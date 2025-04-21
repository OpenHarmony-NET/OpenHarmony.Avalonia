using System.Collections.ObjectModel;

namespace Avalonia.OpenHarmony;

public static class OHDebugHelper
{
    public static ObservableCollection<string> Logs { get; } = new();

    public static void AddLog(string log)
    {
        Logs.Insert(0, log);
    }
}