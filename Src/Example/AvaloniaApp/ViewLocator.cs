using Avalonia.Controls;
using Avalonia.Controls.Templates;

using AvaloniaApp.ViewModels;
using AvaloniaApp.Views;

namespace AvaloniaApp;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;
        if (data is HomePageViewModel)
            return new HomePageView();
        if (data is NewsPageViewModel)
            return new NewsPageView();
        if (data is MessagePageViewModel)
            return new MessagePageView();
        if (data is ProfilePageViewModel)
            return new ProfilePageView();

        return new TextBlock { Text = "Not Found: " + data.GetType().FullName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
