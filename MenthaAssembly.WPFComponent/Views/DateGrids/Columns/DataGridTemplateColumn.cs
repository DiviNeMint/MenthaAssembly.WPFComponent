using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MenthaAssembly.Views
{
    public class DataGridTemplateColumn : DataGridColumn
    {
        protected internal override bool AllowEditingMode
            => true;

        /// <summary>
        /// The DependencyProperty representing the CellTemplate property.
        /// </summary>
        public static readonly DependencyProperty CellTemplateProperty =
            DependencyProperty.Register(nameof(CellTemplate), typeof(DataTemplate), typeof(DataGridTemplateColumn),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(NotifyPropertyChangeForRefreshContent)));
        /// <summary>
        /// A template describing how to display data for a cell in this column.
        /// </summary>
        public DataTemplate CellTemplate
        {
            get => (DataTemplate)GetValue(CellTemplateProperty);
            set => SetValue(CellTemplateProperty, value);
        }

        /// <summary>
        /// The DependencyProperty representing the CellTemplateSelector property.
        /// </summary>
        public static readonly DependencyProperty CellTemplateSelectorProperty =
            DependencyProperty.Register(nameof(CellTemplateSelector), typeof(DataTemplateSelector), typeof(DataGridTemplateColumn),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(NotifyPropertyChangeForRefreshContent)));
        /// <summary>
        /// A template selector describing how to display data for a cell in this column.
        /// </summary>
        public DataTemplateSelector CellTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(CellTemplateSelectorProperty);
            set => SetValue(CellTemplateSelectorProperty, value);
        }

        /// <summary>
        /// The DependencyProperty representing the CellEditingTemplate
        /// </summary>
        public static readonly DependencyProperty CellEditingTemplateProperty =
            DependencyProperty.Register(nameof(CellEditingTemplate), typeof(DataTemplate), typeof(DataGridTemplateColumn),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(NotifyPropertyChangeForRefreshContent)));
        /// <summary>
        /// A template describing how to display data for a cell that is being edited in this column.
        /// </summary>
        public DataTemplate CellEditingTemplate
        {
            get => (DataTemplate)GetValue(CellEditingTemplateProperty);
            set => SetValue(CellEditingTemplateProperty, value);
        }

        /// <summary>
        /// The DependencyProperty representing the CellEditingTemplateSelector
        /// </summary>
        public static readonly DependencyProperty CellEditingTemplateSelectorProperty =
            DependencyProperty.Register(nameof(CellEditingTemplateSelector), typeof(DataTemplateSelector), typeof(DataGridTemplateColumn),
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(NotifyPropertyChangeForRefreshContent)));
        /// <summary>
        /// A template selector describing how to display data for a cell that is being edited in this column.
        /// </summary>
        public DataTemplateSelector CellEditingTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(CellEditingTemplateSelectorProperty);
            set => SetValue(CellEditingTemplateSelectorProperty, value);
        }

        static DataGridTemplateColumn()
        {
            Type ThisType = typeof(DataGridTemplateColumn);
            SortMemberPathProperty.OverrideMetadata(ThisType, new FrameworkPropertyMetadata(OnTemplateColumnSortMemberPathChanged));
            static void OnTemplateColumnSortMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
                => d.CoerceValue(CanUserSortProperty);

            if (typeof(System.Windows.Controls.DataGridColumn).TryGetStaticInternalMethod("OnCoerceCanUserSort", out MethodInfo Method))
            {
                CanUserSortProperty.OverrideMetadata(ThisType, new FrameworkPropertyMetadata(null, OnCoerceTemplateColumnCanUserSort));
                object OnCoerceTemplateColumnCanUserSort(DependencyObject d, object baseValue)
                {
                    if (string.IsNullOrEmpty(((DataGridTemplateColumn)d).SortMemberPath))
                        return false;

                    return Method.Invoke(null, [d, baseValue]);
                }
            }
        }

        protected override FrameworkElement GenerateElement(DataGridCell Cell, object DataItem)
            => LoadTemplateContent(false, DataItem, Cell);

        protected override FrameworkElement GenerateEditingElement(DataGridCell Cell, object DataItem)
            => LoadTemplateContent(true, DataItem, Cell);

        private FrameworkElement LoadTemplateContent(bool IsEditing, object DataItem, DataGridCell Cell)
        {
            if (TryGetCellTemplateAndSelector(IsEditing, out DataTemplate Template, out DataTemplateSelector TemplateSelector))
            {
                ContentPresenter Presenter = new()
                {
                    DataContext = DataItem,
                    ContentTemplate = Template,
                    ContentTemplateSelector = TemplateSelector
                };

                Presenter.PreviewMouseDown += (s, e) =>
                {
                    if (!Presenter.BindingGroup.HasValidationError &&
                        DataGridOwner is DataGrid Owner &&
                        Owner.HasCellValidationError)
                    {
                        Owner.SelectedItem = DataItem;
                        Cell.Focus();
                        e.Handled = true;
                    }
                };

                BindingOperations.SetBinding(Presenter, ContentPresenter.ContentProperty, new Binding());
                return Presenter;
            }

            return null;
        }
        private bool TryGetCellTemplateAndSelector(bool IsEditing, out DataTemplate Template, out DataTemplateSelector TemplateSelector)
        {
            if (IsEditing)
            {
                Template = CellEditingTemplate;
                TemplateSelector = CellEditingTemplateSelector;
            }
            else
            {
                Template = CellTemplate;
                TemplateSelector = CellTemplateSelector;
            }

            return Template != null || TemplateSelector != null;
        }

        protected override void RefreshCellContent(FrameworkElement Element, string PropertyName)
        {
            if (Element is DataGridCell Cell)
            {
                bool isCellEditing = Cell.IsEditing;
                bool PropertyNameOrdinalEquals(string Name)
                    => string.Compare(PropertyName, Name, StringComparison.Ordinal) == 0;

                if ((!isCellEditing && (PropertyNameOrdinalEquals(nameof(CellTemplate)) || PropertyNameOrdinalEquals(nameof(CellTemplateSelector)))) ||
                    (isCellEditing && (PropertyNameOrdinalEquals(nameof(CellEditingTemplate)) || PropertyNameOrdinalEquals(nameof(CellEditingTemplateSelector)))))
                {
                    Cell.BuildVisualTree();
                    return;
                }
            }

            base.RefreshCellContent(Element, PropertyName);
        }

        protected internal override void RaiseBeforeEditing(CellBeforeEditingEventArgs e)
        {
            if (CellEditingTemplate is null &&
                CellEditingTemplateSelector?.SelectTemplate(e.DataContext, e.Cell) is null)
            {
                e.Handled = true;
                return;
            }

            base.RaiseBeforeEditing(e);
        }

        public static bool IsDataGridTextBoxBeginEdit(CellInputEventArgs e)
            => DataGridHelper.IsDataGridTextBoxBeginEdit(e);

    }
}