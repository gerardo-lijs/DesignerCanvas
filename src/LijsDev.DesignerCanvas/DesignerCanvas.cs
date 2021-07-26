using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LijsDev.DesignerCanvas.Controls;

namespace LijsDev.DesignerCanvas
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

            // Load cursors
            var cursorUri = new Uri("pack://application:,,,/LijsDev.DesignerCanvas;component/Cursors/add.cur");
            Add_Cursor = new Cursor(Application.GetResourceStream(cursorUri).Stream);
        }

        #region SelectionChangedEvent

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(DesignerCanvas));
        public event SelectionChangedEventHandler SelectionChanged
        {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        protected internal void RaiseSelectionChangedEvent(List<IDesignerItem> items)
        {
            var newEventArgs = new SelectionChangedEventArgs(SelectionChangedEvent, items);
            RaiseEvent(newEventArgs);
        }
        #endregion SelectionChangedEvent

        #region RectangleDrawnEvent

        public static readonly RoutedEvent RectangleDrawnEvent = EventManager.RegisterRoutedEvent(nameof(RectangleDrawn), RoutingStrategy.Bubble, typeof(RectangleDrawnEventHandler), typeof(DesignerCanvas));
        public event RectangleDrawnEventHandler RectangleDrawn
        {
            add => AddHandler(RectangleDrawnEvent, value);
            remove => RemoveHandler(RectangleDrawnEvent, value);
        }

        protected internal void RaiseRectangleDrawnEvent(double left, double top, double width, double height)
        {
            if (IsReadOnly) return;
            var newEventArgs = new RectangleDrawnEventArgs(RectangleDrawnEvent, left, top, width, height);
            RaiseEvent(newEventArgs);
        }
        #endregion RectangleDrawnEvent

        #region RectangleAutoDetectEvent
        public static readonly RoutedEvent RectangleAutoDetectEvent = EventManager.RegisterRoutedEvent(nameof(RectangleAutoDetect), RoutingStrategy.Bubble, typeof(RectangleAutoDetectEventHandler), typeof(DesignerCanvas));
        public event RectangleAutoDetectEventHandler RectangleAutoDetect
        {
            add => AddHandler(RectangleAutoDetectEvent, value);
            remove => RemoveHandler(RectangleAutoDetectEvent, value);
        }

        protected internal void RaiseRectangleAutoDetectEvent(double centerPosX, double centerPosY)
        {
            if (IsReadOnly) return;
            var newEventArgs = new RectangleAutoDetectEventArgs(RectangleAutoDetectEvent, centerPosX, centerPosY);
            RaiseEvent(newEventArgs);
        }
        #endregion RectangleAutoDetectEvent

        #region ItemsDeletedEvent

        public static readonly RoutedEvent ItemsDeletedEvent = EventManager.RegisterRoutedEvent(nameof(ItemsDeleted), RoutingStrategy.Bubble, typeof(ItemsDeletedEventHandler), typeof(DesignerCanvas));
        public event ItemsDeletedEventHandler ItemsDeleted
        {
            add => AddHandler(ItemsDeletedEvent, value);
            remove => RemoveHandler(ItemsDeletedEvent, value);
        }

        protected internal void RaiseItemsDeletedEvent()
        {
            if (IsReadOnly) return;
            var newEventArgs = new RoutedEventArgs(ItemsDeletedEvent);
            RaiseEvent(newEventArgs);
        }
        #endregion ItemsDeletedEvent

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
                    if (!IsReadOnly && _crosshairPen is null)
                    {
                        _crosshairPen = new Pen(Brushes.LightSlateGray, 1)
                        {
                            DashStyle = new DashStyle(new double[] { 5 }, 1)
                        };
                    }
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

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));

        public bool DisableClipboard
        {
            get => (bool)GetValue(DisableClipboardProperty);
            set => SetValue(DisableClipboardProperty, value);
        }
        public static readonly DependencyProperty DisableClipboardProperty = DependencyProperty.Register(nameof(DisableClipboard), typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));

        public bool DisableZOrder
        {
            get => (bool)GetValue(DisableZOrderProperty);
            set => SetValue(DisableZOrderProperty, value);
        }
        public static readonly DependencyProperty DisableZOrderProperty = DependencyProperty.Register(nameof(DisableZOrder), typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));

        public List<IDesignerItem> SelectedItems { get; } = new();

        private SelectionService selectionService;
        internal SelectionService SelectionService
        {
            get
            {
                selectionService ??= new SelectionService(this);
                return selectionService;
            }
        }

        public void SelectItem(Guid id)
        {
            var designerItem = Children.OfType<IDesignerItem>().Where(x => x.Id == id).FirstOrDefault();
            if (designerItem is null) throw new Exception($"Could not find designer item with the specified Id: {id}");

            // Check
            if (!designerItem.AllowSelection) return;

            // Select
            designerItem.IsSelected = true;
            selectionService.ClearSelection();
            selectionService.AddToSelection(designerItem);
        }

        public void SelectItems(List<Guid> ids)
        {
            selectionService.ClearSelection();
            foreach (var id in ids)
            {
                var designerItem = Children.OfType<IDesignerItem>().Where(x => x.Id == id).FirstOrDefault();
                if (designerItem is null) throw new Exception($"Could not find designer item with the specified Id: {id}");

                // Check
                if (!designerItem.AllowSelection) continue;

                // Select
                designerItem.IsSelected = true;
                selectionService.AddToSelection(designerItem);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            RefreshCursor();
        }

        private void RefreshCursor()
        {
            if (IsReadOnly)
            {
                Cursor = Cursors.Arrow;
                return;
            }

            if (ToolMode == Tool.Rectangle)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                    Cursor = Add_Cursor;
                else
                    Cursor = Cursors.Cross;
            }
            else if (ToolMode == Tool.DetectRectangle)
            {
                Cursor = Add_Cursor;
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            RefreshCursor();
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
            RefreshCursor();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            switch (ToolMode)
            {
                case Tool.None:
                    break;
                case Tool.Select:
                    break;
                case Tool.Rectangle:
                    if (!IsReadOnly)
                    {
                        if ((Keyboard.Modifiers & (ModifierKeys.Control)) != ModifierKeys.None)
                        {
                            // Auto detect rectangle at current position
                            var detectCenterPosition = e.GetPosition(this);
                            RaiseRectangleAutoDetectEvent(detectCenterPosition.X, detectCenterPosition.Y);
                        }
                        else
                        {
                            // in case that this click is the start of a rectangle operation we cache the start point
                            _mouseDownStartPoint = e.GetPosition(this);
                            _crosshairRender = false;
                            InvalidateVisual();
                        }
                        e.Handled = true;
                    }
                    break;
                case Tool.Polygon:
                    break;
                case Tool.DetectRectangle:
                    if (!IsReadOnly)
                    {
                        // Auto detect rectangle at current position
                        var detectCenterPosition = e.GetPosition(this);
                        RaiseRectangleAutoDetectEvent(detectCenterPosition.X, detectCenterPosition.Y);
                    }
                    break;
                case Tool.DetectPolygon:
                    break;
                default:
                    throw new NotImplementedException();
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

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // Remove crosshair
            _crosshairRender = false;
            InvalidateVisual();
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
                    if (!IsReadOnly)
                    {
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
                    }
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
