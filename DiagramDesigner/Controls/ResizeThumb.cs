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
            var itemMinWidth = designerItem.ActualWidth - designerItem.MinWidth;
            var itemMaxWidth = designer.ActualWidth - itemLeft;
            var itemMinHeight = designerItem.ActualHeight - designerItem.MinHeight;
            var itemMaxHeight = designer.ActualHeight - itemTop;

            switch (VerticalAlignment)
            {
                case VerticalAlignment.Bottom:
                    if (e.VerticalChange > 0)
                        designerItem.Height = Math.Min(itemMaxHeight, designerItem.ActualHeight + e.VerticalChange);
                    else
                        designerItem.Height = Math.Max(0, designerItem.ActualHeight + e.VerticalChange);
                    break;
                case VerticalAlignment.Top:
                    var dragDeltaVertical = Math.Min(Math.Max(-itemTop, e.VerticalChange), itemMinHeight);
                    Canvas.SetTop(designerItem, itemTop + dragDeltaVertical);
                    designerItem.Height = designerItem.ActualHeight - dragDeltaVertical;
                    break;
                default:
                    break;
            }

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    var dragDeltaHorizontal = Math.Min(Math.Max(-itemLeft, e.HorizontalChange), itemMinWidth);
                    Canvas.SetLeft(designerItem, itemLeft + dragDeltaHorizontal);
                    designerItem.Width = designerItem.ActualWidth - dragDeltaHorizontal;
                    break;
                case HorizontalAlignment.Right:
                    if (e.HorizontalChange > 0)
                        designerItem.Width = Math.Min(itemMaxWidth, designerItem.ActualWidth + e.HorizontalChange);
                    else
                        designerItem.Width = Math.Max(0, designerItem.ActualWidth + e.HorizontalChange);
                    break;
                default:
                    break;
            }

            e.Handled = true;
        }
    }
}
