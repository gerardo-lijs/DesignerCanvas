using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DiagramDesigner.Controls
{
    public class DragThumb : Thumb
    {
        public DragThumb()
        {
            DragDelta += new DragDeltaEventHandler(DragThumb_DragDelta);
        }

        private void DragThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // Check
            if (!(DataContext is DesignerItem designerItem)) return;

            // Get Canvas
            if (!(VisualTreeHelper.GetParent(designerItem) is DesignerCanvas designer)) return;

            // Calculate drag limits
            var itemLeft = Canvas.GetLeft(designerItem);
            var itemTop = Canvas.GetTop(designerItem);
            var itemMaxLeft = Math.Max(itemLeft, designer.ActualWidth - designerItem.Width);
            var itemMaxTop = Math.Max(itemTop, designer.ActualHeight - designerItem.Height);

            // Horizontal change
            if (e.HorizontalChange > 0)
                Canvas.SetLeft(designerItem, Math.Min(itemMaxLeft, itemLeft + e.HorizontalChange));
            else
                Canvas.SetLeft(designerItem, Math.Max(0, itemLeft + e.HorizontalChange));

            // Vertical change
            if (e.VerticalChange > 0)
                Canvas.SetTop(designerItem, Math.Min(itemMaxTop, itemTop + e.VerticalChange));
            else
                Canvas.SetTop(designerItem, Math.Max(0, itemTop + e.VerticalChange));

            e.Handled = true;
        }
    }
}
