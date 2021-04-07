using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesignerCanvas.Controls;

namespace DesignerCanvas
{
    [TemplatePart(Name = DragThumbPart, Type = typeof(DragThumb))]
    [TemplatePart(Name = ResizeDecoratorPart, Type = typeof(Control))]
    [TemplatePart(Name = ContentPresenterPart, Type = typeof(ContentPresenter))]
    public class DesignerItem : ContentControl, ISelectable
    {
        private const string DragThumbPart = "PART_DragThumb";
        private const string ResizeDecoratorPart = "PART_ResizeDecorator";
        private const string ContentPresenterPart = "PART_ContentPresenter";

        private Control _resizeDecorator;

        public Guid Id { get; }

        public Guid ParentId
        {
            get => (Guid)GetValue(ParentIdProperty);
            set => SetValue(ParentIdProperty, value);
        }
        public static readonly DependencyProperty ParentIdProperty = DependencyProperty.Register(nameof(ParentId), typeof(Guid), typeof(DesignerItem));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));

        static DesignerItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _resizeDecorator = GetTemplateChild(ResizeDecoratorPart) as Control;
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
