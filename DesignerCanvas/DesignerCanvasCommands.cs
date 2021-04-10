using System.Windows.Input;

namespace DesignerCanvas
{
    public static class DesignerCanvasCommands
    {
        public static RoutedCommand BringForward { get; } = new();
        public static RoutedCommand BringToFront { get; } = new();
        public static RoutedCommand SendBackward { get; } = new();
        public static RoutedCommand SendToBack { get; } = new();
        public static RoutedCommand SelectAll { get; } = new();
        public static RoutedCommand ToolModeChange { get; } = new();
    }
}
