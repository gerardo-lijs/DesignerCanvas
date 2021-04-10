using System.Windows;

namespace DesignerCanvas
{
    public delegate void RectangleDrawnEventHandler(object sender, RectangleDrawnEventArgs e);
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
}
