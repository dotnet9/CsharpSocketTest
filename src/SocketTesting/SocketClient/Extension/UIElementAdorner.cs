using System.Collections;
using System.Windows.Documents;
using System.Windows.Media;

namespace SocketClient.Extension
{
    public class UIElementAdorner : Adorner
    {
        private readonly UIElement child;

        public UIElementAdorner(UIElement element, UIElement child)
            : base(element)
        {
            this.child = child;
            AddLogicalChild(child);
            AddVisualChild(child);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            child.Arrange(new Rect(finalSize));

            return finalSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            child.Measure(constraint);

            return AdornedElement.RenderSize;
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            return child;
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                ArrayList list =
                [
                    child
                ];
                return list.GetEnumerator();
            }
        }

        public UIElement Child => child;
    }
}