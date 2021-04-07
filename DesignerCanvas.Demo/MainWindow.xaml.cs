using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesignerCanvasDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DemoDesignerCanvas_RectangleDrawn(object sender, DesignerCanvas.DesignerCanvas.RectangleDrawnEventArgs e)
        {
            var objBox = new System.Windows.Shapes.Rectangle
            {
                Fill = new SolidColorBrush(Colors.Black),
                IsHitTestVisible = false
            };
            var regionItem = new DesignerCanvas.DesignerItem(Guid.NewGuid())
            {
                Opacity = 0.4,
                Width = e.Width,
                Height = e.Height,
                Content = objBox,
            };
            Canvas.SetLeft(regionItem, e.Left);
            Canvas.SetTop(regionItem, e.Top);

            DemoDesignerCanvas.Children.Add(regionItem);
        }
    }
}
