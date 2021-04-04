using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DesignerCanvas.Controls
{
    internal class DragThumb : Thumb
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

            // Horizontal change
            if (e.HorizontalChange > 0)
            {
                // NB: itemMaxLeft = Math.Max(itemLeft, designer.ActualWidth - designerItem.Width);
                var itemLeft = Canvas.GetLeft(designerItem);
                var dragLeft = Math.Min(Math.Max(itemLeft, designer.ActualWidth - designerItem.Width), itemLeft + e.HorizontalChange);
                if (itemLeft != dragLeft) Canvas.SetLeft(designerItem, dragLeft);
            }
            else
            {
                var itemLeft = Canvas.GetLeft(designerItem);
                var dragLeft = Math.Max(0, itemLeft + e.HorizontalChange);
                if (itemLeft != dragLeft) Canvas.SetLeft(designerItem, dragLeft);
            }

            // Vertical change
            if (e.VerticalChange > 0)
            {
                // NB: itemMaxTop = Math.Max(itemTop, designer.ActualHeight - designerItem.Height);
                var itemTop = Canvas.GetTop(designerItem);
                var dragTop = Math.Min(Math.Max(itemTop, designer.ActualHeight - designerItem.Height), itemTop + e.VerticalChange);
                if (itemTop != dragTop) Canvas.SetTop(designerItem, dragTop);
            }
            else
            {
                var itemTop = Canvas.GetTop(designerItem);
                var dragTop = Math.Max(0, itemTop + e.VerticalChange);
                if (itemTop != dragTop) Canvas.SetTop(designerItem, dragTop);
            }

            e.Handled = true;
        }
    }
}
