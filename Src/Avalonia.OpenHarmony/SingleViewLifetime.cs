using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Embedding;

namespace Avalonia.OpenHarmony;

public class SingleViewLifetime : ISingleViewApplicationLifetime
{
    private Control? _mainView;
    public EmbeddableControlRoot? Root;

    public Control? MainView
    {
        get => _mainView;
        set
        {
            _mainView = value;
            Root!.Content = value;
        }
    }
}