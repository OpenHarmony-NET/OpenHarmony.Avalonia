using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AOOH_Gallery.Views;

public partial class SystemFonts : UserControl
{
    public SystemFonts()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ListBox.ItemsSource = FontManager.Current.SystemFonts.Select(x => x.Name);
    }
}
