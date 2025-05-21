using System;
using System.Collections.Specialized;
using System.Reactive.Linq;

using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.OpenHarmony;
using Avalonia.Threading;

using ReactiveMarbles.ObservableEvents;

namespace AOOH_Gallery.Views;

public partial class MainView : UserControl
{
    public static FuncValueConverter<double, double> GetPaneHeightConverter = new(input => input / 3 * 2);

    public MainView()
    {
        InitializeComponent();
        (OHDebugHelper.Logs as INotifyCollectionChanged).Events().CollectionChanged
            .Do(_ => Dispatcher.UIThread.Post(() => ListBox.ScrollIntoView(OHDebugHelper.Logs.Count - 1)))
            .Subscribe();
    }
}
