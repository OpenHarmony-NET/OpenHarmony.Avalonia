using System;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Markup.Xaml;
using Avalonia.OpenHarmony;
using Avalonia.Threading;
using ReactiveMarbles.ObservableEvents;

namespace SandBox.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        (OHDebugHelper.Logs as INotifyCollectionChanged).Events().CollectionChanged
            .Do(_ => Dispatcher.UIThread.Post(() => ListBox.ScrollIntoView(OHDebugHelper.Logs.Count - 1)))
            .Subscribe();
        var inputPane = TopLevel.GetTopLevel(this)?.InputPane;
        inputPane.Events().StateChanged.Do(_ =>
            {
                Dispatcher.UIThread.Post(() =>
                    TextBox.Text = $"输入法面板状态：{inputPane?.State}\n输入法面板的的位置与宽高{inputPane?.OccludedRect}");
            })
            .Subscribe();
    }
}