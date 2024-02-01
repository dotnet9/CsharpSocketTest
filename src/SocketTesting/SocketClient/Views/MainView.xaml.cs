namespace SocketClient.Views;

public partial class MainView : Window
{
    public MainViewModel? ViewModel
    {
        get => DataContext as MainViewModel;
        set => DataContext = value;
    }

    public MainView()
    {
        ViewModel = new MainViewModel();
        InitializeComponent();
        ViewModel.Owner = this;
    }
}