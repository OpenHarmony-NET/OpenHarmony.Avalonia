using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaApp.ViewModels;
using System.Threading.Tasks;

namespace AvaloniaApp.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnShowHomeClick(object sender, PointerPressedEventArgs e)
    {
        var viewmodel = this.DataContext as MainViewModel;
        if (viewmodel == null)
            return;
        viewmodel.ChangeTo(0);

    }

    private void OnShowNewsClick(object sender, PointerPressedEventArgs e)
    {
        var viewmodel = this.DataContext as MainViewModel;
        if (viewmodel == null)
            return;
        viewmodel.ChangeTo(1);

    }

    private void OnShowMessageClick(object sender, PointerPressedEventArgs e)
    {
        var viewmodel = this.DataContext as MainViewModel;
        if (viewmodel == null)
            return;
        viewmodel.ChangeTo(2);

    }

    private void OnShowProfileClick(object sender, PointerPressedEventArgs e)
    {
        var viewmodel = this.DataContext as MainViewModel;
        if (viewmodel == null)
            return;
        viewmodel.ChangeTo(3);
    }
}