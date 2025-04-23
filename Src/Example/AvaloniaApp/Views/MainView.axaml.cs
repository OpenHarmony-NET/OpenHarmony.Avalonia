using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using AvaloniaApp.ViewModels;

namespace AvaloniaApp.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnShowHomeClick(object sender, PointerPressedEventArgs e)
    {
        var viewmodel = DataContext as MainViewModel;
        if (viewmodel == null)
            return;
        viewmodel.ChangeTo(0);
    }

    private void OnShowNewsClick(object sender, PointerPressedEventArgs e)
    {
        var viewmodel = DataContext as MainViewModel;
        if (viewmodel == null)
            return;
        viewmodel.ChangeTo(1);
    }

    private void OnShowMessageClick(object sender, PointerPressedEventArgs e)
    {
        var viewmodel = DataContext as MainViewModel;
        if (viewmodel == null)
            return;
        viewmodel.ChangeTo(2);
    }

    private void OnShowProfileClick(object sender, PointerPressedEventArgs e)
    {
        var viewmodel = DataContext as MainViewModel;
        if (viewmodel == null)
            return;
        viewmodel.ChangeTo(3);
    }

    private void InputElement_OnTextInput(object? sender, TextInputEventArgs e)
    {
    }

    private void InputElement_OnTextInputMethodClientRequested(object? sender,
        TextInputMethodClientRequestedEventArgs e)
    {
    }
}