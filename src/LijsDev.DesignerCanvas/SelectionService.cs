using System.Linq;

namespace LijsDev.DesignerCanvas
{
    internal class SelectionService
    {
        private readonly DesignerCanvas _designerCanvas;

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
            _designerCanvas.SelectedItems.Add(item);
            _designerCanvas.RaiseSelectionChangedEvent(_designerCanvas.SelectedItems);
        }

        internal void RemoveFromSelection(IDesignerItem item)
        {
            item.IsSelected = false;
            _designerCanvas.SelectedItems.Remove(item);
            _designerCanvas.RaiseSelectionChangedEvent(_designerCanvas.SelectedItems);
        }

        internal void ClearSelection(bool dontRaiseSelectionChangedEvent = false)
        {
            if (_designerCanvas.SelectedItems.Count == 0) return;

            _designerCanvas.SelectedItems.ForEach(item => item.IsSelected = false);
            _designerCanvas.SelectedItems.Clear();
            if (!dontRaiseSelectionChangedEvent) _designerCanvas.RaiseSelectionChangedEvent(_designerCanvas.SelectedItems);
        }

        internal void SelectAll()
        {
            ClearSelection();
            _designerCanvas.SelectedItems.AddRange(_designerCanvas.Children.OfType<IDesignerItem>());
            _designerCanvas.SelectedItems.ForEach(item => item.IsSelected = true);
            _designerCanvas.RaiseSelectionChangedEvent(_designerCanvas.SelectedItems);
        }
    }
}
