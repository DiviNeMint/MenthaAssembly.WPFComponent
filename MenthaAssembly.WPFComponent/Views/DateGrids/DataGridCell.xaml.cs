using System;
using System.Collections.Generic;
using System.Linq;
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

        public static readonly DependencyProperty HasChildValidationErrorProperty =
            DependencyProperty.Register(nameof(HasChildValidationError), typeof(bool), typeof(DataGridCell), new PropertyMetadata(false));
        public bool HasChildValidationError
        {
            get => (bool)GetValue(HasChildValidationErrorProperty);
            private set => SetValue(HasChildValidationErrorProperty, value);
        }

        public static readonly DependencyProperty FirstValidationErrorContentProperty =
            DependencyProperty.Register(nameof(FirstValidationErrorContent), typeof(object), typeof(DataGridCell), new PropertyMetadata(null));
        public object FirstValidationErrorContent
        {
            get => GetValue(FirstValidationErrorContentProperty);
            private set => SetValue(FirstValidationErrorContentProperty, value);
        }

        public DataGridRow Row { get; private set; }

        private static readonly MethodInfo CancelEditMethod;
        private static readonly MethodInfo BuildVisualTreeMethod;
        private static readonly PropertyInfo RowOwnerProperty;
        static DataGridCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridCell), new FrameworkPropertyMetadata(typeof(DataGridCell)));

            Type MSCellType = typeof(System.Windows.Controls.DataGridCell);
            MSCellType.TryGetInternalMethod(nameof(CancelEdit), out CancelEditMethod);
            MSCellType.TryGetInternalMethod(nameof(BuildVisualTree), out BuildVisualTreeMethod);
            MSCellType.TryGetInternalProperty("RowOwner", out RowOwnerProperty);
        }
        public DataGridCell()
        {
            AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(OnValidationError), true);
            DataContextChanged += (s, e) => ClearChildValidationErrors();
            Unloaded += (s, e) => ClearChildValidationErrors();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Row = RowOwnerProperty?.GetValue(this) as DataGridRow;
            RefreshChildValidationErrors();
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

        protected internal void CancelEdit()
            => CancelEditMethod?.Invoke(this, null);

        private readonly HashSet<ValidationError> ChildValidationErrors = new();
        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject Source ||
                Source != this &&
                Source.FindVisualParents<DataGridCell>().FirstOrDefault() != this)
                return;

            switch (e.Action)
            {
                case ValidationErrorEventAction.Added:
                    {
                        ChildValidationErrors.Add(e.Error);
                        HasForcedChildValidationError = false;
                        break;
                    }
                case ValidationErrorEventAction.Removed:
                    {
                        ChildValidationErrors.Remove(e.Error);
                        if (ChildValidationErrors.Count == 0)
                        {
                            RefreshChildValidationErrors();
                            return;
                        }

                        break;
                    }
            }

            UpdateChildValidationState();
        }

        private bool HasForcedChildValidationError;
        protected internal void ForceChildValidationError()
        {
            HasForcedChildValidationError = true;
            UpdateChildValidationState();
        }
        protected internal void ClearForcedChildValidationError()
        {
            HasForcedChildValidationError = false;
            UpdateChildValidationState();
        }

        private void RefreshChildValidationErrors()
        {
            ChildValidationErrors.Clear();

            foreach (FrameworkElement Element in this.FindVisualChildren<FrameworkElement>())
                if (Validation.GetHasError(Element))
                    foreach (ValidationError Error in Validation.GetErrors(Element))
                        ChildValidationErrors.Add(Error);

            UpdateChildValidationState();
        }
        private void ClearChildValidationErrors()
        {
            HasForcedChildValidationError = false;
            ChildValidationErrors.Clear();
            UpdateChildValidationState();
        }
        private void UpdateChildValidationState()
        {
            HasChildValidationError = HasForcedChildValidationError || ChildValidationErrors.Count > 0;
            FirstValidationErrorContent = ChildValidationErrors.FirstOrDefault()?.ErrorContent;
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
