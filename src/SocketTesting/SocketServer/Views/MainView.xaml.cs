namespace SocketServer.Views;

public partial class MainView : Window
{
    public MainView()
    {
        ViewModel = new MainViewModel();
        InitializeComponent();
    }

    public MainViewModel? ViewModel
    {
        get => DataContext as MainViewModel;
        set => DataContext = value;
    }
}