using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        // try
        // {
        //     var inputPane = TopLevel.GetTopLevel(this)?.InputPane;
        //     inputPane.Events().StateChanged.Do(_ =>
        //         {
        //             Dispatcher.UIThread.Post(() =>
        //                 TextBox.Text = $"输入法面板状态：{inputPane?.State}\n输入法面板的的位置与宽高{inputPane?.OccludedRect}");
        //         })
        //         .Subscribe();
        // }
        // catch (Exception exception)
        // {
        //     OHDebugHelper.Error("获取输入法面板信息失败", exception);
        // }
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var result = await TopLevel.GetTopLevel(this)?.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions()
                {
                })!;
            string path = result.FirstOrDefault()?.TryGetLocalPath() ?? "没有任何内容被选取";
            (sender as Button)!.Content = path;
            if (path is "没有任何内容被选取")
            {
                Image.Source = null;
                return;
            }

            await using var fileStream = File.OpenRead(path);
            var imageSource = Bitmap.DecodeToHeight(fileStream, 100);
            Image.Source = imageSource;
        }
        catch (Exception exception)
        {
            OHDebugHelper.Error("Button_OnClick", exception);
        }
    }
}