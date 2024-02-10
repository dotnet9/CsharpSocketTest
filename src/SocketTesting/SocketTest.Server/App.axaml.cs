using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using SocketTest.Server.ViewModels;
using SocketTest.Server.Views;

namespace SocketTest.Server;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)

        {
            desktop.MainWindow = new MainWindow();
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            desktop.MainWindow.DataContext =
                new MainWindowViewModel(new WindowNotificationManager(topLevel) { MaxItems = 3 });
        }

        base.OnFrameworkInitializationCompleted();
    }
}