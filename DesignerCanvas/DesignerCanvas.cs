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

        private static readonly Cursor Add_Cursor;

        static DesignerCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerCanvas), new FrameworkPropertyMetadata(typeof(DesignerCanvas)));
            var cursorUri = new Uri("pack://application:,,,/DesignerCanvas;component/Cursors/add.cur");
            Add_Cursor = new Cursor(Application.GetResourceStream(cursorUri).Stream);
        }

        #region RectangleDrawnEvent
        public class RectangleDrawnEventArgs : RoutedEventArgs
        {
            public double Left { get; }
            public double Top { get; }
            public double Width { get; }
            public double Height { get; }

            public RectangleDrawnEventArgs(RoutedEvent routedEvent, double left, double top, double width, double height) : base(routedEvent)
            {
                Left = left;
                Top = top;
                Width = width;
                Height = height;
            }
        }

        public static readonly RoutedEvent RectangleDrawnEvent = EventManager.RegisterRoutedEvent("RectangleDrawn", RoutingStrategy.Bubble, typeof(RectangleDrawnEventHandler), typeof(DesignerCanvas));
        public delegate void RectangleDrawnEventHandler(object sender, RectangleDrawnEventArgs e);
        public event RectangleDrawnEventHandler RectangleDrawn
        {
            add => AddHandler(RectangleDrawnEvent, value);
            remove => RemoveHandler(RectangleDrawnEvent, value);
        }

        protected internal void OnRectangleDrawn(double left, double top, double width, double height)
        {
            var newEventArgs = new RectangleDrawnEventArgs(RectangleDrawnEvent, left, top, width, height);
            RaiseEvent(newEventArgs);
        }
        #endregion RectangleDrawnEvent

        #region RectangleAutoDetectEvent
        public class RectangleAutoDetectEventArgs : RoutedEventArgs
        {
            public double CenterPosX { get; }
            public double CenterPosY { get; }

            public RectangleAutoDetectEventArgs(RoutedEvent routedEvent, double centerPosX, double centerPosY) : base(routedEvent)
            {
                CenterPosX = centerPosX;
                CenterPosY = centerPosY;
            }
        }

        public static readonly RoutedEvent RectangleAutoDetectEvent = EventManager.RegisterRoutedEvent("RectangleAutoDetect", RoutingStrategy.Bubble, typeof(RectangleAutoDetectEventHandler), typeof(DesignerCanvas));
        public delegate void RectangleAutoDetectEventHandler(object sender, RectangleAutoDetectEventArgs e);
        public event RectangleAutoDetectEventHandler RectangleAutoDetect
        {
            add => AddHandler(RectangleAutoDetectEvent, value);
            remove => RemoveHandler(RectangleAutoDetectEvent, value);
        }

        protected internal void OnRectangleAutoDetect(double centerPosX, double centerPosY)
        {
            var newEventArgs = new RectangleAutoDetectEventArgs(RectangleAutoDetectEvent, centerPosX, centerPosY);
            RaiseEvent(newEventArgs);
        }
        #endregion RectangleAutoDetectEvent

        #region ToolMode
        public static readonly DependencyProperty ToolModeProperty = DependencyProperty.Register("ToolMode", typeof(Tool), typeof(DesignerCanvas), new PropertyMetadata(Tool.Select, new PropertyChangedCallback(OnToolModeChanged)));
        public Tool ToolMode
        {
            get { return (Tool)GetValue(ToolModeProperty); }
            set { SetValue(ToolModeProperty, value); }
        }

        private static void OnToolModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DesignerCanvas;
            if (control is not null)
            {
                control.OnToolModeChanged((Tool)e.OldValue, (Tool)e.NewValue);
            }
        }

        protected virtual void OnToolModeChanged(Tool oldValue, Tool newValue)
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
            switch (newValue)
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
            _crosshairRender = false;
            InvalidateVisual();
        }
        #endregion ToolMode

        private SelectionService selectionService;
        internal SelectionService SelectionService
        {
            get
            {
                selectionService ??= new SelectionService(this);
                return selectionService;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (ToolMode == Tool.Rectangle)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    Cursor = Add_Cursor;
                }
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
            Cursor = Cursors.Arrow;
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
                    if ((Keyboard.Modifiers & (ModifierKeys.Control)) != ModifierKeys.None)
                    {
                        // Auto detect rectangle at current position
                        var detectCenterPosition = e.GetPosition(this);
                        OnRectangleAutoDetect(detectCenterPosition.X, detectCenterPosition.Y);
                    }
                    else
                    {
                        // in case that this click is the start of a rectangle operation we cache the start point
                        _mouseDownStartPoint = e.GetPosition(this);
                        _crosshairRender = false;
                        InvalidateVisual();
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

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (ToolMode == Tool.Rectangle && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Cursor = Add_Cursor;
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
            base.OnMouseEnter(e);
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
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        Cursor = Add_Cursor;
                    }
                    else
                    {
                        Cursor = Cursors.Arrow;
                    }
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
