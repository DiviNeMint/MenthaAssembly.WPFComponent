using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MenthaAssembly.Views
{
    public class DataGridCell : System.Windows.Controls.DataGridCell
    {
        internal static readonly DependencyProperty CellClipboardProperty =
            DependencyProperty.Register("CellClipboard", typeof(object), typeof(DataGridCell));

        public DataGridRow Row { get; private set; }

        private static readonly MethodInfo BuildVisualTreeMethod;
        private static readonly PropertyInfo RowOwnerProperty;
        static DataGridCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridCell), new FrameworkPropertyMetadata(typeof(DataGridCell)));

            Type MSCellType = typeof(System.Windows.Controls.DataGridCell);
            MSCellType.TryGetInternalMethod("BuildVisualTree", out BuildVisualTreeMethod);
            MSCellType.TryGetInternalProperty("RowOwner", out RowOwnerProperty);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Row = RowOwnerProperty?.GetValue(this) as DataGridRow;
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (this.Column is DataGridColumn Column)
                Column.RaiseInput(this, e, DataContext);

            if (!e.Handled)
                base.OnTextInput(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (this.Column is DataGridColumn Column)
                Column.RaiseInput(this, e, DataContext);

            if (!e.Handled)
                base.OnKeyDown(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (this.Column is DataGridColumn Column)
                Column.RaiseInput(this, e, DataContext);

            if (!e.Handled)
                base.OnPreviewKeyDown(e);
        }

        protected internal void BuildVisualTree()
        {
            if (BuildVisualTreeMethod is null)
            {
                InvalidateTemplate();
                return;
            }

            BuildVisualTreeMethod.Invoke(this, []);
        }
        private void InvalidateTemplate()
        {
            if (Content is ContentPresenter Presenter &&
                !ReflectionHelper.TryInvokeInternalMethod(Presenter, "ReevaluateTemplate"))
            {
                object Data = Presenter.Content;
                Presenter.Content = null;
                Presenter.Content = Data;
            }
        }

    }
}