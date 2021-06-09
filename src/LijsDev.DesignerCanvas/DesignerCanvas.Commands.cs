using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;

namespace LijsDev.DesignerCanvas
{
    public partial class DesignerCanvas
    {
        public DesignerCanvas()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, Cut_Executed, Cut_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, Copy_Executed, Copy_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste_Executed, Paste_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, Delete_Executed, Delete_CanExecute));
            CommandBindings.Add(new CommandBinding(DesignerCanvasCommands.BringForward, BringForward_Executed, Order_CanExecute));
            CommandBindings.Add(new CommandBinding(DesignerCanvasCommands.BringToFront, BringToFront_Executed, Order_CanExecute));
            CommandBindings.Add(new CommandBinding(DesignerCanvasCommands.SendBackward, SendBackward_Executed, Order_CanExecute));
            CommandBindings.Add(new CommandBinding(DesignerCanvasCommands.SendToBack, SendToBack_Executed, Order_CanExecute));
            CommandBindings.Add(new CommandBinding(DesignerCanvasCommands.SelectAll, SelectAll_Executed));
            CommandBindings.Add(new CommandBinding(DesignerCanvasCommands.ToolModeChange, ToolModeChange_Executed));
            DesignerCanvasCommands.SelectAll.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
        }

        #region New Command

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Children.Clear();
            SelectionService.ClearSelection();
        }

        #endregion

        #region Copy Command

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CopyCurrentSelection();
        }

        private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !DisableClipboard && SelectedItems.Count > 0;
        }

        #endregion

        #region Paste Command

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var root = LoadSerializedDataFromClipBoard();
            if (root is null) return;

            // Create DesignerItems
            var mappingOldToNewIDs = new Dictionary<Guid, Guid>();
            var newItems = new List<IDesignerItem>();
            var itemsXML = root.Elements("DesignerItems").Elements("DesignerItem");

            var offsetX = double.Parse(root.Attribute("OffsetX").Value, CultureInfo.InvariantCulture);
            var offsetY = double.Parse(root.Attribute("OffsetY").Value, CultureInfo.InvariantCulture);

            foreach (XElement itemXML in itemsXML)
            {
                var oldID = new Guid(itemXML.Element("Id").Value);
                var newID = Guid.NewGuid();
                mappingOldToNewIDs.Add(oldID, newID);
                DesignerItem item = DeserializeDesignerItem(itemXML, newID, offsetX, offsetY);
                Children.Add(item);
                newItems.Add(item);
            }

            // Update selection
            SelectionService.ClearSelection();
            foreach (DesignerItem item in newItems)
            {
                SelectionService.AddToSelection(item);
            }

            DesignerCanvasCommands.BringToFront.Execute(null, this);

            // update paste offset
            root.Attribute("OffsetX").Value = (offsetX + 10).ToString();
            root.Attribute("OffsetY").Value = (offsetY + 10).ToString();
            Clipboard.Clear();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetData(DataFormats.Xaml, root);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(10);
            }
        }

        private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DisableClipboard)
            {
                e.CanExecute = false;
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    e.CanExecute = Clipboard.ContainsData(DataFormats.Xaml);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(10);
            }
        }

        #endregion

        #region Delete Command

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DeleteCurrentSelection();
        }

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedItems.Count > 0;
        }

        #endregion

        #region Cut Command

        private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CopyCurrentSelection();
            DeleteCurrentSelection();
        }

        private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !DisableClipboard && SelectedItems.Count > 0;
        }

        #endregion

        #region BringForward Command

        private void BringForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ordered = SelectedItems.Cast<UIElement>().OrderByDescending(GetZIndex).ToList();

            int count = Children.Count;

            for (int i = 0; i < ordered.Count; i++)
            {
                int currentIndex = GetZIndex(ordered[i]);
                int newIndex = Math.Min(count - 1 - i, currentIndex + 1);
                if (currentIndex != newIndex)
                {
                    SetZIndex(ordered[i], newIndex);
                    foreach (var elm in Children.OfType<UIElement>().Where(item => GetZIndex(item) == newIndex))
                    {
                        if (elm != ordered[i])
                        {
                            SetZIndex(elm, currentIndex);
                            break;
                        }
                    }
                }
            }
        }

        private void Order_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !DisableZOrder && SelectedItems.Count > 0;
        }

        #endregion

        #region BringToFront Command

        private void BringToFront_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var selectionSorted = SelectedItems.Cast<UIElement>().OrderBy(GetZIndex).ToList();
            var childrenSorted = Children.OfType<UIElement>().OrderBy(GetZIndex).ToList();

            var i = 0;
            var j = 0;
            foreach (var item in childrenSorted)
            {
                if (selectionSorted.Contains(item))
                {
                    SetZIndex(item, childrenSorted.Count - selectionSorted.Count + j++);
                }
                else
                {
                    SetZIndex(item, i++);
                }
            }
        }

        #endregion

        #region SendBackward Command

        private void SendBackward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ordered = SelectedItems.Cast<UIElement>().OrderBy(GetZIndex).ToList();

            int count = Children.Count;

            for (int i = 0; i < ordered.Count; i++)
            {
                int currentIndex = GetZIndex(ordered[i]);
                int newIndex = Math.Max(i, currentIndex - 1);
                if (currentIndex != newIndex)
                {
                    SetZIndex(ordered[i], newIndex);
                    IEnumerable<UIElement> it = Children.OfType<UIElement>().Where(item => GetZIndex(item) == newIndex);

                    foreach (UIElement elm in it)
                    {
                        if (elm != ordered[i])
                        {
                            SetZIndex(elm, currentIndex);
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region SendToBack Command

        private void SendToBack_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var selectionSorted = SelectedItems.Cast<UIElement>().OrderBy(GetZIndex).ToList();
            var childrenSorted = Children.OfType<UIElement>().OrderBy(GetZIndex).ToList();

            var i = 0;
            var j = 0;
            foreach (UIElement item in childrenSorted)
            {
                if (selectionSorted.Contains(item))
                {
                    SetZIndex(item, j++);
                }
                else
                {
                    SetZIndex(item, selectionSorted.Count + i++);
                }
            }
        }

        #endregion

        #region SelectAll Command

        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SelectionService.SelectAll();
        }

        #endregion

        private void ToolModeChange_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is Tool tool)
            {
                ToolMode = tool;
            }
        }

        #region Helper Methods

        private static XElement? LoadSerializedDataFromClipBoard()
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (Clipboard.ContainsData(DataFormats.Xaml))
                    {
                        var clipboardData = Clipboard.GetData(DataFormats.Xaml) as string;

                        if (String.IsNullOrEmpty(clipboardData)) return null;
                        try
                        {
                            return XElement.Load(new StringReader(clipboardData));
                        }
                        catch
                        {
                            return null;
                        }
                    }

                    return null;
                }
                catch { }
                System.Threading.Thread.Sleep(10);
            }

            return null;
        }

        private static XElement SerializeDesignerItems(IEnumerable<DesignerItem> designerItems)
        {
            return new XElement("DesignerItems",
                                       from item in designerItems
                                       let contentXaml = XamlWriter.Save(item.Content)
                                       select new XElement("DesignerItem",
                                                  new XElement("Left", GetLeft(item)),
                                                  new XElement("Top", GetTop(item)),
                                                  new XElement("Width", item.Width),
                                                  new XElement("Height", item.Height),
                                                  new XElement("Id", item.Id),
                                                  new XElement("zIndex", GetZIndex(item)),
                                                  new XElement("Opacity", item.Opacity),
                                                  new XElement("Content", contentXaml)
                                              )
                                   );
        }

        private static DesignerItem DeserializeDesignerItem(XElement itemXML, Guid id, double OffsetX, double OffsetY)
        {
            var item = new DesignerItem(id)
            {
                Width = double.Parse(itemXML.Element("Width").Value, CultureInfo.InvariantCulture),
                Height = double.Parse(itemXML.Element("Height").Value, CultureInfo.InvariantCulture),
                Opacity = double.Parse(itemXML.Element("Opacity").Value),
            };
            SetLeft(item, double.Parse(itemXML.Element("Left").Value, CultureInfo.InvariantCulture) + OffsetX);
            SetTop(item, double.Parse(itemXML.Element("Top").Value, CultureInfo.InvariantCulture) + OffsetY);
            SetZIndex(item, int.Parse(itemXML.Element("zIndex").Value));
            item.Content = XamlReader.Load(XmlReader.Create(new StringReader(itemXML.Element("Content").Value)));
            return item;
        }

        private void CopyCurrentSelection()
        {
            var selectedDesignerItems = SelectedItems.OfType<DesignerItem>();

            var designerItemsXML = SerializeDesignerItems(selectedDesignerItems);

            var root = new XElement("Root");
            root.Add(designerItemsXML);

            root.Add(new XAttribute("OffsetX", 10));
            root.Add(new XAttribute("OffsetY", 10));

            Clipboard.Clear();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetData(DataFormats.Xaml, root);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(10);
            }
        }

        private void DeleteCurrentSelection()
        {
            foreach (var item in SelectedItems.OfType<DesignerItem>().ToList())
            {
                Children.Remove(item);
                SelectionService.RemoveFromSelection(item);
            }
            RaiseItemsDeletedEvent();
            UpdateZIndex();
        }

        private void UpdateZIndex()
        {
            var ordered = Children.OfType<UIElement>().OrderBy(GetZIndex).ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                SetZIndex(ordered[i], i);
            }
        }

        #endregion
    }
}
