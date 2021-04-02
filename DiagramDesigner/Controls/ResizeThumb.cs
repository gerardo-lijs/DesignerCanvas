using System;
using System.Collections.Generic;
using System.Linq;
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
            if (!(VisualTreeHelper.GetParent(designerItem) is DesignerCanvas designer)) return;

            // Check selected
            if (!designerItem.IsSelected) return;

            // Resize
            double dragDeltaVertical, dragDeltaHorizontal, scale;

            var selectedDesignerItems = designer.SelectionService.CurrentSelection.OfType<DesignerItem>();

            CalculateDragLimits(selectedDesignerItems, out var minLeft, out var minTop, out var minDeltaHorizontal, out var minDeltaVertical);

            foreach (DesignerItem item in selectedDesignerItems)
            {
                if (item != null && item.ParentID == Guid.Empty)
                {
                    switch (VerticalAlignment)
                    {
                        case VerticalAlignment.Bottom:
                            dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                            scale = (item.ActualHeight - dragDeltaVertical) / item.ActualHeight;
                            DragBottom(scale, item);
                            break;
                        case VerticalAlignment.Top:
                            dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                            scale = (item.ActualHeight - dragDeltaVertical) / item.ActualHeight;
                            DragTop(scale, item);
                            break;
                    }

                    switch (HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                            scale = (item.ActualWidth - dragDeltaHorizontal) / item.ActualWidth;
                            DragLeft(scale, item);
                            break;
                        case HorizontalAlignment.Right:
                            dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                            scale = (item.ActualWidth - dragDeltaHorizontal) / item.ActualWidth;
                            DragRight(scale, item);
                            break;
                    }
                }
            }
            e.Handled = true;
        }

        private void DragLeft(double scale, DesignerItem item)
        {
            double delta = item.Width * (scale - 1);
            Canvas.SetLeft(item, Canvas.GetLeft(item) - delta);
            item.Width = item.ActualWidth * scale;
        }

        private void DragTop(double scale, DesignerItem item)
        {
            double delta = item.Height * (scale - 1);
            Canvas.SetTop(item, Canvas.GetTop(item) - delta);
            item.Height = item.ActualHeight * scale;
        }

        private void DragRight(double scale, DesignerItem item)
        {
            item.Width = item.ActualWidth * scale;
        }

        private void DragBottom(double scale, DesignerItem item)
        {
            item.Height = item.ActualHeight * scale;
        }

        private void CalculateDragLimits(IEnumerable<DesignerItem> selectedItems, out double minLeft, out double minTop, out double minDeltaHorizontal, out double minDeltaVertical)
        {
            minLeft = double.MaxValue;
            minTop = double.MaxValue;
            minDeltaHorizontal = double.MaxValue;
            minDeltaVertical = double.MaxValue;

            // drag limits are set by these parameters: canvas top, canvas left, minHeight, minWidth
            // calculate min value for each parameter for each item
            foreach (DesignerItem item in selectedItems)
            {
                double left = Canvas.GetLeft(item);
                double top = Canvas.GetTop(item);

                minLeft = double.IsNaN(left) ? 0 : Math.Min(left, minLeft);
                minTop = double.IsNaN(top) ? 0 : Math.Min(top, minTop);

                minDeltaVertical = Math.Min(minDeltaVertical, item.ActualHeight - item.MinHeight);
                minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.ActualWidth - item.MinWidth);
            }
        }
    }
}
