using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LijsDev.DesignerCanvas.Controls
{
    internal class CrosshairAdorner : Adorner
    {
        private Point? currentPosition;
        private readonly Pen _rubberbandPen;

        public CrosshairAdorner(DesignerCanvas designerCanvas) : base(designerCanvas)
        {
            _rubberbandPen = new Pen(Brushes.LightSlateGray, 1)
            {
                DashStyle = new DashStyle(new double[] { 2 }, 1)
            };
            if (!IsMouseCaptured) CaptureMouse();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed && e.RightButton != MouseButtonState.Pressed)
            {
                currentPosition = e.GetPosition(this);
                InvalidateVisual();
            }
            e.Handled = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // Check
            if (currentPosition is null) return;

            dc.DrawLine(_rubberbandPen, new Point(currentPosition.Value.X, 0), new Point(currentPosition.Value.X, RenderSize.Height));
            dc.DrawLine(_rubberbandPen, new Point(0, currentPosition.Value.Y), new Point(RenderSize.Width, currentPosition.Value.Y));
        }
    }
}
