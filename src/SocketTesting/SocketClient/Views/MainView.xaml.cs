namespace SocketClient.Views;

public partial class MainView : Window
{
    public MainView()
    {
        ViewModel = new MainViewModel { Owner = this };
        InitializeComponent();
    }

    public MainViewModel? ViewModel
    {
        get => DataContext as MainViewModel;
        set => DataContext = value;
    }
}