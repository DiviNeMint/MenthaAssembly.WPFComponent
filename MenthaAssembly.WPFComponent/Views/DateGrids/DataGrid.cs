using MenthaAssembly.MarkupExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Views
{
    public class DataGrid : System.Windows.Controls.DataGrid
    {
        private static readonly RoutedEvent ProgrammaticBeginEditEvent =
            EventManager.RegisterRoutedEvent(nameof(ProgrammaticBeginEditEvent), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DataGrid));

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

        public static readonly DependencyProperty CanUserFilterColumnsProperty =
              DependencyProperty.Register(nameof(CanUserFilterColumns), typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true, OnCanUserFilterColumnsChanged));
        public bool CanUserFilterColumns
        {
            get => (bool)GetValue(CanUserFilterColumnsProperty);
            set => SetValue(CanUserFilterColumnsProperty, value);
        }

        public static readonly DependencyProperty ColumnFilterViewStyleProperty =
              DependencyProperty.Register(nameof(ColumnFilterViewStyle), typeof(Style), typeof(DataGrid), new FrameworkPropertyMetadata(null));
        public Style ColumnFilterViewStyle
        {
            get => (Style)GetValue(ColumnFilterViewStyleProperty);
            set => SetValue(ColumnFilterViewStyleProperty, value);
        }

        public bool IsExternalColumnFilterUsed
            => GetCollectionView() is ICollectionView View && IsExternalFilterUsed(View);

        static DataGrid()
        {
            Type ThisType = typeof(DataGrid);
            Brush GridLinesBrush = new SolidColorBrush(Color.FromRgb(0xD5, 0xD5, 0xD5));
            VerticalGridLinesBrushProperty.OverrideMetadata(ThisType, new FrameworkPropertyMetadata(GridLinesBrush, OnNotifyGridLinePropertyChanged));
            HorizontalGridLinesBrushProperty.OverrideMetadata(ThisType, new FrameworkPropertyMetadata(GridLinesBrush, OnNotifyGridLinePropertyChanged));
            CanUserAddRowsProperty.OverrideMetadata(ThisType, new FrameworkPropertyMetadata(true, null, CoerceCanUserAddRows));

            Type BaseType = typeof(System.Windows.Controls.DataGrid);

            // CurrentCellContainer
            BaseType.TryGetInternalProperty(nameof(CurrentCellContainer), out CurrentCellContainerProperty);

            // NewItemPlaceholder
            BaseType.TryGetStaticInternalPropertyValue(nameof(CollectionView.NewItemPlaceholder), out DataGridNewItemPlaceholder);

            // HasCellValidationError
            BaseType.TryGetInternalProperty(nameof(HasCellValidationError), out HasCellValidationErrorProperty);

            // Clipboard handling
            CommandManager.RegisterClassCommandBinding(ThisType, new CommandBinding(ApplicationCommands.Paste, new ExecutedRoutedEventHandler(OnExecutedPaste), new CanExecuteRoutedEventHandler(OnCanExecutePaste)));
        }

        private readonly Predicate<object> ColumnFilterPredicate;
        public DataGrid()
        {
            ColumnFilterPredicate = OnColumnFilter;
            Columns.CollectionChanged += (s, e) => CoerceColumnCanUserFilter();
        }

        private static void OnNotifyGridLinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
        private static object CoerceCanUserAddRows(DependencyObject d, object baseValue)
        {
            DataGrid grid = (DataGrid)d;

            if (!(bool)baseValue || grid.IsReadOnly || !grid.IsEnabled || grid.Items is not IEditableCollectionView view)
                return false;

            if (!view.CanAddNew)
            {
                IEditableCollectionViewAddNewItem addNewItem = grid.Items;
                if (addNewItem?.CanAddNewItem != true)
                    return false;
            }

            return true;
        }

        private static void OnExecutedPaste(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DataGrid This)
                This.OnExecutedPaste(e);
        }
        private static void OnCanExecutePaste(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is DataGrid This)
                This.OnCanExecutePaste(e);
        }
        private static void OnCanUserFilterColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((DataGrid)d).CoerceColumnCanUserFilter();

        protected override void OnCanExecuteBeginEdit(CanExecuteRoutedEventArgs e)
        {
            base.OnCanExecuteBeginEdit(e);

            if (e.CanExecute &&
                LastEditingCellContainer?.HasChildValidationError is true)
            {
                if (e.OriginalSource is not DataGridCell EditingCell)
                {
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                DataGridCellInfo CellInfo = new(EditingCell.DataContext, EditingCell.Column);
                if (!ContainsPendingInvalidCell(CellInfo) ||
                    !EditingCell.HasChildValidationError ||
                    IsSameCell(CellInfo, LastEditingCell))
                {
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }
            }

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
                    CellBeforeEditingEventArgs Args = new(Column, Cell);
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
        protected override void OnExecutedBeginEdit(ExecutedRoutedEventArgs e)
        {
            if (IsNewItemPlaceholder(CurrentItem))
            {
                AddingNewItemEventArgs args = new();
                OnAddingNewItem(args);

                if (args.NewItem != null)
                {
                    IEditableCollectionViewAddNewItem editableView = Items;
                    if (editableView?.CanAddNewItem == true)
                    {
                        object newItem = editableView.AddNewItem(args.NewItem);

                        System.Windows.Controls.DataGridColumn column = CurrentCell.Column ?? (Columns.Count > 0 ? Columns[0] : null);
                        if (column != null)
                        {
                            SelectedItem = newItem;
                            CurrentCell = new DataGridCellInfo(newItem, column);
                        }

                        UpdateLayout();

                        base.OnExecutedBeginEdit(e);
                        if (CurrentCellContainerProperty.GetValue(this) is DataGridCell Cell &&
                            Cell.IsEditing)
                        {
                            LastEditingCell = new DataGridCellInfo(Cell.DataContext, Cell.Column);
                            LastEditingCellContainer = Cell;
                        }

                        return;
                    }
                }

                // CanAddNew = false 且無 CreatingNewItem 提供物件，靜默取消以避免例外。
                if (!(Items as IEditableCollectionView)!.CanAddNew)
                    return;
            }

            base.OnExecutedBeginEdit(e);
            if (CurrentCellContainerProperty.GetValue(this) is DataGridCell EditingCell &&
                EditingCell.IsEditing)
            {
                LastEditingCell = new DataGridCellInfo(EditingCell.DataContext, EditingCell.Column);
                LastEditingCellContainer = EditingCell;
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
        protected override void OnExecutedCommitEdit(ExecutedRoutedEventArgs e)
        {
            DataGridCellInfo EditingCell = CurrentCell;
            base.OnExecutedCommitEdit(e);

            if (PendingInvalidCells.Count == 0)
                return;

            Dispatcher.BeginInvoke(new Action(() => MoveToNextPendingInvalidCell(EditingCell)), DispatcherPriority.Background);
        }

        protected override void OnCanExecuteDelete(CanExecuteRoutedEventArgs e)
        {
            base.OnCanExecuteDelete(e);

            if (e.CanExecute)
                return;

            if (!CanUserDeleteRows ||
                CurrentCellContainer?.IsEditing is true ||
                Items is not IEditableCollectionView View ||
                !View.CanRemove)
                return;

            e.CanExecute = GetSelectedCellItems().Any();
            e.Handled = true;
        }
        protected override void OnExecutedDelete(ExecutedRoutedEventArgs e)
        {
            if (SelectedItems.Count > 0)
            {
                base.OnExecutedDelete(e);
                return;
            }

            if (Items is not IEditableCollectionView View)
                return;

            List<object> DeleteItems = GetSelectedCellItems().ToList();
            if (DeleteItems.Count == 0)
                return;

            int AnchorIndex = DeleteItems.Select(Items.IndexOf)
                                         .Where(i => 0 <= i)
                                         .DefaultIfEmpty(Items.IndexOf(DeleteItems[0]))
                                         .Min();
            System.Windows.Controls.DataGridColumn Column = CurrentCell.Column ?? Columns.FirstOrDefault();

            foreach (object Item in DeleteItems)
                View.Remove(Item);

            MoveCurrentCellAfterDelete(AnchorIndex, Column);
            e.Handled = true;
        }
        private IEnumerable<object> GetSelectedCellItems()
        {
            HashSet<object> Items = [];
            foreach (DataGridCellInfo Cell in SelectedCells)
                if (Cell.Item is object Item &&
                    !IsNewItemPlaceholder(Item) &&
                    Items.Add(Item))
                    yield return Item;

            if (Items.Count == 0 &&
                CurrentItem is object Current &&
                !IsNewItemPlaceholder(Current))
                yield return Current;
        }
        private void MoveCurrentCellAfterDelete(int AnchorIndex, System.Windows.Controls.DataGridColumn Column)
        {
            SelectedItems.Clear();
            SelectedCells.Clear();

            if (Column is null ||
                Items.Count == 0)
            {
                CurrentCell = default;
                return;
            }

            int TargetIndex = Math.Min(AnchorIndex, Items.Count - 1);
            while (0 <= TargetIndex &&
                   IsNewItemPlaceholder(Items[TargetIndex]))
                TargetIndex--;

            if (TargetIndex < 0)
            {
                CurrentCell = default;
                return;
            }

            object TargetItem = Items[TargetIndex];
            DataGridCellInfo Cell = new(TargetItem, Column);
            CurrentCell = Cell;
            SelectedCells.Add(Cell);
            ScrollIntoView(TargetItem, Column);
        }

        protected virtual void OnCanExecutePaste(CanExecuteRoutedEventArgs e)
            => e.CanExecute = CurrentCell.IsValid;
        protected virtual void OnExecutedPaste(ExecutedRoutedEventArgs e)
        {
            if (this.CurrentItem is not object CurrentItem)
                return;

            string[] PastingDatas = Clipboard.GetText().Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            if (PastingDatas.Length == 0)
                return;

            int MaxRow = Items.Count - 1,
                StartRow = IsNewItemPlaceholder(CurrentItem) ? MaxRow : Items.IndexOf(CurrentItem),
                EndRow = StartRow + PastingDatas.Length - 1;

            if (StartRow < 0)
                return;

            IEditableCollectionViewAddNewItem EditableView = Items;
            
            List<PastingRow> Rows = [];
            for (int i = StartRow; i <= EndRow; i++)
            {
                if (i < Items.Count &&
                    !IsNewItemPlaceholder(Items[i]))
                {
                    Rows.Add(new PastingRow(Items[i], false));
                    continue;
                }

                if (!CanUserAddRows ||
                    EditableView?.CanAddNewItem != true)
                    break;

                AddingNewItemEventArgs Args = new();
                OnAddingNewItem(Args);

                if (Args.NewItem is not object NewItem)
                    break;

                Rows.Add(new PastingRow(NewItem, true));
            }

            if (Rows.Count == 0)
                return;

            int StartColumn = Columns.IndexOf(CurrentColumn);
            for (int j = 0; j < Rows.Count; j++)
            {
                PastingRow Row = Rows[j];
                string[] ColumnDatas = DataGridHelper.ParsePastingColumnDatas(PastingDatas[j]);
                int ColumnCount = Math.Min(ColumnDatas.Length, Columns.Count - StartColumn);
                Row.PastedColumnCount = ColumnCount;
                for (int i = 0; i < ColumnCount; i++)
                {
                    if (Columns[StartColumn + i] is DataGridColumn Column)
                        Row.Paste(Column, ColumnDatas[i]);
                }
            }

            foreach (PastingRow Row in Rows)
            {
                if (!Row.IsDetached)
                    continue;

                EditableView.AddNewItem(Row.Item);
                EditableView.CommitNew();
            }

            PendingInvalidCells.Clear();
            LastEditingCell = default;
            LastEditingCellContainer = null;
            for (int j = 0; j < Rows.Count; j++)
            {
                if (this.GetRow(StartRow + j) is not DataGridRow Row)
                    continue;

                List<DataGridCell> PastedCells = [];
                for (int i = 0; i < Rows[j].PastedColumnCount; i++)
                {
                    ScrollIntoView(Row.Item, Columns[StartColumn + i]);
                    UpdateLayout();
                    if (this.GetCell(Row, StartColumn + i) is DataGridCell Cell)
                        PastedCells.Add(Cell);
                }

                if (Row.BindingGroup is BindingGroup Group &&
                    !Group.Validate())
                    PendingInvalidCells.AddRange(GetInvalidCells(PastedCells));
            }

            RemoveDuplicatePendingInvalidCells();

            if (GetNextPendingInvalidCell(default) is DataGridCell InvalidCell)
                BeginEditInvalidCell(InvalidCell);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (ColumnHeaderStyle is null)
                SetResourceReference(ColumnHeaderStyleProperty, DataGridColumn.DefaultStyleKey);

            if (ColumnFilterViewStyle is null)
                SetResourceReference(ColumnFilterViewStyleProperty, DataGridColumnFilterView.DefaultStyleKey);
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (PendingInvalidCells.Count > 0 &&
                LastEditingCellContainer is DataGridCell EditingCell &&
                EditingCell.IsEditing &&
                IsSameCell(LastEditingCell, new DataGridCellInfo(EditingCell.DataContext, EditingCell.Column)) &&
                ContainsPendingInvalidCell(LastEditingCell) &&
                EditingCell.HasChildValidationError &&
                e.OriginalSource is DependencyObject Source &&
                (Source as DataGridCell ?? Source.FindVisualParents<DataGridCell>().FirstOrDefault()) is DataGridCell Cell)
            {
                DataGridCellInfo CellInfo = new(Cell.DataContext, Cell.Column);
                if (ContainsPendingInvalidCell(CellInfo) &&
                    Cell.HasChildValidationError &&
                    !IsSameCell(CellInfo, LastEditingCell))
                {
                    // Cancel the previous invalid edit so the double-click can follow the native begin-edit path.
                    EditingCell.ForceChildValidationError();
                    EditingCell.CancelEdit();
                    HasCellValidationError = false;
                }
            }

            base.OnPreviewMouseDoubleClick(e);
        }

        private DataGridCellInfo LastEditingCell;
        private DataGridCell LastEditingCellContainer;
        protected internal readonly List<DataGridCellInfo> PendingInvalidCells = [];
        private static IEnumerable<DataGridCellInfo> GetInvalidCells(IEnumerable<DataGridCell> Cells)
        {
            foreach (DataGridCell Cell in Cells)
                if (Cell.HasChildValidationError)
                    yield return new DataGridCellInfo(Cell.DataContext, Cell.Column);
        }
        private void RemoveDuplicatePendingInvalidCells()
        {
            HashSet<(object Item, System.Windows.Controls.DataGridColumn Column)> Cells = [];
            for (int i = PendingInvalidCells.Count - 1; 0 <= i; i--)
            {
                DataGridCellInfo Cell = PendingInvalidCells[i];
                if (!Cell.IsValid ||
                    Cell.Item is null ||
                    Cell.Column is null)
                {
                    if (IsSameCell(Cell, LastEditingCell))
                    {
                        LastEditingCell = default;
                        LastEditingCellContainer = null;
                    }

                    PendingInvalidCells.RemoveAt(i);
                }
            }

            for (int i = 0; i < PendingInvalidCells.Count; i++)
            {
                DataGridCellInfo Cell = PendingInvalidCells[i];
                if (Cells.Add((Cell.Item, Cell.Column)))
                    continue;

                PendingInvalidCells.RemoveAt(i);
                if (IsSameCell(Cell, LastEditingCell))
                {
                    LastEditingCell = default;
                    LastEditingCellContainer = null;
                }

                i--;
            }
        }
        private void MoveToNextPendingInvalidCell(DataGridCellInfo EditingCell)
        {
            if (!ContainsPendingInvalidCell(EditingCell))
                return;

            if (IsPendingCellInvalid(EditingCell))
                return;

            if (GetNextPendingInvalidCell(EditingCell) is DataGridCell InvalidCell)
            {
                BeginEditInvalidCell(InvalidCell);
                return;
            }

            PendingInvalidCells.Clear();
            LastEditingCell = default;
            LastEditingCellContainer = null;
        }
        private bool ContainsPendingInvalidCell(DataGridCellInfo Cell)
            => PendingInvalidCells.Any(i => IsSameCell(i, Cell));
        private DataGridCell GetNextPendingInvalidCell(DataGridCellInfo Current)
        {
            List<DataGridCellInfo> OriginalCells = PendingInvalidCells.ToList();
            int CurrentIndex = OriginalCells.FindIndex(i => IsSameCell(i, Current));
            for (int i = PendingInvalidCells.Count - 1; 0 <= i; i--)
            {
                DataGridCellInfo Cell = PendingInvalidCells[i];
                if (!IsPendingCellInvalid(Cell))
                {
                    if (IsSameCell(Cell, LastEditingCell))
                    {
                        LastEditingCell = default;
                        LastEditingCellContainer = null;
                    }

                    PendingInvalidCells.RemoveAt(i);
                    continue;
                }

                if (IsSameCell(Cell, Current))
                {
                    if (IsSameCell(Cell, LastEditingCell))
                    {
                        LastEditingCell = default;
                        LastEditingCellContainer = null;
                    }

                    PendingInvalidCells.RemoveAt(i);
                }
            }

            if (0 <= CurrentIndex)
            {
                for (int i = CurrentIndex + 1; i < OriginalCells.Count; i++)
                    if (ContainsPendingInvalidCell(OriginalCells[i]) &&
                        TryGetCell(OriginalCells[i], out DataGridCell InvalidCell))
                        return InvalidCell;
            }

            int EndIndex = 0 <= CurrentIndex ? CurrentIndex : OriginalCells.Count;
            for (int i = 0; i < EndIndex; i++)
                if (ContainsPendingInvalidCell(OriginalCells[i]) &&
                    TryGetCell(OriginalCells[i], out DataGridCell InvalidCell))
                    return InvalidCell;

            return null;
        }
        private bool IsPendingCellInvalid(DataGridCellInfo Cell)
        {
            if (!TryGetCell(Cell, out DataGridCell Target))
                return false;

            if (Target.Row?.BindingGroup is BindingGroup Group)
                Group.Validate();

            return Target.HasChildValidationError;
        }
        private bool TryGetCell(DataGridCellInfo Cell, out DataGridCell Target)
        {
            Target = null;

            if (!Cell.IsValid ||
                Cell.Item is null ||
                Cell.Column is null ||
                Items.IndexOf(Cell.Item) < 0)
                return false;

            ScrollIntoView(Cell.Item, Cell.Column);
            UpdateLayout();

            if (this.GetRow(Cell.Item) is not DataGridRow Row)
                return false;

            int ColumnIndex = Columns.IndexOf(Cell.Column);
            if (ColumnIndex < 0)
                return false;

            Target = this.GetCell(Row, ColumnIndex) as DataGridCell;
            return Target is not null;
        }
        private static bool IsSameCell(DataGridCellInfo x, DataGridCellInfo y)
            => x.IsValid &&
               y.IsValid &&
               ReferenceEquals(x.Item, y.Item) &&
               x.Column == y.Column;
        private void BeginEditInvalidCell(DataGridCell Cell)
        {
            DataGridCellInfo CellInfo = new(Cell.DataContext, Cell.Column);
            SelectedCells.Clear();
            CurrentCell = CellInfo;
            SelectedCells.Add(CellInfo);
            ScrollIntoView(Cell.DataContext, Cell.Column);
            Cell.Focus();
            BeginEdit(new RoutedEventArgs(ProgrammaticBeginEditEvent, this));
            if (CurrentCellContainerProperty.GetValue(this) is DataGridCell EditingCell &&
                EditingCell.IsEditing)
            {
                LastEditingCell = new DataGridCellInfo(EditingCell.DataContext, EditingCell.Column);
                LastEditingCellContainer = EditingCell;
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

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            foreach (ColumnFilterState Filter in ColumnFilters.Values)
                Filter.Column.SetIsFilterActive(false);

            ColumnFilters.Clear();
            CurrentColumnFilter = null;
            LastEditingCell = default;
            LastEditingCellContainer = null;

            base.OnItemsSourceChanged(oldValue, newValue);
            CoerceColumnCanUserFilter();
        }

        private readonly Dictionary<DataGridColumn, ColumnFilterState> ColumnFilters = [];
        private ColumnFilterState CurrentColumnFilter;
        private bool CanOpenColumnFilter(DataGridColumn Column)
            => Column.CanUserFilter && !string.IsNullOrEmpty(Column.FilterMemberPath) && GetCollectionView() != null;
        internal ColumnFilterState OpenColumnFilter(DataGridColumn Column)
        {
            CoerceColumnCanUserFilter();
            if (!CanOpenColumnFilter(Column))
                return null;

            CurrentColumnFilter = GetColumnFilter(Column);
            RebuildColumnFilterValues(CurrentColumnFilter);
            return CurrentColumnFilter;
        }
        private ColumnFilterState GetColumnFilter(DataGridColumn Column)
        {
            if (ColumnFilters.TryGetValue(Column, out ColumnFilterState Filter))
                return Filter;

            Filter = new ColumnFilterState(Column);
            ColumnFilters.Add(Column, Filter);
            return Filter;
        }
        internal void ApplyColumnSort(ListSortDirection Direction)
        {
            if (CurrentColumnFilter is null ||
                string.IsNullOrEmpty(CurrentColumnFilter.Column.SortMemberPath))
                return;

            ICollectionView View = GetCollectionView();
            if (View is null)
                return;

            View.SortDescriptions.Clear();
            View.SortDescriptions.Add(new SortDescription(CurrentColumnFilter.Column.SortMemberPath, Direction));
            foreach (System.Windows.Controls.DataGridColumn Column in Columns)
                Column.SortDirection = null;

            CurrentColumnFilter.Column.SortDirection = Direction;
        }
        internal void ClearColumnSort()
        {
            ICollectionView View = GetCollectionView();
            if (View is null)
                return;

            View.SortDescriptions.Clear();
            foreach (System.Windows.Controls.DataGridColumn Column in Columns)
                Column.SortDirection = null;
        }
        internal void SelectAllColumnFilterValues()
        {
            if (CurrentColumnFilter is null)
                return;

            foreach (ColumnFilterValue Value in CurrentColumnFilter.Values)
                Value.IsChecked = true;
        }
        internal void ClearColumnFilter()
        {
            if (CurrentColumnFilter is null)
                return;

            CurrentColumnFilter.Clear();
            RebuildColumnFilterValues(CurrentColumnFilter);
            RefreshColumnFilter();
        }
        internal void ApplyColumnFilter()
        {
            if (CurrentColumnFilter is null)
                return;

            CurrentColumnFilter.Apply();
            CurrentColumnFilter.Column.SetIsFilterActive(CurrentColumnFilter.IsActive);
            RefreshColumnFilter();
        }
        private void RebuildColumnFilterValues(ColumnFilterState Filter)
        {
            Filter.PendingContainsText = Filter.ContainsText;
            Filter.Values.Clear();

            foreach (string Value in GetFilterSourceItems().Where(i => MatchesColumnFilters(i, Filter))
                                                           .Select(Filter.GetValueText)
                                                           .Distinct()
                                                           .OrderBy(i => i))
            {
                bool IsChecked = Filter.SelectedValues is null || Filter.SelectedValues.Contains(Value);
                Filter.Values.Add(new ColumnFilterValue(Value, IsChecked));
            }
        }
        private IEnumerable<object> GetFilterSourceItems()
        {
            IEnumerable Source = ItemsSource ?? Items;
            foreach (object Item in Source)
                if (!IsNewItemPlaceholder(Item))
                    yield return Item;
        }
        private void RefreshColumnFilter()
        {
            ICollectionView View = GetCollectionView();
            if (View is null ||
                IsExternalFilterUsed(View))
            {
                CoerceColumnCanUserFilter();
                return;
            }

            View.Filter = HasColumnFilter ? ColumnFilterPredicate : null;
            View.Refresh();
            CoerceColumnCanUserFilter();
        }
        private bool HasColumnFilter
            => ColumnFilters.Values.Any(i => i.IsActive);
        private bool OnColumnFilter(object Item)
            => IsNewItemPlaceholder(Item) || MatchesColumnFilters(Item, null);
        private bool MatchesColumnFilters(object Item, ColumnFilterState Except)
            => ColumnFilters.Values.All(i => ReferenceEquals(i, Except) || !i.IsActive || i.Matches(Item));

        private ICollectionView GetCollectionView()
            => CollectionViewSource.GetDefaultView(ItemsSource ?? Items);
        private bool IsExternalFilterUsed(ICollectionView View)
            => View.Filter != null && View.Filter != ColumnFilterPredicate;
        private void CoerceColumnCanUserFilter()
        {
            foreach (System.Windows.Controls.DataGridColumn Column in Columns)
                if (Column is DataGridColumn ThisColumn)
                    ThisColumn.CoerceValue(DataGridColumn.CanUserFilterProperty);
        }

        protected override DependencyObject GetContainerForItemOverride()
            => new DataGridRow();

        private static readonly object DataGridNewItemPlaceholder;
        public static bool IsNewItemPlaceholder(object Data)
            => Data != null && (Data == CollectionView.NewItemPlaceholder || Data == DataGridNewItemPlaceholder);

        public sealed class ColumnFilterState(DataGridColumn Column) : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public DataGridColumn Column { get; } = Column;

            public ObservableCollection<ColumnFilterValue> Values { get; } = [];

            private string _ContainsText;
            public string ContainsText
            {
                get => _ContainsText;
                private set
                {
                    if (_ContainsText == value)
                        return;

                    _ContainsText = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsActive));
                }
            }

            private string _PendingContainsText;
            public string PendingContainsText
            {
                get => _PendingContainsText;
                set
                {
                    if (_PendingContainsText == value)
                        return;

                    _PendingContainsText = value;
                    OnPropertyChanged();
                }
            }

            internal HashSet<string> SelectedValues { get; private set; }

            public bool IsActive
                => !string.IsNullOrWhiteSpace(ContainsText) || SelectedValues is not null;

            public void Apply()
            {
                ContainsText = PendingContainsText;

                HashSet<string> Selected = new(Values.Where(i => i.IsChecked).Select(i => i.Text));
                SelectedValues = Selected.Count == Values.Count ? null : Selected;
                OnPropertyChanged(nameof(IsActive));
            }

            public void Clear()
            {
                ContainsText = null;
                PendingContainsText = null;
                SelectedValues = null;
                foreach (ColumnFilterValue Value in Values)
                    Value.IsChecked = true;

                Column.SetIsFilterActive(false);
                OnPropertyChanged(nameof(IsActive));
            }

            public bool Matches(object Item)
            {
                string Value = GetValueText(Item);
                if (!string.IsNullOrWhiteSpace(ContainsText) &&
                    Value.IndexOf(ContainsText, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;

                return SelectedValues is null || SelectedValues.Contains(Value);
            }

            public string GetValueText(object Item)
            {
                object Value = Item;
                foreach (string Member in Column.FilterMemberPath.Split('.'))
                {
                    if (Value is null)
                        return string.Empty;

                    PropertyDescriptor Property = TypeDescriptor.GetProperties(Value)[Member];
                    if (Property is null)
                        return string.Empty;

                    Value = Property.GetValue(Value);
                }

                return Value?.ToString() ?? string.Empty;
            }

            private void OnPropertyChanged([CallerMemberName] string PropertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        }

        public sealed class ColumnFilterValue(string Text, bool IsChecked) : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string Text { get; } = Text;

            private bool _IsChecked = IsChecked;
            public bool IsChecked
            {
                get => _IsChecked;
                set
                {
                    if (_IsChecked == value)
                        return;

                    _IsChecked = value;
                    OnPropertyChanged();
                }
            }

            private void OnPropertyChanged([CallerMemberName] string PropertyName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        }

        private sealed class PastingRow(object Item, bool IsDetached)
        {
            public object Item { get; } = Item;

            public bool IsDetached { get; } = IsDetached;

            public int PastedColumnCount { get; set; }

            public void Paste(DataGridColumn Column, object CellContent)
            {
                if (IsDetached)
                    Column.PasteDetachedCellClipboardContent(Item, CellContent);
                else
                    Column.OnPastingCellClipboardContent(Item, CellContent);
            }

        }

    }
}
