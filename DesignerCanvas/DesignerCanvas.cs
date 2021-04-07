using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DesignerCanvas.Controls;

namespace DesignerCanvas
{
    public partial class DesignerCanvas : Canvas
    {
        private Point? _mouseDownStartPoint;
        private Point? currentPosition;
        private Pen? _crosshairPen;
        private bool _crosshairRender;

        static DesignerCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerCanvas), new FrameworkPropertyMetadata(typeof(DesignerCanvas)));
        }

        public Tool ToolMode { get; private set; } = Tool.Select;
        public void ChangeToolMode(Tool tool)
        {
            // Clear previous adorners
            var adornerLayer = AdornerLayer.GetAdornerLayer(this);
            {
                var adorners = adornerLayer.GetAdorners(this);
                if (adorners is not null)
                {
                    foreach (var adorner in adorners)
                    {
                        adornerLayer.Remove(adorner);
                    }
                }
            }

            // Create tool adorner/mode
            switch (tool)
            {
                case Tool.None:
                    break;
                case Tool.Select:
                    break;
                case Tool.Rectangle:
                    if (_crosshairPen is null)
                    {
                        _crosshairPen = new Pen(Brushes.LightSlateGray, 1)
                        {
                            DashStyle = new DashStyle(new double[] { 5 }, 1)
                        };
                    }
                    // Create crosshair adorner
                    //adornerLayer.Add(new CrosshairAdorner(this));
                    break;
                case Tool.Polygon:
                    break;
                case Tool.DetectRectangle:
                    break;
                case Tool.DetectPolygon:
                    break;
                default:
                    break;
            }
            SelectionService.ClearSelection();
            ToolMode = tool;
            _crosshairRender = false;
            InvalidateVisual();
        }

        private SelectionService selectionService;
        internal SelectionService SelectionService
        {
            get
            {
                selectionService ??= new SelectionService(this);
                return selectionService;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            switch (ToolMode)
            {
                case Tool.None:
                    break;
                case Tool.Select:
                    // in case that this click is the start of a 
                    // drag operation we cache the start point
                    _mouseDownStartPoint = e.GetPosition(this);

                    // if you click directly on the canvas all 
                    // selected items are 'de-selected'
                    SelectionService.ClearSelection();
                    Focus();
                    e.Handled = true;
                    break;
                case Tool.Rectangle:
                    // in case that this click is the start of a rectangle operation we cache the start point
                    _mouseDownStartPoint = e.GetPosition(this);
                    _crosshairRender = false;
                    InvalidateVisual();
                    e.Handled = true;
                    break;
                case Tool.Polygon:
                    break;
                case Tool.DetectRectangle:
                    break;
                case Tool.DetectPolygon:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            switch (ToolMode)
            {
                case Tool.None:
                    break;
                case Tool.Select:
                    // If mouse button is not pressed we have no drag operation, ...
                    if (e.LeftButton != MouseButtonState.Pressed) _mouseDownStartPoint = null;

                    // ... but if mouse button is pressed and start
                    // point value is set we do have one
                    if (_mouseDownStartPoint is not null)
                    {
                        // create rubberband adorner
                        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                        if (adornerLayer is not null)
                        {
                            var adorner = new RubberbandAdorner(this, _mouseDownStartPoint);
                            if (adorner is not null)
                            {
                                adornerLayer.Add(adorner);
                            }
                        }
                        e.Handled = true;
                    }
                    break;
                case Tool.Rectangle:
                    if (e.LeftButton != MouseButtonState.Pressed && e.RightButton != MouseButtonState.Pressed)
                    {
                        _crosshairRender = true;
                        currentPosition = e.GetPosition(this);
                        InvalidateVisual();
                    }
                    else
                    {
                        // Create rectangle adorner
                        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                        if (adornerLayer is not null)
                        {
                            var adorner = new RectangleAdorner(this, _mouseDownStartPoint);
                            if (adorner is not null)
                            {
                                adornerLayer.Add(adorner);
                            }
                        }
                    }
                    e.Handled = true;
                    break;
                case Tool.Polygon:
                    break;
                case Tool.DetectRectangle:
                    break;
                case Tool.DetectPolygon:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // Crosshair
            if (_crosshairRender && currentPosition is not null)
            {
                dc.DrawLine(_crosshairPen, new Point(currentPosition.Value.X, 0), new Point(currentPosition.Value.X, RenderSize.Height));
                dc.DrawLine(_crosshairPen, new Point(0, currentPosition.Value.Y), new Point(RenderSize.Width, currentPosition.Value.Y));
            }
        }
    }
}
