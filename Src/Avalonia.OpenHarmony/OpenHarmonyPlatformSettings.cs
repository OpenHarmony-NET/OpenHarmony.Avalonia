using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Platform;
using Avalonia.Threading;
using OpenHarmony.NDK.Bindings.Native;

namespace Avalonia.OpenHarmony;

public class OpenHarmonyPlatformSettings : DefaultPlatformSettings
{
    public OpenHarmonyPlatformSettings()
    {
        OHDebugHelper.Debug("OpenHarmonyPlatformSettings 被创建了");
        ColorModeChanged += mode =>
        {
            OHDebugHelper.Debug("OpenHarmonyPlatformSettings" + $"ColorModeChanged: {mode}");
            OnColorValuesChanged(new PlatformColorValues()
            {
                ThemeVariant = mode switch
                {
                    ColorMode.COLOR_MODE_DARK => PlatformThemeVariant.Dark,
                    _ => PlatformThemeVariant.Light
                }
            });
        };
        OnColorValuesChanged(new PlatformColorValues()
        {
            ThemeVariant = _colorMode switch
            {
                ColorMode.COLOR_MODE_DARK => PlatformThemeVariant.Dark,
                _ => PlatformThemeVariant.Light
            }
        });
    }

    private static event Action<ColorMode>? ColorModeChanged;
    private static ColorMode _colorMode;

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "setColor")]
    public static unsafe napi_value SetColor(napi_env env, napi_callback_info info)
    {
        ulong argc = 1;
        var args = stackalloc napi_value[(int)argc];
        node_api.napi_get_cb_info(env, info, &argc, args, null, null);
        int result;
        if (node_api.napi_get_value_int32(env, args[0], &result) is napi_status.napi_ok)
        {
            if (result is >= -1 and <= 1)
            {
                ColorModeChanged?.Invoke(_colorMode = (ColorMode)result);
            }
            else
            {
                ColorModeChanged?.Invoke(_colorMode = ColorMode.COLOR_MODE_NOT_SET);
            }
        }

        return default;
    }
}

internal enum ColorMode
{
    /**
     * The color mode is not set.
     *
     * @syscap SystemCapability.Ability.AbilityBase
     * @since 9
     */
    /**
     * The color mode is not set.
     *
     * @syscap SystemCapability.Ability.AbilityBase
     * @crossplatform
     * @since 10
     */
    /**
     * The color mode is not set.
     *
     * @syscap SystemCapability.Ability.AbilityBase
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    COLOR_MODE_NOT_SET = -1,

    /**
     * Dark mode.
     *
     * @syscap SystemCapability.Ability.AbilityBase
     * @since 9
     */
    /**
     * Dark mode.
     *
     * @syscap SystemCapability.Ability.AbilityBase
     * @crossplatform
     * @since 10
     */
    /**
     * Dark mode.
     *
     * @syscap SystemCapability.Ability.AbilityBase
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    COLOR_MODE_DARK = 0,

    /**
     * Light mode.
     *
     * @syscap SystemCapability.Ability.AbilityBase
     * @since 9
     */
    /**
     * Light mode.
     *
     * @syscap SystemCapability.Ability.AbilityBase
     * @crossplatform
     * @since 10
     */
    /**
     * Light mode.
     *
     * @syscap SystemCapability.Ability.AbilityBase
     * @crossplatform
     * @atomicservice
     * @since 11
     */
    COLOR_MODE_LIGHT = 1
}