﻿using System;
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
            if (DataContext is not DesignerItem designerItem) return;

            // Get Canvas
            if (VisualTreeHelper.GetParent(designerItem) is not DesignerCanvas designer) return;

            // Horizontal change
            if (e.HorizontalChange > 0)
            {
                var itemLeft = Canvas.GetLeft(designerItem);
                var dragHorizontal = Math.Min(Math.Max(itemLeft, designer.ActualWidth - designerItem.Width - designerItem.Margin.Left - designerItem.Margin.Right), itemLeft + e.HorizontalChange);
                if (itemLeft != dragHorizontal) Canvas.SetLeft(designerItem, dragHorizontal);
            }
            else
            {
                var itemLeft = Canvas.GetLeft(designerItem);
                var dragHorizontal = Math.Max(0, itemLeft + e.HorizontalChange);
                if (itemLeft != dragHorizontal) Canvas.SetLeft(designerItem, dragHorizontal);
            }

            // Vertical change
            if (e.VerticalChange > 0)
            {
                var itemTop = Canvas.GetTop(designerItem);
                var dragVertical = Math.Min(Math.Max(itemTop, designer.ActualHeight - designerItem.Height - designerItem.Margin.Top - designerItem.Margin.Bottom), itemTop + e.VerticalChange);
                if (itemTop != dragVertical) Canvas.SetTop(designerItem, dragVertical);
            }
            else
            {
                var itemTop = Canvas.GetTop(designerItem);
                var dragVertical = Math.Max(0, itemTop + e.VerticalChange);
                if (itemTop != dragVertical) Canvas.SetTop(designerItem, dragVertical);
            }

            e.Handled = true;
        }
    }
}
