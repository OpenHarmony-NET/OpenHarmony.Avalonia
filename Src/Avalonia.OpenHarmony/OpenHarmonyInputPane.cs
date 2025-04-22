using Avalonia.Controls.Platform;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyInputPane : InputPaneBase
{
    private readonly TopLevelImpl _topLevelImpl;

    public OpenHarmonyInputPane(TopLevelImpl topLevelImpl)
    {
        _topLevelImpl = topLevelImpl;
    }

    public bool OnGeometryChange(double y, double height)
    {
        var oldState = (OccludedRect, State);

        OccludedRect = new Rect(0, y, _topLevelImpl.ClientSize.Width, height);
        State = OccludedRect.Height != 0 ? InputPaneState.Open : InputPaneState.Closed;

        if (oldState != (OccludedRect, State))
        {
            OnStateChanged(new InputPaneStateEventArgs(State, null, OccludedRect));
        }

        return true;
    }
}