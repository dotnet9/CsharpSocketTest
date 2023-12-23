using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

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

    private ListSortDirection _sortDirection;
    private GridViewColumnHeader _sortColumn;

    private void PointListView_OnClick(object sender, RoutedEventArgs e)
    {
        if (!(e.OriginalSource is GridViewColumnHeader column) || column.Column == null)
        {
            return;
        }

        if (_sortColumn == column)
        {
            _sortDirection = _sortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }
        else
        {
            if (_sortColumn?.Column != null)
            {
                _sortColumn.Column.HeaderTemplate = null;
                _sortColumn.Column.Width = _sortColumn.ActualWidth - 20;
            }

            _sortColumn = column;
            _sortDirection = ListSortDirection.Ascending;
            column.Column.Width = column.ActualWidth + 20;
        }

        if (_sortDirection == ListSortDirection.Ascending)
        {
            column.Column.HeaderTemplate = Resources["ArrowUp"] as DataTemplate;
        }
        else
        {
            column.Column.HeaderTemplate = Resources["ArrowDown"] as DataTemplate;
        }

        var header = string.Empty;

        if (_sortColumn.Column.DisplayMemberBinding is Binding b)
        {
            header = b.Path.Path;
        }

        var itemsSource = (sender as ListView)?.ItemsSource;
        if (itemsSource == null)
        {
            return;
        }

        var resultDataView = CollectionViewSource.GetDefaultView(
            itemsSource);
        resultDataView.SortDescriptions.Clear();
        resultDataView.SortDescriptions.Add(
            new SortDescription(header, _sortDirection));
    }
}