using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.OpenHarmony;
using Avalonia.Platform.Storage;
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