using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MenthaAssembly.MarkupExtensions
{
    public static class DataGridEx
    {
        /// <summary>
        /// Gets a specific row from the data grid. If the DataGrid is virtualised the row will be scrolled into view.
        /// </summary>
        /// <param name="This">The DataGrid.</param>
        /// <param name="RowIndex">Row number to get.</param>
        public static DataGridRow GetRow(this DataGrid This, int RowIndex)
        {
            if (RowIndex < 0 || This.Items.Count <= RowIndex)
                return null;

            if (This.ItemContainerGenerator.ContainerFromIndex(RowIndex) is not DataGridRow Row)
            {
                This.UpdateLayout();
                This.ScrollIntoView(This.Items[RowIndex]);
                Row = This.ItemContainerGenerator.ContainerFromIndex(RowIndex) as DataGridRow;
            }

            return Row;
        }

        /// <summary>
        /// Gets a specific row from the data grid. If the DataGrid is virtualised the row will be scrolled into view.
        /// </summary>
        /// <param name="This">The DataGrid.</param>
        /// <param name="Item">The Row data.</param>
        public static DataGridRow GetRow(this DataGrid This, object Item)
        {
            if (!This.Items.Contains(Item))
                return null;

            if (This.ItemContainerGenerator.ContainerFromItem(Item) is not DataGridRow Row)
            {
                This.UpdateLayout();
                This.ScrollIntoView(Item);
                Row = This.ItemContainerGenerator.ContainerFromItem(Item) as DataGridRow;
            }

            return Row;
        }

        /// <summary>
        /// Gets the row of the specific DataGridCell.
        /// </summary>
        /// <param name="This">The specific DataGridCell.</param>
        public static DataGridRow GetRow(this DataGridCell This)
        {
            if (ReflectionHelper.TryGetInternalPropertyValue(This, "RowOwner", out DataGridRow Row))
                return Row;

            if (This.FindLogicalParents<DataGridRow>().FirstOrDefault() is DataGridRow LogicalRow)
                return LogicalRow;

            return null;
        }

        /// <summary>
        /// Get the selected row.
        /// </summary>
        /// <param name="This">DataGridRow.</param>
        /// <returns>DataGridRow or null if no row selected.</returns>
        public static DataGridRow GetSelectedRow(this DataGrid This)
            => GetRow(This, This.SelectedIndex);

        /// <summary>
        /// Gets a specific cell from the DataGrid.
        /// </summary>
        /// <param name="This">The DataGrid.</param>
        /// <param name="Row">The row from which to get a cell from.</param>
        /// <param name="Column">The cell index.</param>
        /// <returns>A DataGridCell.</returns>
        public static DataGridCell GetCell(this DataGrid This, DataGridRow Row, int Column)
        {
            if (Row is null)
                return null;

            if (Column < 0 || This.Columns.Count <= Column)
                return null;

            if (Row.FindVisualChildren<DataGridCellsPresenter>().FirstOrDefault() is not DataGridCellsPresenter Presenter)
            {
                // Virtualised - scroll into view.
                This.ScrollIntoView(Row, This.Columns[Column]);
                Presenter = Row.FindVisualChildren<DataGridCellsPresenter>().FirstOrDefault();
            }

            return Presenter?.ItemContainerGenerator.ContainerFromIndex(Column) as DataGridCell;
        }

        /// <summary>
        /// Gets a specific cell from the DataGrid.
        /// </summary>
        /// <param name="This">The DataGrid.</param>
        /// <param name="Row">The row index.</param>
        /// <param name="Column">The cell index.</param>
        /// <returns>A DataGridCell.</returns>
        public static DataGridCell GetCell(this DataGrid This, int Row, int Column)
            => This.GetCell(This.GetRow(Row), Column);

        /// <summary>
        /// Gets a specific cell from the DataGrid.
        /// </summary>
        /// <param name="This">The DataGrid.</param>
        /// <param name="Item">The row datacontext.</param>
        /// <param name="Column">The cell column.</param>
        /// <returns>A DataGridCell.</returns>
        public static DataGridCell GetCell(this DataGrid This, object Item, DataGridColumn Column)
            => This.GetCell(This.GetRow(Item), This.Columns.IndexOf(Column));

        /// <summary>
        /// Gets the currently selected (focused) cell.
        /// </summary>
        /// <param name="This">The DataGrid.</param>
        /// <returns>DataGridCell or null if no cell is currently selected.</returns>
        public static DataGridCell GetSelectedCell(this DataGrid This)
        {
            if (ReflectionHelper.TryGetInternalPropertyValue(This, "CurrentCellContainer", out DataGridCell Cell))
                return Cell;

            if (This.GetSelectedRow() is DataGridRow Row)
            {
                for (int i = 0; i < This.Columns.Count; i++)
                {
                    Cell = This.GetCell(Row, i);
                    if (Cell.IsFocused)
                        return Cell;
                }
            }

            return null;
        }

    }
}