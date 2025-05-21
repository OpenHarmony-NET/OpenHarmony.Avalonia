using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaGame.ViewModels;

namespace AvaloniaGame;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        return new TextBlock { Text = "Not Found: " + data.GetType().FullName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}