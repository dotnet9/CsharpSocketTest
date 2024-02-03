using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SocketClient.Extension
{
    public class ListViewBehavior
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty HeaderSortProperty =
            DependencyProperty.RegisterAttached("HeaderSort", typeof(bool), typeof(ListViewBehavior),
                new UIPropertyMetadata(OnHeaderSortPropertyChanged));

        /// <summary>
        /// 
        /// </summary>
        internal static readonly DependencyPropertyKey SortInfoProperty =
            DependencyProperty.RegisterAttachedReadOnly("SortInfo", typeof(SortInfo), typeof(ListViewBehavior),
                new PropertyMetadata());

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty SortFieldProperty =
            DependencyProperty.RegisterAttached("SortField", typeof(string), typeof(ListViewBehavior));

        public static bool GetHeaderSort(DependencyObject obj)
        {
            return (bool)obj.GetValue(HeaderSortProperty);
        }

        public static void SetHeaderSort(DependencyObject obj, bool value)
        {
            obj.SetValue(HeaderSortProperty, value);
        }

        public static SortInfo GetSortInfo(DependencyObject obj)
        {
            return (SortInfo)obj.GetValue(SortInfoProperty.DependencyProperty);
        }

        internal static void SetSortInfo(DependencyObject obj, SortInfo value)
        {
            obj.SetValue(SortInfoProperty.DependencyProperty, value);
        }

        public static string GetSortField(DependencyObject obj)
        {
            return (string)obj.GetValue(SortFieldProperty);
        }

        public static void SetSortField(DependencyObject obj, string value)
        {
            obj.SetValue(SortFieldProperty, value);
        }

        private static void OnHeaderSortPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ListView listView)
            {
                throw new InvalidOperationException("HeaderSort Property can only be set on a ListView");
            }

            if ((bool)e.NewValue)
            {
                listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnListViewHeaderClick));
            }
            else
            {
                listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnListViewHeaderClick));
            }
        }

        private static void OnListViewHeaderClick(object sender, RoutedEventArgs e)
        {
            var listView = e.Source as ListView;
            var header = e.OriginalSource as GridViewColumnHeader;
            var sortInfo = listView?.GetValue(SortInfoProperty.DependencyProperty) as SortInfo;

            if (sortInfo != null)
            {
                AdornerLayer.GetAdornerLayer(sortInfo.LastSortColumn)?.Remove(sortInfo.CurrentAdorner);
                listView?.Items.SortDescriptions.Clear();
            }
            else
                sortInfo = new SortInfo();

            if (sortInfo.LastSortColumn == header)
                ((sortInfo.CurrentAdorner.Child as ListSortDecorator)!).SortDirection =
                    ((sortInfo.CurrentAdorner.Child as ListSortDecorator)!).SortDirection == ListSortDirection.Ascending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
            else
                sortInfo.CurrentAdorner = new UIElementAdorner(header!, new ListSortDecorator());

            sortInfo.LastSortColumn = header!;
            listView!.SetValue(SortInfoProperty, sortInfo);

            AdornerLayer.GetAdornerLayer(header!)?.Add(sortInfo.CurrentAdorner);
            var sortDescription = new SortDescription()
            {
                Direction = ((sortInfo.CurrentAdorner.Child as ListSortDecorator)!).SortDirection,
                PropertyName = header!.Column.GetValue(SortFieldProperty) as string ?? header.Column.Header as string
            };
            listView.Items.SortDescriptions.Add(sortDescription);
        }
    }
}