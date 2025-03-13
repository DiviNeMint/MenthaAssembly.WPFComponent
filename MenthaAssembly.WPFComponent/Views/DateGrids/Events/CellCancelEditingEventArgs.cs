using System.ComponentModel;
using System.Windows;

namespace MenthaAssembly.Views
{
    public class CellCancelEditingEventArgs(DataGridColumn Column, FrameworkElement EditingElement, object UneditedValue) : HandledEventArgs
    {
        public DataGridColumn Column { get; } = Column;

        public FrameworkElement EditingElement { get; } = EditingElement;

        public object DataContext { get; } = EditingElement.DataContext;

        public object UneditedValue { get; } = UneditedValue;

    }
}