using System.Collections.Generic;
using System.Linq;

namespace LijsDev.DesignerCanvas
{
    internal class SelectionService
    {
        private readonly DesignerCanvas _designerCanvas;

        internal List<IDesignerItem> CurrentSelection { get; } = new();

        public SelectionService(DesignerCanvas designerCanvas)
        {
            _designerCanvas = designerCanvas;
        }

        internal void SelectItem(IDesignerItem item)
        {
            ClearSelection(dontRaiseSelectionChangedEvent: true);
            AddToSelection(item);
        }

        internal void AddToSelection(IDesignerItem item)
        {
            item.IsSelected = true;
            CurrentSelection.Add(item);
            _designerCanvas.RaiseSelectionChangedEvent(CurrentSelection);
        }

        internal void RemoveFromSelection(IDesignerItem item)
        {
            item.IsSelected = false;
            CurrentSelection.Remove(item);
            _designerCanvas.RaiseSelectionChangedEvent(CurrentSelection);
        }

        internal void ClearSelection(bool dontRaiseSelectionChangedEvent = false)
        {
            if (CurrentSelection.Count == 0) return;

            CurrentSelection.ForEach(item => item.IsSelected = false);
            CurrentSelection.Clear();
            if (!dontRaiseSelectionChangedEvent) _designerCanvas.RaiseSelectionChangedEvent(CurrentSelection);
        }

        internal void SelectAll()
        {
            ClearSelection();
            CurrentSelection.AddRange(_designerCanvas.Children.OfType<IDesignerItem>());
            CurrentSelection.ForEach(item => item.IsSelected = true);
            _designerCanvas.RaiseSelectionChangedEvent(CurrentSelection);
        }
    }
}
