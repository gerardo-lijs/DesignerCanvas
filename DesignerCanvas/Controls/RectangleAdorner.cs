using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DesignerCanvas.Controls
{
    internal class RectangleAdorner : Adorner
    {
        private Point? startPoint;
        private Point? endPoint;
        private readonly Pen _rubberbandPen;
        private readonly DesignerCanvas _designerCanvas;

        public RectangleAdorner(DesignerCanvas designerCanvas, Point? dragStartPoint)
            : base(designerCanvas)
        {
            _designerCanvas = designerCanvas;
            startPoint = dragStartPoint;
            _rubberbandPen = new Pen(Brushes.LightSlateGray, 1)
            {
                DashStyle = new DashStyle(new double[] { 10 }, 1)
            };
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!IsMouseCaptured) CaptureMouse();

                endPoint = e.GetPosition(this);
                InvalidateVisual();
            }
            else
            {
                if (IsMouseCaptured) ReleaseMouseCapture();
            }

            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            // Release mouse capture
            if (IsMouseCaptured) ReleaseMouseCapture();

            // Remove this adorner from adorner layer
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_designerCanvas);
            if (adornerLayer is not null)
                adornerLayer.Remove(this);

            // Create rectangle
            if (startPoint is not null && endPoint is not null)
            {
                var objBox = new System.Windows.Shapes.Rectangle
                {
                    Fill = new SolidColorBrush(Colors.Black),
                    IsHitTestVisible = false
                };
                var regionItem = new DesignerItem(Guid.NewGuid())
                {
                    Opacity = 0.4,
                    Width = Math.Abs(endPoint.Value.X - startPoint.Value.X),
                    Height = Math.Abs(endPoint.Value.Y - startPoint.Value.Y),
                    Content = objBox,
                };
                Canvas.SetLeft(regionItem, Math.Min(startPoint.Value.X, endPoint.Value.X));
                Canvas.SetTop(regionItem, Math.Min(startPoint.Value.Y, endPoint.Value.Y));
                _designerCanvas.Children.Add(regionItem);
            }

            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // without a background the OnMouseMove event would not be fired!
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

            if (startPoint.HasValue && endPoint.HasValue)
                dc.DrawRectangle(Brushes.Transparent, _rubberbandPen, new Rect(startPoint.Value, endPoint.Value));
        }
    }
}
