using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Interactivity;
using SocketTest.Client.ViewModels;

namespace SocketTest.Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        var vm = DataContext as MainWindowViewModel;
        if (vm is not { NotificationManager: null }) return;
        var topLevel = GetTopLevel(this);
        vm.NotificationManager =
            new WindowNotificationManager(topLevel) { MaxItems = 3 };
    }
}