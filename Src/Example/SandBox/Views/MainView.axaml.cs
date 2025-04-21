using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Markup.Xaml;
using Avalonia.OpenHarmony;
using ReactiveMarbles.ObservableEvents;

namespace SandBox.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        OHDebugHelper.Logs.Events().CollectionChanged.Do(_ => ListBox.ScrollIntoView(0));
    }
}