using System;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Interactivity;
using Avalonia.Media;
using ReactiveMarbles.ObservableEvents;

namespace AOOH_Gallery.Views;

public partial class IMEPage : UserControl
{
    public IMEPage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var inputPane = TopLevel.GetTopLevel(this)!.InputPane;
        inputPane.Events().StateChanged.Do(_ => { Rectangle.Height = inputPane!.OccludedRect.Height; }).Subscribe();
    }
}