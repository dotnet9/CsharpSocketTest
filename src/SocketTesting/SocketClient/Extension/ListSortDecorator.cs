using System.ComponentModel;
using System.Windows.Controls;

namespace SocketClient.Extension
{
    // 参考：https://www.cnblogs.com/nankezhishi/archive/2009/12/04/SortListView.html
    public class ListSortDecorator : Control
    {
        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register(nameof(SortDirection), typeof(ListSortDirection), typeof(ListSortDecorator));

        static ListSortDecorator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListSortDecorator),
                new FrameworkPropertyMetadata(typeof(ListSortDecorator)));
        }

        public ListSortDirection SortDirection
        {
            get => (ListSortDirection)GetValue(SortDirectionProperty);
            set => SetValue(SortDirectionProperty, value);
        }
    }
}