using System;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.OpenHarmony;
using Avalonia.Threading;
using OpenHarmony.NDK.Bindings;
using ReactiveMarbles.ObservableEvents;

namespace AOOH_Gallery.Views;

public partial class MainView : UserControl
{
    public static readonly FuncValueConverter<double, double> GetPaneHeightConverter = new(input => input / 3 * 2);

    public MainView()
    {
        InitializeComponent();
        (OHDebugHelper.Logs as INotifyCollectionChanged).Events().CollectionChanged
            .Do(_ => Dispatcher.UIThread.Post(() => ListBox.ScrollIntoView(OHDebugHelper.Logs.Count - 1)))
            .Subscribe();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            // jsvm.OH_JSVM_GetGlobal(, out var global);
            // var jsDocumentSelectOptions =
            //     App.ArkTSRootScope.RuntimeContext.Import("@kit.CoreFileKit", "DocumentSelectOptions", true);
            // var jsDocumentViewPicker =
            //     App.ArkTSRootScope.RuntimeContext.Import("@kit.CoreFileKit", "DocumentViewPicker", true);
            // var documentSelectOptions = jsDocumentSelectOptions.CallAsConstructor();
            // var documentViewPicker = jsDocumentViewPicker.CallAsConstructor(App.UIContext.Value);
            // documentViewPicker.CallMethod("select", documentSelectOptions);
        }
        catch (Exception exception)
        {
            OHDebugHelper.Error("通过Node-API-Dotnet项目拉起OpenHarmony的文件捡取器时失败。", exception);
        }
    }
}