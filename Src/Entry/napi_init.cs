using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Avalonia.OpenHarmony;

using OpenHarmony.NDK.Bindings.Native;

namespace Entry;

public class napi_init
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "RegisterEntryModule")]
    public static unsafe void RegisterEntryModule()
    {
        try
        {
            const string moduleName = "entry";
            var moduleNamePtr = Marshal.StringToHGlobalAnsi(moduleName);
            var demoModule = new napi_module
            {
                nm_version = 1,
                nm_flags = 0,
                nm_filename = null,
                nm_modname = (sbyte*)moduleNamePtr,
                nm_priv = null,
                napi_addon_register_func = &Init,
                reserved_0 = null,
                reserved_1 = null,
                reserved_2 = null,
                reserved_3 = null
            };

            node_api.napi_module_register(&demoModule);
        }
        catch (Exception e)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "csharp", e.Message);
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "csharp", e.StackTrace);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe napi_value Init(napi_env env, napi_value exports)
    {
        napi_value exportInstance = default;
        OH_NativeXComponent* nativeXComponent = null;
        int ret = default;
        var xcomponentName = "__NATIVE_XCOMPONENT_OBJ__";
        var xcomponentNamePtr = Marshal.StringToHGlobalAnsi(xcomponentName);
        if (node_api.napi_get_named_property(env, exports, (sbyte*)xcomponentNamePtr, &exportInstance) ==
            napi_status.napi_ok)
        {
            if (node_api.napi_unwrap(env, exportInstance, (void**)&nativeXComponent) == napi_status.napi_ok)
            {
                var p = Marshal.AllocHGlobal(sizeof(OH_NativeXComponent_Callback));
                ref var g_ComponentCallback = ref Unsafe.AsRef<OH_NativeXComponent_Callback>((void*)p);
                g_ComponentCallback.OnSurfaceCreated = &XComponentEntry.OnSurfaceCreated;
                g_ComponentCallback.OnSurfaceChanged = &XComponentEntry.OnSurfaceChanged;
                g_ComponentCallback.OnSurfaceDestroyed = &XComponentEntry.OnSurfaceDestroyed;
                g_ComponentCallback.DispatchTouchEvent = &XComponentEntry.DispatchTouchEvent;
                Ace.OH_NativeXComponent_RegisterCallback(nativeXComponent, (OH_NativeXComponent_Callback*)p);
            }
        }

        Marshal.FreeHGlobal(xcomponentNamePtr);
        try
        {
            const int methodNamesLength = 5;
            var desc = stackalloc napi_property_descriptor[methodNamesLength]
            {
                Create((sbyte*)Marshal.StringToHGlobalAnsi("setStartDocumentViewPicker"),
                    &OpenHarmonyStorageProvider.SetStartDocumentViewPicker),
                Create((sbyte*)Marshal.StringToHGlobalAnsi("setStartDocumentViewPickerSaveMode"),
                    &OpenHarmonyStorageProvider.SetStartDocumentViewPickerSaveMode),
                Create((sbyte*)Marshal.StringToHGlobalAnsi("setPickerResult"),
                    &OpenHarmonyStorageProvider.SetPickerResult),
                Create((sbyte*)Marshal.StringToHGlobalAnsi("setColor"),
                    &OpenHarmonyPlatformSettings.SetColor),
                Create((sbyte*)Marshal.StringToHGlobalAnsi("setInputPaneHeight"),
                    &OpenHarmonyInputPane.SetInputPaneHeight),
            };

            napi_property_descriptor Create(sbyte* methodName,
                delegate* unmanaged[Cdecl]<napi_env, napi_callback_info, napi_value> method)
            {
                return new()
                {
                    utf8name = methodName,
                    name = default,
                    method = method,
                    getter = null,
                    setter = null,
                    value = default,
                    attributes = napi_property_attributes.napi_default,
                    data = null
                };
            }

            node_api.napi_define_properties(env, exports, methodNamesLength, desc);
            for (int i = 0; i < methodNamesLength; i++)
                Marshal.FreeHGlobal((IntPtr)desc[i].utf8name);
        }
        catch (Exception e)
        {
            Hilog.OH_LOG_ERROR(LogType.LOG_APP, "testTag", $"{e}");
        }

        return exports;
    }
}
