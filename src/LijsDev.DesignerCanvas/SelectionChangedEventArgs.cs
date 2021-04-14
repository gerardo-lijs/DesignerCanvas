using System;
using System.Collections.Generic;
using System.Windows;

namespace LijsDev.DesignerCanvas
{
    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e);
    public class SelectionChangedEventArgs : RoutedEventArgs
    {
        public List<IDesignerItem> Items { get; }

        public SelectionChangedEventArgs(RoutedEvent routedEvent, List<IDesignerItem> items) : base(routedEvent)
        {
            Items = items;
        }
    }
}
