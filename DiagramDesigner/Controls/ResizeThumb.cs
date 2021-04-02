using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DiagramDesigner.Controls
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            DragDelta += new DragDeltaEventHandler(ResizeThumb_DragDelta);
        }

        void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // Check
            if (!(DataContext is DesignerItem designerItem)) return;
            if (designerItem.ParentID != Guid.Empty) return;

            // Get Canvas
            if (!(VisualTreeHelper.GetParent(designerItem) is DesignerCanvas designer)) return;

            // Calculate resize limits
            var itemLeft = Canvas.GetLeft(designerItem);
            var itemTop = Canvas.GetTop(designerItem);

            var minLeft = double.IsNaN(itemLeft) ? 0 : itemLeft;
            var minTop = double.IsNaN(itemTop) ? 0 : itemTop;
            var minDeltaVertical = designerItem.ActualHeight - designerItem.MinHeight;
            var minDeltaHorizontal = designerItem.ActualWidth - designerItem.MinWidth;

            // Resize
            double dragDeltaVertical, dragDeltaHorizontal, scale;
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Bottom:
                    dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                    scale = (designerItem.ActualHeight - dragDeltaVertical) / designerItem.ActualHeight;

                    designerItem.Height = designerItem.ActualHeight * scale;
                    break;

                case VerticalAlignment.Top:
                    dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                    scale = (designerItem.ActualHeight - dragDeltaVertical) / designerItem.ActualHeight;

                    var delta = designerItem.Height * (scale - 1);
                    Canvas.SetTop(designerItem, Canvas.GetTop(designerItem) - delta);
                    designerItem.Height = designerItem.ActualHeight * scale;
                    break;
            }

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                    scale = (designerItem.ActualWidth - dragDeltaHorizontal) / designerItem.ActualWidth;

                    var delta = designerItem.Width * (scale - 1);
                    Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem) - delta);
                    designerItem.Width = designerItem.ActualWidth * scale;
                    break;

                case HorizontalAlignment.Right:
                    dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                    scale = (designerItem.ActualWidth - dragDeltaHorizontal) / designerItem.ActualWidth;

                    designerItem.Width = designerItem.ActualWidth * scale;

                    break;
            }

            e.Handled = true;
        }
    }
}
