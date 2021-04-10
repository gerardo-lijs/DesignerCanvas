using System.Windows;

namespace DesignerCanvas
{
    public delegate void RectangleAutoDetectEventHandler(object sender, RectangleAutoDetectEventArgs e);
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
}
