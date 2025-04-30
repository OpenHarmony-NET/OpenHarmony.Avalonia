using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.OpenHarmony;
using Avalonia.Platform.Storage;

namespace SandBox.Views;

public partial class StorgePage : UserControl
{
    public StorgePage()
    {
        InitializeComponent();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        OHDebugHelper.Debug("Button_OnClick");
        try
        {
            var result = await TopLevel.GetTopLevel(this)?.StorageProvider.OpenFilePickerAsync(
                new()
                {
                    AllowMultiple = CheckBox.IsChecked is true,
                    FileTypeFilter = [FilePickerFileTypes.ImageAll, FilePickerFileTypes.All]
                })!;
            string path = result.FirstOrDefault()?.TryGetLocalPath() ?? "没有任何内容被选取";
            TextBox.Text = path;
            if (path is "没有任何内容被选取")
            {
                return;
            }

            FileInfo di = new(path);
            if (di.Exists)
                TextBox.Text =
                    $"""
                     {di.Name}
                     {di.FullName}
                     {di.CreationTime}
                     {di.LastAccessTime}
                     {di.LastWriteTime}
                     {di.Attributes}
                     """;
        }
        catch (Exception exception)
        {
            OHDebugHelper.Error("Button_OnClick", exception);
        }
    }
//198589
    private async void Button_OnClick2(object? sender, RoutedEventArgs e)
    {
        OHDebugHelper.Debug("Button_OnClick2");
        try
        {
            var result = await TopLevel.GetTopLevel(this)?.StorageProvider.SaveFilePickerAsync(
                new()
                {
                    SuggestedFileName = TextBox_Name.Text,
                    FileTypeChoices = [new FilePickerFileType("test") { Patterns = ["*.json"] }],
                })!;
            string path = result?.TryGetLocalPath() ?? "没有任何内容被选取";
            TextBox.Text = path;
            if (path is "没有任何内容被选取")
            {
                return;
            }

            FileInfo di = new(path);
            if (di.Exists)
                TextBox.Text =
                    $"""
                     {di.Name}
                     {di.FullName}
                     {di.CreationTime}
                     {di.LastAccessTime}
                     {di.LastWriteTime}
                     {di.Attributes}
                     """;
        }
        catch (Exception exception)
        {
            OHDebugHelper.Error("Button_OnClick", exception);
        }
    }

    private async void Button_OnClick3(object? sender, RoutedEventArgs e)
    {
        OHDebugHelper.Debug("Button_OnClick3");
        try
        {
            var result = await TopLevel.GetTopLevel(this)?.StorageProvider.OpenFolderPickerAsync(
                new()
                {
                    AllowMultiple = CheckBox.IsChecked is true,
                })!;
            string path = result.FirstOrDefault()?.TryGetLocalPath() ?? "没有任何内容被选取";
            TextBox.Text = path;
            if (path is "没有任何内容被选取")
            {
                return;
            }

            DirectoryInfo di = new(path);
            if (di.Exists)
                TextBox.Text =
                    $"""
                     {di.Name}
                     {di.FullName}
                     {di.CreationTime}
                     {di.LastAccessTime}
                     {di.LastWriteTime}
                     {di.Parent?.FullName}
                     {di.Attributes}
                     """;
        }
        catch (Exception exception)
        {
            OHDebugHelper.Error("Button_OnClick", exception);
        }
    }
}