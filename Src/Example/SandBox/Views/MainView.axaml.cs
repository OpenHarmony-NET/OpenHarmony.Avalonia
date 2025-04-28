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
    static unsafe MainView()
    {
        string str = "tessldkfjslkdfjslkdfjsld";
        var ptr = Marshal.StringToCoTaskMemUTF8(str);
        var l = str.Length;
        var b = Encoding.UTF8.GetBytes(str);
        var length = b.Length;
        List<sbyte> sbytes = new List<sbyte>();
        for (int i = 0; i < length; i++)
        {
            sbytes.Add(((sbyte*)ptr)[i]);
        }

        var buf = Marshal.PtrToStringUTF8(ptr);
        Marshal.ZeroFreeCoTaskMemUTF8(ptr);
    }

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
                new()
                {
                    AllowMultiple = true,
                    FileTypeFilter = [FilePickerFileTypes.ImageAll, FilePickerFileTypes.All]
                })!;
            string path = result.FirstOrDefault()?.TryGetLocalPath() ?? "没有任何内容被选取";
            (sender as Button)!.Content = path;
            if (path is "没有任何内容被选取")
            {
                Image.Source = null;
                return;
            }

            FileInfo di = new(path);
            if (di.Exists)
                (sender as Button)!.Content =
                    $"""
                     {di.Name}
                     {di.FullName}
                     {di.CreationTime}
                     {di.LastAccessTime}
                     {di.LastWriteTime}
                     {di.Attributes}
                     """;
            // var fileStream = File.OpenRead(path);
            // var imageSource = Bitmap.DecodeToHeight(fileStream, 100);
            // Image.Source = imageSource;
            // fileStream.Dispose();
            // File.Delete(path);
        }
        catch (Exception exception)
        {
            OHDebugHelper.Error("Button_OnClick", exception);
        }
    }
}