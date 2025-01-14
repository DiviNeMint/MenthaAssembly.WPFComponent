using System;
using System.Windows;
using System.Windows.Data;

namespace MenthaAssembly.Views
{
    /// <summary>
    /// A base class for specifying column definitions for certain standard types that do not allow arbitrary templates.
    /// </summary>
    public abstract class DataGridBoundColumn : DataGridColumn
    {
        private BindingBase _Binding;
        /// <summary>
        /// The binding that will be applied to the generated element.
        /// </summary>
        /// <remarks>
        /// This isn't a DP because if it were getting the value would evaluate the binding.
        /// </remarks>
        public virtual BindingBase Binding
        {
            get => _Binding;
            set
            {
                if (_Binding != value)
                {
                    BindingBase oldBinding = _Binding;
                    _Binding = value;
                    CoerceValue(IsReadOnlyProperty);
                    CoerceValue(SortMemberPathProperty);
                    OnBindingChanged(oldBinding, _Binding);
                }
            }
        }

        public override BindingBase ClipboardContentBinding
        {
            get => base.ClipboardContentBinding ?? Binding;
            set => base.ClipboardContentBinding = value;
        }

        /// <summary>
        /// Called when Binding changes.
        /// </summary>
        /// <remarks>
        /// Default implementation notifies the DataGrid and its subtree about the change.
        /// </remarks>
        /// <param name="oldBinding">The old binding.</param>
        /// <param name="newBinding">The new binding.</param>
        protected virtual void OnBindingChanged(BindingBase oldBinding, BindingBase newBinding)
            => NotifyPropertyChanged(nameof(Binding));

        /// <summary>
        /// The DependencyProperty for the ElementStyle property.
        /// </summary>
        public static readonly DependencyProperty ElementStyleProperty =
            DependencyProperty.Register("ElementStyle", typeof(Style), typeof(DataGridBoundColumn), new FrameworkPropertyMetadata(null, NotifyPropertyChangeForRefreshContent));
        /// <summary>
        /// A style that is applied to the generated element when not editing.
        /// The TargetType of the style depends on the derived column class.
        /// </summary>
        public Style ElementStyle
        {
            get => (Style)GetValue(ElementStyleProperty);
            set => SetValue(ElementStyleProperty, value);
        }

        /// <summary>
        /// The DependencyProperty for the EditingElementStyle property.
        /// </summary>
        public static readonly DependencyProperty EditingElementStyleProperty =
            DependencyProperty.Register("EditingElementStyle", typeof(Style), typeof(DataGridBoundColumn), new FrameworkPropertyMetadata(null, NotifyPropertyChangeForRefreshContent));
        /// <summary>
        /// A style that is applied to the generated element when editing.
        /// The TargetType of the style depends on the derived column class.
        /// </summary>
        public Style EditingElementStyle
        {
            get => (Style)GetValue(EditingElementStyleProperty);
            set => SetValue(EditingElementStyleProperty, value);
        }

        static DataGridBoundColumn()
        {
            SortMemberPathProperty.OverrideMetadata(typeof(DataGridBoundColumn), new FrameworkPropertyMetadata(null, OnCoerceSortMemberPath));
            static object OnCoerceSortMemberPath(DependencyObject d, object baseValue)
            {
                DataGridBoundColumn Column = (DataGridBoundColumn)d;

                string SortMemberPath = (string)baseValue;
                if (string.IsNullOrEmpty(SortMemberPath))
                {
                    string bindingSortMemberPath = DependencyHelper.GetPathFromBinding(Column.Binding as Binding);
                    if (!string.IsNullOrEmpty(bindingSortMemberPath))
                        SortMemberPath = bindingSortMemberPath;
                }

                return SortMemberPath;
            }
        }

        protected override bool OnCoerceIsReadOnly(bool baseValue)
            => DependencyHelper.IsOneWay(_Binding) || base.OnCoerceIsReadOnly(baseValue);

        protected override void RefreshCellContent(FrameworkElement Element, string PropertyName)
        {
            if (Element is DataGridCell Cell)
            {
                bool isCellEditing = Cell.IsEditing;
                if ((string.Compare(PropertyName, nameof(Binding), StringComparison.Ordinal) == 0) ||
                    (string.Compare(PropertyName, nameof(ElementStyle), StringComparison.Ordinal) == 0 && !isCellEditing) ||
                    (string.Compare(PropertyName, nameof(EditingElementStyle), StringComparison.Ordinal) == 0 && isCellEditing))
                {
                    Cell.BuildVisualTree();
                    return;
                }
            }

            base.RefreshCellContent(Element, PropertyName);
        }

    }
}