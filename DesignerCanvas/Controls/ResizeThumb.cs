using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DesignerCanvas.Controls
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

            // Resize vertical
            var itemTop = Canvas.GetTop(designerItem);
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Bottom:
                    if (e.VerticalChange > 0)
                    {
                        // NB: itemMaxHeight = designer.ActualHeight - itemTop;
                        designerItem.Height = Math.Min(designer.ActualHeight - itemTop, designerItem.ActualHeight + e.VerticalChange);
                    }
                    else
                    {
                        designerItem.Height = Math.Max(0, designerItem.ActualHeight + e.VerticalChange);
                    }
                    break;
                case VerticalAlignment.Top:
                    // NB: itemMinHeight = designerItem.ActualHeight - designerItem.MinHeight;
                    var dragDeltaVertical = Math.Min(Math.Max(-itemTop, e.VerticalChange), designerItem.ActualHeight - designerItem.MinHeight);
                    Canvas.SetTop(designerItem, itemTop + dragDeltaVertical);
                    designerItem.Height = designerItem.ActualHeight - dragDeltaVertical;
                    break;
                default:
                    break;
            }

            // Resize horizontal
            var itemLeft = Canvas.GetLeft(designerItem);
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    // NB: itemMinWidth = designerItem.ActualWidth - designerItem.MinWidth;
                    var dragDeltaHorizontal = Math.Min(Math.Max(-itemLeft, e.HorizontalChange), designerItem.ActualWidth - designerItem.MinWidth);
                    Canvas.SetLeft(designerItem, itemLeft + dragDeltaHorizontal);
                    designerItem.Width = designerItem.ActualWidth - dragDeltaHorizontal;
                    break;
                case HorizontalAlignment.Right:
                    if (e.HorizontalChange > 0)
                    {
                        // NB: itemMaxWidth = designer.ActualWidth - itemLeft;
                        designerItem.Width = Math.Min(designer.ActualWidth - itemLeft, designerItem.ActualWidth + e.HorizontalChange);
                    }
                    else
                    {
                        designerItem.Width = Math.Max(0, designerItem.ActualWidth + e.HorizontalChange);
                    }
                    break;
                default:
                    break;
            }

            e.Handled = true;
        }
    }
}
