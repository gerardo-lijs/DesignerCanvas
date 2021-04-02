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

        private void DragLeft(double scale, DesignerItem item, SelectionService selectionService)
        {
            double groupLeft = Canvas.GetLeft(item) + item.Width;
            double groupItemLeft = Canvas.GetLeft(item);
            double delta = (groupLeft - groupItemLeft) * (scale - 1);
            Canvas.SetLeft(item, groupItemLeft - delta);
            item.Width = item.ActualWidth * scale;
        }

        private void DragTop(double scale, DesignerItem item, SelectionService selectionService)
        {
            double groupBottom = Canvas.GetTop(item) + item.Height;
            double groupItemTop = Canvas.GetTop(item);
            double delta = (groupBottom - groupItemTop) * (scale - 1);
            Canvas.SetTop(item, groupItemTop - delta);
            item.Height = item.ActualHeight * scale;
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
            var itemMaxWidth = designer.ActualWidth - itemLeft;
            var itemMaxHeight = designer.ActualHeight - itemTop;

            var minDeltaVertical = designerItem.ActualHeight - designerItem.MinHeight;
            var minDeltaHorizontal = designerItem.ActualWidth - designerItem.MinWidth;

            double dragDeltaVertical, dragDeltaHorizontal, scale;
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Bottom:
                    if (e.VerticalChange > 0)
                        designerItem.Height = Math.Min(itemMaxHeight, designerItem.ActualHeight + e.VerticalChange);
                    else
                        designerItem.Height = Math.Max(0, designerItem.ActualHeight + e.VerticalChange);

                    //dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                    //designerItem.Height = designerItem.ActualHeight - dragDeltaVertical;
                    break;
                case VerticalAlignment.Top:
                    dragDeltaVertical = Math.Min(Math.Max(-itemTop, e.VerticalChange), minDeltaVertical);
                    scale = (designerItem.ActualHeight - dragDeltaVertical) / designerItem.ActualHeight;
                    DragTop(scale, designerItem, designer.SelectionService);
                    break;
                default:
                    break;
            }

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    dragDeltaHorizontal = Math.Min(Math.Max(-itemLeft, e.HorizontalChange), minDeltaHorizontal);
                    scale = (designerItem.ActualWidth - dragDeltaHorizontal) / designerItem.ActualWidth;
                    DragLeft(scale, designerItem, designer.SelectionService);
                    break;
                case HorizontalAlignment.Right:
                    if (e.HorizontalChange > 0)
                        designerItem.Width = Math.Min(itemMaxWidth, designerItem.ActualWidth + e.HorizontalChange);
                    else
                        designerItem.Width = Math.Max(0, designerItem.ActualWidth + e.HorizontalChange);
                    //dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                    //designerItem.Width = designerItem.ActualWidth - dragDeltaHorizontal;
                    break;
                default:
                    break;
            }

            //// Calculate resize limits
            //var itemMaxLeft = Math.Max(itemLeft, designer.ActualWidth - designerItem.Width);
            //var itemMaxTop = Math.Max(itemTop, designer.ActualHeight - designerItem.Height);

            //// Resize
            //double deltaVertical;
            //double deltaHorizontal;
            //double scale;
            //switch (VerticalAlignment)
            //{
            //    case VerticalAlignment.Top:
            //        //dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);

            //        if (e.VerticalChange > 0)
            //        {
            //            deltaVertical = Math.Min(designerItem.ActualHeight - designerItem.MinHeight, e.VerticalChange);
            //            scale = (designerItem.ActualHeight - deltaVertical) / designerItem.ActualHeight;
            //            Canvas.SetTop(designerItem, Math.Min(itemMaxTop, itemTop + deltaVertical));
            //            designerItem.Height = designerItem.ActualHeight * scale;
            //        }
            //        else
            //        {
            //            deltaVertical = Math.Min(designerItem.ActualHeight - designerItem.MinHeight, e.VerticalChange);
            //            scale = (designerItem.ActualHeight - deltaVertical) / designerItem.ActualHeight;
            //            var deltaVertical2 = Math.Max(0, itemTop + deltaVertical);
            //            Canvas.SetTop(designerItem, Math.Max(0, itemTop + deltaVertical));
            //            designerItem.Height = designerItem.ActualHeight + deltaVertical2 - itemTop;
            //        }
            //        break;
            //}

            //switch (HorizontalAlignment)
            //{
            //    case HorizontalAlignment.Left:
            //        //if (e.HorizontalChange > 0)
            //        //    Canvas.SetLeft(designerItem, Math.Min(itemMaxLeft, itemLeft + e.HorizontalChange));
            //        //else
            //        //    Canvas.SetLeft(designerItem, Math.Max(0, itemLeft + e.HorizontalChange));

            //        //if (e.HorizontalChange > 0)
            //        //{
            //        //    designerItem.Width = designerItem.Width - e.HorizontalChange;
            //        //}
            //        //else
            //        //{
            //        //    designerItem.Width = designerItem.Width - e.HorizontalChange;
            //        //}
            //        break;

            e.Handled = true;
        }
    }
}
