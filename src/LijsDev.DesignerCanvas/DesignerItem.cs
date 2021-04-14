using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LijsDev.DesignerCanvas.Controls;

namespace LijsDev.DesignerCanvas
{
    [TemplatePart(Name = DragThumbPart, Type = typeof(DragThumb))]
    [TemplatePart(Name = ResizeDecoratorPart, Type = typeof(Control))]
    [TemplatePart(Name = ContentPresenterPart, Type = typeof(ContentPresenter))]
    public class DesignerItem : ContentControl, IDesignerItem
    {
        private const string DragThumbPart = "PART_DragThumb";
        private const string ResizeDecoratorPart = "PART_ResizeDecorator";
        private const string ContentPresenterPart = "PART_ContentPresenter";

        public Guid Id { get; }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));

        public bool AllowSelection
        {
            get => (bool)GetValue(AllowSelectionProperty);
            set => SetValue(AllowSelectionProperty, value);
        }
        public static readonly DependencyProperty AllowSelectionProperty = DependencyProperty.Register(nameof(AllowSelection), typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(true));

        static DesignerItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }

        public DesignerItem(Guid id)
        {
            Id = id;
        }

        public DesignerItem() : this(Guid.NewGuid())
        {
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            // Check item can be selected
            if (!AllowSelection) return;

            // Get parent / Must be of type DesignerCanvas
            if (VisualTreeHelper.GetParent(this) is not DesignerCanvas designer) return;

            // Update selection
            if (designer.ToolMode == Tool.Select)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    if (IsSelected)
                    {
                        designer.SelectionService.RemoveFromSelection(this);
                    }
                    else
                    {
                        designer.SelectionService.AddToSelection(this);
                    }
                else if (!IsSelected)
                {
                    designer.SelectionService.SelectItem(this);
                }
                Focus();
            }
        }
    }
}
