using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using SkiaSharp;

namespace SandBox.Views;

public partial class IMEPage : UserControl
{
    public IMEPage()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        // var fontManager = SKFontManager.Default;
        // var skTypeface = fontManager.MatchCharacter('鸿');
        // ListBox.ItemsSource = fontManager.FontFamilies.Append("其它方法测试：" + skTypeface.FamilyName);
        // var font = new FontFamily(new Uri("/system/fonts/#HarmonyOS Sans SC"), "HarmonyOS Sans SC");;
        // using var fileStream = File.OpenRead("/system/fonts/HarmonyOS_Sans.ttf");
        //

        ListBox.ItemsSource = FontManager.Current.SystemFonts.Select(x => x.Name);
    }
}