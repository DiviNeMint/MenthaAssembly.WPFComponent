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

    }
}