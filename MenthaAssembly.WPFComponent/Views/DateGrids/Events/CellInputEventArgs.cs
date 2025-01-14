using System;
using System.Windows.Input;

namespace MenthaAssembly.Views
{
    public sealed class CellInputEventArgs(DataGridCell Cell, InputEventArgs TriggerEventArgs, object DataContext) : EventArgs
    {
        public InputEventArgs TriggerEventArgs { get; } = TriggerEventArgs;

        public DataGridCell Cell { get; } = Cell;

        public object DataContext { get; } = DataContext;

        public bool Handled
        {
            get => TriggerEventArgs.Handled;
            set => TriggerEventArgs.Handled = value;
        }

        public bool BeginEdit { set; get; }

    }
}