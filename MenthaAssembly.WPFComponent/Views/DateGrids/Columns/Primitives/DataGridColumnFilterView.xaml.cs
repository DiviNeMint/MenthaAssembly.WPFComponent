using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MenthaAssembly.Views
{
    public class DataGridColumnFilterView : Control
    {
        public static RoutedCommand SortAscendingCommand { get; } = new(nameof(SortAscendingCommand), typeof(DataGridColumnFilterView));

        public static RoutedCommand SortDescendingCommand { get; } = new(nameof(SortDescendingCommand), typeof(DataGridColumnFilterView));

        public static RoutedCommand ClearSortCommand { get; } = new(nameof(ClearSortCommand), typeof(DataGridColumnFilterView));

        public static RoutedCommand SelectAllCommand { get; } = new(nameof(SelectAllCommand), typeof(DataGridColumnFilterView));

        public static RoutedCommand ClearCommand { get; } = new(nameof(ClearCommand), typeof(DataGridColumnFilterView));

        public static RoutedCommand ApplyCommand { get; } = new(nameof(ApplyCommand), typeof(DataGridColumnFilterView));

        internal new static ComponentResourceKey DefaultStyleKey { get; } = new ComponentResourceKey(typeof(DataGridColumnFilterView), nameof(DefaultStyle));

        public static Style DefaultStyle
            => Application.Current.TryFindResource(DefaultStyleKey) as Style;

        public static readonly DependencyProperty ColumnProperty =
            DependencyProperty.Register(nameof(Column), typeof(System.Windows.Controls.DataGridColumn), typeof(DataGridColumnFilterView), new PropertyMetadata(null, OnColumnChanged));
        public System.Windows.Controls.DataGridColumn Column
        {
            get => (System.Windows.Controls.DataGridColumn)GetValue(ColumnProperty);
            set => SetValue(ColumnProperty, value);
        }

        public static readonly DependencyProperty DataGridOwnerProperty =
            DependencyProperty.Register(nameof(DataGridOwner), typeof(DataGrid), typeof(DataGridColumnFilterView), new PropertyMetadata(null, OnDataGridOwnerChanged));
        public DataGrid DataGridOwner
        {
            get => (DataGrid)GetValue(DataGridOwnerProperty);
            set => SetValue(DataGridOwnerProperty, value);
        }

        static DataGridColumnFilterView()
        {
            CommandManager.RegisterClassCommandBinding(typeof(DataGridColumnFilterView), new CommandBinding(SortAscendingCommand, OnExecutedSortAscending, OnCanExecuteSort));
            CommandManager.RegisterClassCommandBinding(typeof(DataGridColumnFilterView), new CommandBinding(SortDescendingCommand, OnExecutedSortDescending, OnCanExecuteSort));
            CommandManager.RegisterClassCommandBinding(typeof(DataGridColumnFilterView), new CommandBinding(ClearSortCommand, OnExecutedClearSort, OnCanExecuteClearSort));
            CommandManager.RegisterClassCommandBinding(typeof(DataGridColumnFilterView), new CommandBinding(SelectAllCommand, OnExecutedSelectAll, OnCanExecuteFilterCommand));
            CommandManager.RegisterClassCommandBinding(typeof(DataGridColumnFilterView), new CommandBinding(ClearCommand, OnExecutedClear, OnCanExecuteFilterCommand));
            CommandManager.RegisterClassCommandBinding(typeof(DataGridColumnFilterView), new CommandBinding(ApplyCommand, OnExecutedApply, OnCanExecuteFilterCommand));
        }

        public DataGridColumnFilterView()
        {
            Loaded += DataGridColumnFilterView_Loaded;
            IsVisibleChanged += DataGridColumnFilterView_IsVisibleChanged;
        }

        private void DataGridColumnFilterView_Loaded(object sender, RoutedEventArgs e)
        {
            ResolveDataGridOwner();
            ApplyFilterViewStyle();
        }
        private void DataGridColumnFilterView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
                PrepareFilter();
        }

        private bool IsPreparingFilter;
        private void PrepareFilter()
        {
            if (IsPreparingFilter)
                return;

            IsPreparingFilter = true;
            ResolveDataGridOwner();
            ApplyFilterViewStyle();
            DataContext = Column is DataGridColumn ThisColumn ? DataGridOwner?.OpenColumnFilter(ThisColumn) : null;
            IsPreparingFilter = false;

            if (DataContext is null)
                ClosePopup();
        }
        private void ResolveDataGridOwner()
        {
            if (DataGridOwner != null)
                return;

            if (Parent is Popup Popup &&
                Popup.PlacementTarget is DependencyObject Target)
                DataGridOwner = Target.FindVisualParents<DataGrid>().FirstOrDefault();
        }
        private void ApplyFilterViewStyle()
        {
            Style NewStyle = DataGridOwner?.ColumnFilterViewStyle ?? DefaultStyle;
            if (NewStyle != null &&
                !ReferenceEquals(Style, NewStyle))
                Style = NewStyle;
        }
        private void ClosePopup()
        {
            if (Parent is Popup Popup)
                Popup.IsOpen = false;
        }

        private static void OnColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridColumnFilterView View &&
                View.IsVisible)
                View.PrepareFilter();
        }
        private static void OnDataGridOwnerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridColumnFilterView View &&
                View.IsVisible)
                View.PrepareFilter();
        }
        private static void OnCanExecuteSort(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is DataGridColumnFilterView View &&
                View.DataContext is DataGrid.ColumnFilterState Filter)
            {
                e.CanExecute = !(View.DataGridOwner?.Items is IEditableCollectionView EditableView &&
                                 (EditableView.IsAddingNew || EditableView.IsEditingItem)) &&
                               !string.IsNullOrEmpty(Filter.Column.SortMemberPath) &&
                               ((e.Command == SortAscendingCommand && Filter.Column.SortDirection != ListSortDirection.Ascending) ||
                                (e.Command == SortDescendingCommand && Filter.Column.SortDirection != ListSortDirection.Descending));
                e.Handled = true;
            }
        }
        private static void OnCanExecuteClearSort(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is DataGridColumnFilterView View &&
                View.DataContext is DataGrid.ColumnFilterState Filter)
            {
                e.CanExecute = !(View.DataGridOwner?.Items is IEditableCollectionView EditableView &&
                                 (EditableView.IsAddingNew || EditableView.IsEditingItem)) &&
                               Filter.Column.SortDirection != null;
                e.Handled = true;
            }
        }
        private static void OnCanExecuteFilterCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is DataGridColumnFilterView View)
            {
                e.CanExecute = !(View.DataGridOwner?.Items is IEditableCollectionView EditableView &&
                                 (EditableView.IsAddingNew || EditableView.IsEditingItem)) &&
                               View.DataContext is DataGrid.ColumnFilterState;
                e.Handled = true;
            }
        }
        private static void OnExecutedSortAscending(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DataGridColumnFilterView View)
            {
                View.DataGridOwner?.ApplyColumnSort(ListSortDirection.Ascending);
                e.Handled = true;
            }
        }
        private static void OnExecutedSortDescending(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DataGridColumnFilterView View)
            {
                View.DataGridOwner?.ApplyColumnSort(ListSortDirection.Descending);
                e.Handled = true;
            }
        }
        private static void OnExecutedClearSort(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DataGridColumnFilterView View)
            {
                View.DataGridOwner?.ClearColumnSort();
                e.Handled = true;
            }
        }
        private static void OnExecutedSelectAll(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DataGridColumnFilterView View)
            {
                View.DataGridOwner?.SelectAllColumnFilterValues();
                e.Handled = true;
            }
        }
        private static void OnExecutedClear(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DataGridColumnFilterView View)
            {
                View.DataGridOwner?.ClearColumnFilter();
                e.Handled = true;
            }
        }
        private static void OnExecutedApply(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DataGridColumnFilterView View)
            {
                View.DataGridOwner?.ApplyColumnFilter();
                View.ClosePopup();
                e.Handled = true;
            }
        }

    }
}
