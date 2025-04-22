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
    }
}