using System.Windows.Controls;

namespace SocketClient.Extension
{
    public class SortInfo
    {
        public GridViewColumnHeader LastSortColumn { get; set; }

        public UIElementAdorner CurrentAdorner { get; set; }
    }
}
