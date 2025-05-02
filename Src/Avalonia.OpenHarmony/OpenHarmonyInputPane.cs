using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Controls.Platform;
using OpenHarmony.NDK.Bindings.Native;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyInputPane : InputPaneBase
{
    private readonly TopLevelImpl _topLevelImpl;

    public OpenHarmonyInputPane(TopLevelImpl topLevelImpl)
    {
        _topLevelImpl = topLevelImpl;
        InputPaneHeightChanged += data => { OnGeometryChange(0, data); };
        OnGeometryChange(0, _inputPaneHeight);
    }

    public bool OnGeometryChange(double y, double height)
    {
        var oldState = (OccludedRect, State);

        OccludedRect = new Rect(0, y, _topLevelImpl.ClientSize.Width, height);
        State = OccludedRect.Height != 0 ? InputPaneState.Open : InputPaneState.Closed;

        if (oldState != (OccludedRect, State)) OnStateChanged(new InputPaneStateEventArgs(State, null, OccludedRect));
        OHDebugHelper.Debug("输入法的软键盘高度：" + height);
        return true;
    }

    private static int _inputPaneHeight;
    private static event Action<int>? InputPaneHeightChanged;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "setInputPaneHeight")]
    public static unsafe napi_value SetInputPaneHeight(napi_env env, napi_callback_info info)
    {
        ulong argc = 1;
        var args = stackalloc napi_value[(int)argc];
        node_api.napi_get_cb_info(env, info, &argc, args, null, null);
        int result;
        if (node_api.napi_get_value_int32(env, args[0], &result) is napi_status.napi_ok)
        {
            InputPaneHeightChanged?.Invoke(_inputPaneHeight = result);
        }

        return default;
    }
}