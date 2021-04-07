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
        private Point? rubberbandSelectionStartPoint = null;

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
                    // Create crosshair adorner
                    adornerLayer.Add(new CrosshairAdorner(this));
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
                    rubberbandSelectionStartPoint = new Point?(e.GetPosition(this));

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

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            switch (ToolMode)
            {
                case Tool.None:
                    break;
                case Tool.Select:
                    // If mouse button is not pressed we have no drag operation, ...
                    if (e.LeftButton != MouseButtonState.Pressed) rubberbandSelectionStartPoint = null;

                    // ... but if mouse button is pressed and start
                    // point value is set we do have one
                    if (rubberbandSelectionStartPoint is not null)
                    {
                        // create rubberband adorner
                        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                        if (adornerLayer is not null)
                        {
                            var adorner = new RubberbandAdorner(this, rubberbandSelectionStartPoint);
                            if (adorner is not null)
                            {
                                adornerLayer.Add(adorner);
                            }
                        }
                        e.Handled = true;
                    }
                    break;
                case Tool.Rectangle:
                    e.Handled = true;
                    //// Draw Crosshair lines
                    //if (e.LeftButton != MouseButtonState.Pressed && e.RightButton != MouseButtonState.Pressed)
                    //{
                    //    var currentPosition = e.GetPosition(this);

                    //    e.Handled = true;
                    //    //Children.Add(new System.Windows.Shapes.Line() { X1 = currentPosition.X, Y1 = 0, X2 = currentPosition.X, Y2 = 1024, StrokeThickness = 2, Stroke = new SolidColorBrush(Colors.Black) });
                    //}
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
    }
}
