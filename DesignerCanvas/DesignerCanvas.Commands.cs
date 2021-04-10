using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;

namespace DesignerCanvas
{
    public partial class DesignerCanvas
    {
        public static RoutedCommand BringForward = new();
        public static RoutedCommand BringToFront = new();
        public static RoutedCommand SendBackward = new();
        public static RoutedCommand SendToBack = new();
        public static RoutedCommand SelectAll = new();
        public static RoutedCommand ToolModeChange = new();

        public DesignerCanvas()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, New_Executed));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, Cut_Executed, Cut_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, Copy_Executed, Copy_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste_Executed, Paste_CanExecute));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, Delete_Executed, Delete_CanExecute));
            CommandBindings.Add(new CommandBinding(BringForward, BringForward_Executed, Order_CanExecute));
            CommandBindings.Add(new CommandBinding(BringToFront, BringToFront_Executed, Order_CanExecute));
            CommandBindings.Add(new CommandBinding(SendBackward, SendBackward_Executed, Order_CanExecute));
            CommandBindings.Add(new CommandBinding(SendToBack, SendToBack_Executed, Order_CanExecute));
            CommandBindings.Add(new CommandBinding(SelectAll, SelectAll_Executed));
            CommandBindings.Add(new CommandBinding(ToolModeChange, ToolModeChange_Executed));
            SelectAll.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
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
            e.CanExecute = SelectionService.CurrentSelection.Count > 0;
        }

        #endregion

        #region Paste Command

        private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            XElement root = LoadSerializedDataFromClipBoard();

            if (root == null)
                return;

            // create DesignerItems
            var mappingOldToNewIDs = new Dictionary<Guid, Guid>();
            var newItems = new List<ISelectable>();
            var itemsXML = root.Elements("DesignerItems").Elements("DesignerItem");

            double offsetX = Double.Parse(root.Attribute("OffsetX").Value, CultureInfo.InvariantCulture);
            double offsetY = Double.Parse(root.Attribute("OffsetY").Value, CultureInfo.InvariantCulture);

            foreach (XElement itemXML in itemsXML)
            {
                var oldID = new Guid(itemXML.Element("Id").Value);
                var newID = Guid.NewGuid();
                mappingOldToNewIDs.Add(oldID, newID);
                DesignerItem item = DeserializeDesignerItem(itemXML, newID, offsetX, offsetY);
                this.Children.Add(item);
                newItems.Add(item);
            }

            // update group hierarchy
            SelectionService.ClearSelection();
            foreach (DesignerItem el in newItems)
            {
                if (el.ParentId != Guid.Empty)
                    el.ParentId = mappingOldToNewIDs[el.ParentId];
            }


            foreach (DesignerItem item in newItems)
            {
                if (item.ParentId == Guid.Empty)
                {
                    SelectionService.AddToSelection(item);
                }
            }

            DesignerCanvas.BringToFront.Execute(null, this);

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
            e.CanExecute = this.SelectionService.CurrentSelection.Count > 0;
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
            e.CanExecute = SelectionService.CurrentSelection.Count > 0;
        }

        #endregion

        #region BringForward Command

        private void BringForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            List<UIElement> ordered = (from item in SelectionService.CurrentSelection
                                       orderby Canvas.GetZIndex(item as UIElement) descending
                                       select item as UIElement).ToList();

            int count = this.Children.Count;

            for (int i = 0; i < ordered.Count; i++)
            {
                int currentIndex = Canvas.GetZIndex(ordered[i]);
                int newIndex = Math.Min(count - 1 - i, currentIndex + 1);
                if (currentIndex != newIndex)
                {
                    Canvas.SetZIndex(ordered[i], newIndex);
                    IEnumerable<UIElement> it = this.Children.OfType<UIElement>().Where(item => Canvas.GetZIndex(item) == newIndex);

                    foreach (UIElement elm in it)
                    {
                        if (elm != ordered[i])
                        {
                            Canvas.SetZIndex(elm, currentIndex);
                            break;
                        }
                    }
                }
            }
        }

        private void Order_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = SelectionService.CurrentSelection.Count() > 0;
            e.CanExecute = true;
        }

        #endregion

        #region BringToFront Command

        private void BringToFront_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            List<UIElement> selectionSorted = (from item in SelectionService.CurrentSelection
                                               orderby Canvas.GetZIndex(item as UIElement) ascending
                                               select item as UIElement).ToList();

            List<UIElement> childrenSorted = (from UIElement item in this.Children
                                              orderby Canvas.GetZIndex(item as UIElement) ascending
                                              select item as UIElement).ToList();

            int i = 0;
            int j = 0;
            foreach (UIElement item in childrenSorted)
            {
                if (selectionSorted.Contains(item))
                {
                    int idx = Canvas.GetZIndex(item);
                    Canvas.SetZIndex(item, childrenSorted.Count - selectionSorted.Count + j++);
                }
                else
                {
                    Canvas.SetZIndex(item, i++);
                }
            }
        }

        #endregion

        #region SendBackward Command

        private void SendBackward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            List<UIElement> ordered = (from item in SelectionService.CurrentSelection
                                       orderby Canvas.GetZIndex(item as UIElement) ascending
                                       select item as UIElement).ToList();

            int count = this.Children.Count;

            for (int i = 0; i < ordered.Count; i++)
            {
                int currentIndex = Canvas.GetZIndex(ordered[i]);
                int newIndex = Math.Max(i, currentIndex - 1);
                if (currentIndex != newIndex)
                {
                    Canvas.SetZIndex(ordered[i], newIndex);
                    IEnumerable<UIElement> it = this.Children.OfType<UIElement>().Where(item => Canvas.GetZIndex(item) == newIndex);

                    foreach (UIElement elm in it)
                    {
                        if (elm != ordered[i])
                        {
                            Canvas.SetZIndex(elm, currentIndex);
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
            List<UIElement> selectionSorted = (from item in SelectionService.CurrentSelection
                                               orderby Canvas.GetZIndex(item as UIElement) ascending
                                               select item as UIElement).ToList();

            List<UIElement> childrenSorted = (from UIElement item in this.Children
                                              orderby Canvas.GetZIndex(item as UIElement) ascending
                                              select item as UIElement).ToList();
            int i = 0;
            int j = 0;
            foreach (UIElement item in childrenSorted)
            {
                if (selectionSorted.Contains(item))
                {
                    int idx = Canvas.GetZIndex(item);
                    Canvas.SetZIndex(item, j++);

                }
                else
                {
                    Canvas.SetZIndex(item, selectionSorted.Count + i++);
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

        private static XElement LoadSerializedDataFromClipBoard()
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
                                                  new XElement("ParentId", item.ParentId),
                                                  new XElement("Content", contentXaml)
                                              )
                                   );
        }

        private static DesignerItem DeserializeDesignerItem(XElement itemXML, Guid id, double OffsetX, double OffsetY)
        {
            var item = new DesignerItem(id)
            {
                Width = Double.Parse(itemXML.Element("Width").Value, CultureInfo.InvariantCulture),
                Height = Double.Parse(itemXML.Element("Height").Value, CultureInfo.InvariantCulture),
                Opacity = Double.Parse(itemXML.Element("Opacity").Value),
                ParentId = new Guid(itemXML.Element("ParentId").Value)
            };
            SetLeft(item, Double.Parse(itemXML.Element("Left").Value, CultureInfo.InvariantCulture) + OffsetX);
            SetTop(item, Double.Parse(itemXML.Element("Top").Value, CultureInfo.InvariantCulture) + OffsetY);
            SetZIndex(item, Int32.Parse(itemXML.Element("zIndex").Value));
            item.Content = XamlReader.Load(XmlReader.Create(new StringReader(itemXML.Element("Content").Value)));
            return item;
        }

        private void CopyCurrentSelection()
        {
            var selectedDesignerItems = SelectionService.CurrentSelection.OfType<DesignerItem>();

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
            foreach (DesignerItem item in SelectionService.CurrentSelection.OfType<DesignerItem>())
            {
                this.Children.Remove(item);
            }

            SelectionService.ClearSelection();
            UpdateZIndex();
        }

        private void UpdateZIndex()
        {
            List<UIElement> ordered = (from UIElement item in this.Children
                                       orderby Canvas.GetZIndex(item as UIElement)
                                       select item as UIElement).ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                Canvas.SetZIndex(ordered[i], i);
            }
        }

        #endregion
    }
}
