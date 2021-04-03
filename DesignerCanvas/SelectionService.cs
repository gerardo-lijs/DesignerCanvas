using System.Collections.Generic;
using System.Linq;

namespace DesignerCanvas
{
    internal class SelectionService
    {
        private readonly DesignerCanvas _designerCanvas;

        internal List<ISelectable> CurrentSelection { get; } = new List<ISelectable>();

        public SelectionService(DesignerCanvas designerCanvas)
        {
            _designerCanvas = designerCanvas;
        }

        internal void SelectItem(ISelectable item)
        {
            ClearSelection();
            AddToSelection(item);
        }

        internal void AddToSelection(ISelectable item)
        {
            item.IsSelected = true;
            CurrentSelection.Add(item);
        }

        internal void RemoveFromSelection(ISelectable item)
        {
            item.IsSelected = false;
            CurrentSelection.Remove(item);
        }

        internal void ClearSelection()
        {
            CurrentSelection.ForEach(item => item.IsSelected = false);
            CurrentSelection.Clear();
        }

        internal void SelectAll()
        {
            ClearSelection();
            CurrentSelection.AddRange(_designerCanvas.Children.OfType<ISelectable>());
            CurrentSelection.ForEach(item => item.IsSelected = true);
        }
    }
}
