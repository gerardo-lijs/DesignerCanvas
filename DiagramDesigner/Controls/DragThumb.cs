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

            // Horizontal change
            if (e.HorizontalChange > 0)
            {
                // NB: itemMaxLeft = Math.Max(Canvas.GetLeft(designerItem), designer.ActualWidth - designerItem.Width);
                Canvas.SetLeft(designerItem, Math.Min(Math.Max(Canvas.GetLeft(designerItem), designer.ActualWidth - designerItem.Width), Canvas.GetLeft(designerItem) + e.HorizontalChange));
            }
            else
            {
                Canvas.SetLeft(designerItem, Math.Max(0, Canvas.GetLeft(designerItem) + e.HorizontalChange));
            }

            // Vertical change
            if (e.VerticalChange > 0)
            {
                // NB: itemMaxTop = Math.Max(Canvas.GetTop(designerItem), designer.ActualHeight - designerItem.Height);
                Canvas.SetTop(designerItem, Math.Min(Math.Max(Canvas.GetTop(designerItem), designer.ActualHeight - designerItem.Height), Canvas.GetTop(designerItem) + e.VerticalChange));
            }
            else
            {
                Canvas.SetTop(designerItem, Math.Max(0, Canvas.GetTop(designerItem) + e.VerticalChange));
            }

            e.Handled = true;
        }
    }
}
