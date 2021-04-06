using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DesignerCanvas.Controls;

namespace DesignerCanvas
{
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_ResizeDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    public class DesignerItem : ContentControl, ISelectable
    {
        public Guid Id { get; }

        public Guid ParentId
        {
            get { return (Guid)GetValue(ParentIdProperty); }
            set { SetValue(ParentIdProperty, value); }
        }
        public static readonly DependencyProperty ParentIdProperty = DependencyProperty.Register(nameof(ParentId), typeof(Guid), typeof(DesignerItem));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Allows to replace the default template for the DragThumb
        /// </summary>
        public static readonly DependencyProperty DragThumbTemplateProperty = DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetDragThumbTemplate(UIElement element) => (ControlTemplate)element.GetValue(DragThumbTemplateProperty);
        public static void SetDragThumbTemplate(UIElement element, ControlTemplate value) => element.SetValue(DragThumbTemplateProperty, value);

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
            
            var designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;
            
            // Update selection
            if (designer is not null)
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

            e.Handled = false;
        }
    }
}
