using System.ComponentModel;

namespace MenthaAssembly.Views
{
    public class CellBeforeEditingEventArgs(DataGridColumn Column, DataGridCell Cell) : HandledEventArgs
    {
        public DataGridColumn Column { get; } = Column;

        public DataGridCell Cell { get; } = Cell;

        public object DataContext { get; } = Cell.DataContext;

    }
}