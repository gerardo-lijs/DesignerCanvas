using System;
using System.Windows.Controls;

namespace LijsDev.DesignerCanvas
{
    public interface IDesignerItem
    {
        Guid Id { get; }
        /// <summary>
        /// Returns true if item is currently selected by the user.
        /// </summary>
        bool IsSelected { get; set; }
        /// <summary>
        /// If true, item can be selected by the user.
        /// </summary>
        bool AllowSelection { get; set; }
    }
}
