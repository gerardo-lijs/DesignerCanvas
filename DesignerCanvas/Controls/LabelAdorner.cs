using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DesignerCanvas.Controls
{
    public class LabelAdorner : Adorner
    {
        private readonly Brush _backgroundBrush;
        private readonly string _label;

        public LabelAdorner(UIElement adornedElement, Brush backgroundBrush, string label) : base(adornedElement)
        {
            _backgroundBrush = backgroundBrush;
            _label = label;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var formattedText = new FormattedText(_label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), 12, Brushes.White);
            drawingContext.DrawRectangle(_backgroundBrush, null, new Rect(0, -21, formattedText.Width + 10, 20));
            drawingContext.DrawText(formattedText, new Point(4, -19));
        }
    }
}
