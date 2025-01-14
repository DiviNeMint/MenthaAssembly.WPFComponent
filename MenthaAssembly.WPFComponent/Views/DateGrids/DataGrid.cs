using MenthaAssembly.MarkupExtensions;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class DataGrid : System.Windows.Controls.DataGrid
    {
        public static readonly RoutedEvent CurrentCellChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(CurrentCellChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<DataGridCell>), typeof(DataGrid));
        public new event RoutedPropertyChangedEventHandler<DataGridCell> CurrentCellChanged
        {
            add => AddHandler(CurrentCellChangedEvent, value);
            remove => RemoveHandler(CurrentCellChangedEvent, value);
        }

        private static readonly PropertyInfo HasCellValidationErrorProperty;
        public bool HasCellValidationError
        {
            get => HasCellValidationErrorProperty?.GetValue(this) is true;
            protected set => HasCellValidationErrorProperty?.SetValue(this, value);
        }

        static DataGrid()
        {
            Type ThisType = typeof(DataGrid);
            Brush GridLinesBrush = new SolidColorBrush(Color.FromRgb(0xD5, 0xD5, 0xD5));
            VerticalGridLinesBrushProperty.OverrideMetadata(ThisType, new FrameworkPropertyMetadata(GridLinesBrush, OnNotifyGridLinePropertyChanged));
            HorizontalGridLinesBrushProperty.OverrideMetadata(ThisType, new FrameworkPropertyMetadata(GridLinesBrush, OnNotifyGridLinePropertyChanged));

            static void OnNotifyGridLinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                // Clear out and regenerate all containers.  We do this so that we don't have to propagate this notification
                // to containers that are currently on the recycle queue -- doing so costs us perf on every scroll.  We don't
                // care about the time spent on a GridLine change since it'll be a very rare occurance.
                //
                // ItemsControl.OnItemTemplateChanged calls the internal ItemContainerGenerator.Refresh() method, which
                // clears out all containers and notifies the panel.  The fact we're passing in two null templates is ignored.
                if (e.OldValue != e.NewValue)
                    ((DataGrid)d).OnItemTemplateChanged(null, null);
            }

            Type BaseType = typeof(System.Windows.Controls.DataGrid);

            // CurrentCellContainer
            BaseType.TryGetInternalProperty(nameof(CurrentCellContainer), out CurrentCellContainerProperty);

            // NewItemPlaceholder
            BaseType.TryGetStaticInternalPropertyValue(nameof(CollectionView.NewItemPlaceholder), out DataGridNewItemPlaceholder);

            // HasCellValidationError
            BaseType.TryGetInternalProperty(nameof(HasCellValidationError), out HasCellValidationErrorProperty);

            // Clipboard handling
            CommandManager.RegisterClassCommandBinding(ThisType, new CommandBinding(ApplicationCommands.Paste, new ExecutedRoutedEventHandler(OnExecutedPaste), new CanExecuteRoutedEventHandler(OnCanExecutePaste)));
            static void OnExecutedPaste(object sender, ExecutedRoutedEventArgs e)
            {
                if (sender is DataGrid This)
                    This.OnExecutedPaste(e);
            }
            static void OnCanExecutePaste(object sender, CanExecuteRoutedEventArgs e)
            {
                if (sender is DataGrid This)
                    This.OnCanExecutePaste(e);
            }
        }

        protected override void OnCanExecuteBeginEdit(CanExecuteRoutedEventArgs e)
        {
            base.OnCanExecuteBeginEdit(e);

            if (e.CanExecute &&
                e.OriginalSource is DataGridCell Cell &&
                Cell.Column is DataGridColumn Column)
            {
                // AllowEditingMode
                if (!Column.AllowEditingMode)
                {
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                // EditableNewItemPlaceholder
                if (IsNewItemPlaceholder(Cell.DataContext))
                {
                    if (!Column.EditableNewItemPlaceholder)
                    {
                        e.CanExecute = false;
                        e.Handled = true;
                        return;
                    }
                }
                else
                {
                    // Event
                    CellEditingEventArgs Args = new(Column, Cell);
                    Column.RaiseBeforeEditing(Args);

                    if (Args.Handled)
                    {
                        e.CanExecute = false;
                        e.Handled = true;
                        return;
                    }
                }
            }

        }

        protected override void OnCanExecuteCancelEdit(CanExecuteRoutedEventArgs e)
        {
            IEditableCollectionView View = Items;

            object CurrentItem = this.CurrentItem;
            if (View.IsAddingNew && View.CurrentAddItem == CurrentItem &&
                this.GetSelectedRow() is DataGridRow Row &&
                Row.BindingGroup is BindingGroup Group)
            {
                if (Group.HasValidationError)
                {
                    // Set the Property【HasCellValidationError】to false
                    // so that other operations can be performed after deleting the new item.
                    if (HasCellValidationErrorProperty != null)
                    {
                        HasCellValidationError = false;
                        e.CanExecute = true;
                        e.Handled = true;
                    }
                }
                else
                {
                    // Set IsEditing to false so that new items are deleted when the cancel command is executed.
                    if (!Group.Validate() &&
                        this.GetSelectedCell() is DataGridCell Cell)
                    {
                        Cell.IsEditing = false;
                        e.CanExecute = true;
                        e.Handled = true;
                    }
                }

                return;
            }

            base.OnCanExecuteCancelEdit(e);
        }

        protected override void OnCanExecuteCommitEdit(CanExecuteRoutedEventArgs e)
        {
            // Because after canceling the new data, the data will not be deleted immediately.
            // At this time, triggering Commit through Enter will not trigger any verification, so it is foolproof here.
            IEditableCollectionView View = Items;
            if (View.IsAddingNew &&
                e.OriginalSource is DataGridCell Cell &&
                Cell.BindingGroup is BindingGroup Group)
                Group.Validate();

            base.OnCanExecuteCommitEdit(e);
        }

        protected virtual void OnCanExecutePaste(CanExecuteRoutedEventArgs e)
            => e.CanExecute = CurrentCell.IsValid;
        protected virtual void OnExecutedPaste(ExecutedRoutedEventArgs e)
        {
            if (this.CurrentItem is object CurrentItem)
            {
                string[] PastingDatas = Clipboard.GetText().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);

                IEditableCollectionView View = Items;
                int MaxRow = Items.Count - 1,
                    StartRow = IsNewItemPlaceholder(CurrentItem) ? MaxRow : Items.IndexOf(CurrentItem),
                    EndRow = StartRow + PastingDatas.Length - 1;

                // Checks the data that can be overwritten
                if (View.CanAddNew)
                {
                    for (int j = MaxRow; j <= EndRow; j++)
                    {
                        View.AddNew();
                        View.CommitNew();
                    }
                }
                else
                {
                    EndRow = Math.Min(EndRow, MaxRow);
                }

                // SetValue
                int StartColumn = Columns.IndexOf(CurrentColumn);
                for (int j = StartRow; j <= EndRow; j++)
                {
                    object Item = Items[j];

                    string[] ColumnDatas = PastingDatas[j - StartRow].Split(',', '\t', '|');
                    for (int i = 0; i < ColumnDatas.Length; i++)
                        Columns[StartColumn + i].OnPastingCellClipboardContent(Item, ColumnDatas[i]);
                }

                // Validate
                for (int j = StartRow; j <= EndRow; j++)
                    if (this.GetRow(j) is DataGridRow Row &&
                        Row.BindingGroup is BindingGroup Group)
                        Group.Validate();
            }
        }

        private DataGridCell CurrentCellContainer;
        private static readonly PropertyInfo CurrentCellContainerProperty;
        protected override void OnCurrentCellChanged(EventArgs e)
        {
            base.OnCurrentCellChanged(e);

            DataGridCell New = CurrentCellContainerProperty.GetValue(this) as DataGridCell;
            RaiseEvent(new RoutedPropertyChangedEventArgs<DataGridCell>(CurrentCellContainer, New, CurrentCellChangedEvent));
            CurrentCellContainer = New;
        }

        protected override DependencyObject GetContainerForItemOverride()
            => new DataGridRow();

        private static readonly object DataGridNewItemPlaceholder;
        public static bool IsNewItemPlaceholder(object Data)
            => Data != null && (Data == CollectionView.NewItemPlaceholder || Data == DataGridNewItemPlaceholder);

    }
}