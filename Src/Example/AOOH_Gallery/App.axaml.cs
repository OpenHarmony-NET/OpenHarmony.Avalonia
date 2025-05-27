using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOOH_Gallery.ViewModels;
using AOOH_Gallery.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.OpenHarmony;
using OpenHarmony.NDK.Bindings.Native;

namespace AOOH_Gallery;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate int TestDelegate(int a, sbyte b);

public unsafe partial class App : Application
{
    [DllImport("libSystem.Native", EntryPoint = "SystemNative_Calloc", ExactSpelling = true)]
    internal static extern void* Calloc(nuint num, nuint size);

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewViewModel()
            };
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            singleViewPlatform.MainView = new MainView
            {
                DataContext = null
            };

        base.OnFrameworkInitializationCompleted();

        try
        {
            TestDelegate testDelegate = Test;

            static int Test(int a, sbyte b)
            {
                return 0;
            }

            var ptr = Calloc(16, 1);
            var p = Marshal.GetFunctionPointerForDelegate(testDelegate);
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("Marshal.GetFunctionPointerForDelegate failed.", e);
        }
    }

    public static napi_env? _rootArkTSEnv;

    // public static JSValueScope? ArkTSRootScope { get; set; }
    public static napi_value? UIContext { get; private set; }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "setRootArkTSEnv")]
    public static unsafe napi_value SetRootArkTSEnv(napi_env env, napi_callback_info info)
    {
        try
        {
            _rootArkTSEnv = env;
            // var runtime = new NodejsRuntime(NativeLibrary.Load("libace_napi.z.so"));
            // ArkTSRootScope = new JSValueScope(JSValueScopeType.Root, new JSRuntime.napi_env(env.Pointer), runtime);
            // OHDebugHelper.Debug(JSValue.Global["importNet"].TypeOf().ToString());
            // ArkTSRootScope.RuntimeContext.ImportFunction = JSValue.Global["importNet"].CastTo<JSFunction>();
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("链接本App的ArkTS运行时失败。", e);
        }

        return default;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)], EntryPoint = "setUIContext")]
    public static unsafe napi_value SetUIContext(napi_env env, napi_callback_info info)
    {
        try
        {
            ulong argc = 1;
            var args = stackalloc napi_value[(int)argc];
            node_api.napi_get_cb_info(env, info, &argc, args, null, null);
            UIContext = args[0];
        }
        catch (Exception e)
        {
            OHDebugHelper.Error("引入UI上下文时失败。", e);
        }

        return default;
    }
}