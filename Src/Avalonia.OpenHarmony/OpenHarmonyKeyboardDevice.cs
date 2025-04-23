using Avalonia.Input;

namespace Avalonia.OpenHarmony;

internal class OpenHarmonyKeyboardDevice : KeyboardDevice
{
    internal static KeyboardDevice Instance =>
        (AvaloniaLocator.Current.GetService<IKeyboardDevice>() as KeyboardDevice)!;
}