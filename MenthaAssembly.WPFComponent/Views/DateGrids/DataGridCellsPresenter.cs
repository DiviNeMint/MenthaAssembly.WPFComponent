using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace MenthaAssembly.Views
{
    public class DataGridCellsPresenter : System.Windows.Controls.Primitives.DataGridCellsPresenter
    {
        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is DataGridCell;

        protected override DependencyObject GetContainerForItemOverride()
            => new DataGridCell();

        private bool HasCellLayoutSignature;
        private Size LastArrangeSize;
        private CellLayoutSnapshot[] LastCellLayoutSnapshots = [];
        protected override Size ArrangeOverride(Size FinalSize)
        {
            Size Result = base.ArrangeOverride(FinalSize);
            if (TemplatedParent is DataGridRow Row &&
                Row.IsHighlighting &&
                CellLayoutSignatureChanged(FinalSize))
            {
                Row.InvalidateHighlighting();
            }

            return Result;
        }

        private bool CellLayoutSignatureChanged(Size ArrangeSize)
        {
            List<CellLayoutSnapshot> Snapshots = [];
            for (int i = 0; i < Items.Count; i++)
                if (ItemContainerGenerator.ContainerFromIndex(i) is DataGridCell Cell)
                    Snapshots.Add(new CellLayoutSnapshot(Cell.Column, Cell.ActualWidth));

            bool Changed = !HasCellLayoutSignature ||
                           !AreClose(LastArrangeSize.Width, ArrangeSize.Width) ||
                           !AreClose(LastArrangeSize.Height, ArrangeSize.Height) ||
                           LastCellLayoutSnapshots.Length != Snapshots.Count;
            if (!Changed)
            {
                for (int i = 0; i < Snapshots.Count; i++)
                {
                    CellLayoutSnapshot LastSnapshot = LastCellLayoutSnapshots[i],
                                       Snapshot = Snapshots[i];
                    if (!ReferenceEquals(LastSnapshot.Column, Snapshot.Column) ||
                        !AreClose(LastSnapshot.Width, Snapshot.Width))
                    {
                        Changed = true;
                        break;
                    }
                }
            }

            HasCellLayoutSignature = true;
            LastArrangeSize = ArrangeSize;
            LastCellLayoutSnapshots = [.. Snapshots];
            return Changed;
        }

        private bool AreClose(double Left, double Right)
            => Math.Abs(Left - Right) < 0.001d;

        private readonly Dictionary<DependencyObject, Action> DetachActionTable = [];
        protected override void PrepareContainerForItemOverride(DependencyObject Element, object DataContext)
        {
            base.PrepareContainerForItemOverride(Element, DataContext);
            if (Element is DataGridCell Cell &&
                Cell.Column is DataGridColumn Column &&
                DataContext is INotifyPropertyChanged Notifier)
            {
                void OnNotifierPropertyChanged(object sender, PropertyChangedEventArgs e)
                {
                    string Name = e.PropertyName;
                    if (string.IsNullOrEmpty(Name) ||
                        Column.DependencyMemberPath.Contains(Name))
                        Cell.BuildVisualTree();
                }

                Notifier.PropertyChanged += OnNotifierPropertyChanged;

                if (DetachActionTable.TryGetValue(Element, out Action Detach))
                {
                    DetachActionTable.Remove(Element);
                    Detach.Invoke();
                }

                DetachActionTable.Add(Element, () => Notifier.PropertyChanged -= OnNotifierPropertyChanged);

                // When the DataGridCell is being prepared, if there are any pending invalid cells for the corresponding column and item, force a validation error on the cell.
                if (Column.DataGridOwner?.PendingInvalidCells?.Any(i => i.Column == Column && i.Item == DataContext) is true)
                    Cell.ForceChildValidationError();
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject Element, object item)
        {
            base.ClearContainerForItemOverride(Element, item);

            if (DetachActionTable.TryGetValue(Element, out Action Action))
            {
                DetachActionTable.Remove(Element);
                Action.Invoke();
            }
        }

        private readonly struct CellLayoutSnapshot(object Column, double Width)
        {
            public object Column { get; } = Column;

            public double Width { get; } = Width;

        }

    }
}
