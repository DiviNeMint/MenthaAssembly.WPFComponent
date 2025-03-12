using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace MenthaAssembly.Views
{
    public class DataGridComboBoxColumn : DataGridColumn
    {
        private event EventHandler GroupStyleSelectorChanged;

        public static Style DefaultStyle
            => (Application.Current.TryFindResource(typeof(DataGridComboBoxColumn)) as Style) ?? System.Windows.Controls.DataGridComboBoxColumn.DefaultElementStyle;

        protected internal override bool AllowEditingMode
            => false;

        private BindingBase _TextBinding;
        /// <summary>
        /// The binding that will be applied to the Text property of the ComboBoxValue.
        /// </summary>
        /// <remarks>
        /// This isn't a DP because if it were getting the value would evaluate the binding.
        /// </remarks>
        public virtual BindingBase TextBinding
        {
            get => _TextBinding;
            set
            {
                if (_TextBinding != value)
                {
                    BindingBase oldBinding = _TextBinding;
                    _TextBinding = value;
                    CoerceValue(IsReadOnlyProperty);
                    CoerceValue(SortMemberPathProperty);
                    OnTextBindingChanged(oldBinding, _TextBinding);
                }
            }
        }

        private BindingBase _SelectedValueBinding;
        /// <summary>
        /// The binding that will be applied to the SelectedValue property of the ComboBox.  This works in conjunction with SelectedValuePath
        /// </summary>
        /// <remarks>
        /// This isn't a DP because if it were getting the value would evaluate the binding.
        /// </remarks>
        public virtual BindingBase SelectedValueBinding
        {
            get => _SelectedValueBinding;
            set
            {
                if (_SelectedValueBinding != value)
                {
                    BindingBase oldBinding = _SelectedValueBinding;
                    _SelectedValueBinding = value;
                    CoerceValue(IsReadOnlyProperty);
                    CoerceValue(SortMemberPathProperty);
                    OnSelectedValueBindingChanged(oldBinding, _SelectedValueBinding);
                }
            }
        }

        private BindingBase _SelectedItemBinding;
        /// <summary>
        /// The binding that will be applied to the SelectedItem property of the ComboBoxValue.
        /// </summary>
        /// <remarks>
        /// This isn't a DP because if it were getting the value would evaluate the binding.
        /// </remarks>
        public virtual BindingBase SelectedItemBinding
        {
            get => _SelectedItemBinding;
            set
            {
                if (_SelectedItemBinding != value)
                {
                    BindingBase oldBinding = _SelectedItemBinding;
                    _SelectedItemBinding = value;
                    CoerceValue(IsReadOnlyProperty);
                    CoerceValue(SortMemberPathProperty);
                    OnSelectedItemBindingChanged(oldBinding, _SelectedItemBinding);
                }
            }
        }

        public override BindingBase ClipboardContentBinding
        {
            get => base.ClipboardContentBinding ?? EffectiveBinding;
            set => base.ClipboardContentBinding = value;
        }

        /// <summary>
        /// Chooses either SelectedItemBinding, TextBinding, SelectedValueBinding or  based which are set.
        /// </summary>
        private BindingBase EffectiveBinding
            => SelectedItemBinding ?? SelectedValueBinding ?? TextBinding;

        /// <summary>
        /// The DependencyProperty for ItemsSource.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            ItemsControl.ItemsSourceProperty.AddOwner(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(null, NotifyPropertyChangeForRefreshContent));
        /// <summary>
        /// The ComboBox will attach to this ItemsSource.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// The DependencyProperty for the DisplayMemberPath property.
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            ItemsControl.DisplayMemberPathProperty.AddOwner(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(string.Empty, NotifyPropertyChangeForRefreshContent));
        /// <summary>
        /// DisplayMemberPath is a simple way to define a default template that describes how to convert Items into UI elements by using the specified path.
        /// </summary>
        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        /// <summary>
        /// SelectedValuePath DependencyProperty
        /// </summary>
        public static readonly DependencyProperty SelectedValuePathProperty =
            Selector.SelectedValuePathProperty.AddOwner(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(string.Empty, NotifyPropertyChangeForRefreshContent));
        /// <summary>
        /// The path used to retrieve the SelectedValue from the SelectedItem
        /// </summary>
        public string SelectedValuePath
        {
            get => (string)GetValue(SelectedValuePathProperty);
            set => SetValue(SelectedValuePathProperty, value);
        }

        /// <summary>
        /// The DependencyProperty for the Style property.
        /// </summary>
        public static readonly DependencyProperty StyleProperty =
            FrameworkElement.StyleProperty.AddOwner(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(null));
        /// <summary>
        /// A style that is applied to the generated element when not editing.
        /// The TargetType of the style depends on the derived column class.
        /// </summary>
        public Style Style
        {
            get => (Style)GetValue(StyleProperty);
            set => SetValue(StyleProperty, value);
        }

        public ObservableCollection<GroupStyle> GroupStyle { get; } = [];

        private GroupStyleSelector _GroupStyleSelector;
        public GroupStyleSelector GroupStyleSelector
        {
            get => _GroupStyleSelector;
            set
            {
                _GroupStyleSelector = value;
                GroupStyleSelectorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        static DataGridComboBoxColumn()
        {
            SortMemberPathProperty.OverrideMetadata(typeof(DataGridComboBoxColumn), new FrameworkPropertyMetadata(null, OnCoerceSortMemberPath));
            static object OnCoerceSortMemberPath(DependencyObject d, object baseValue)
            {
                DataGridComboBoxColumn Column = (DataGridComboBoxColumn)d;

                string SortMemberPath = (string)baseValue;
                if (string.IsNullOrEmpty(SortMemberPath))
                {
                    string BindingSortMemberPath = DependencyHelper.GetPathFromBinding(Column.EffectiveBinding as Binding);
                    if (!string.IsNullOrEmpty(BindingSortMemberPath))
                        SortMemberPath = BindingSortMemberPath;
                }

                return SortMemberPath;
            }
        }

        protected override bool OnCoerceIsReadOnly(bool baseValue)
            => DependencyHelper.IsOneWay(EffectiveBinding) || base.OnCoerceIsReadOnly(baseValue);

        protected override FrameworkElement GenerateElement(DataGridCell Cell, object DataItem)
        {
            ComboBox Element = new()
            {
                Style = Style ?? DefaultStyle,
                GroupStyleSelector = GroupStyleSelector,
            };

            // GroupStyle
            foreach (GroupStyle Style in GroupStyle)
                Element.GroupStyle.Add(Style);

            Element.DropDownOpened += (s, e) =>
            {
                CellEditingEventArgs Arg = new(this, Cell);
                RaiseBeforeEditing(Arg);

                if (Arg.Handled)
                    Element.IsDropDownOpen = false;
            };
            Element.DropDownClosed += (s, e) => Cell.Focus();
            Element.PreviewMouseDown += (s, e) =>
            {
                if (!Element.BindingGroup.HasValidationError &&
                    DataGridOwner is DataGrid Owner &&
                    Owner.HasCellValidationError)
                {
                    Owner.SelectedItem = DataItem;
                    Cell.Focus();
                    e.Handled = true;
                }
            };
            Element.SelectionChanged += (s, e) =>
            {
                if (Element.IsLoaded)
                {
                    BindingOperations.GetBindingExpression(Element, Selector.SelectedItemProperty)?.UpdateSource();
                    BindingOperations.GetBindingExpression(Element, Selector.SelectedValueProperty)?.UpdateSource();
                    BindingOperations.GetBindingExpression(Element, ComboBox.TextProperty)?.UpdateSource();
                }
            };

            GroupStyle.CollectionChanged += (s, e) =>
            {
                void Add()
                {
                    foreach (GroupStyle Data in e.NewItems.OfType<GroupStyle>())
                        Element.GroupStyle.Add(Data);
                }
                void Remove()
                {
                    foreach (GroupStyle Data in e.OldItems.OfType<GroupStyle>())
                        Element.GroupStyle.Remove(Data);
                }

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            Add();
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            Remove();
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            Remove();
                            Add();
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            Element.GroupStyle.Clear();
                            break;
                        }
                }
            };
            GroupStyleSelectorChanged += (s, e) => Element.GroupStyleSelector = GroupStyleSelector;

            ApplyColumnProperties(Cell, Element);
            return Element;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
            => throw new NotImplementedException();

        private void ApplyColumnProperties(DataGridCell Cell, ComboBox Element)
        {
            //Element.ApplyBinding(new Binding(nameof(Cell.DataContext)) { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridCell), 1) }, FrameworkElement.DataContextProperty);
            Element.ApplyBinding(new Binding(nameof(IsReadOnly)) { Source = this }, ComboBox.IsReadOnlyProperty);
            Element.ApplyBinding(SelectedItemBinding, Selector.SelectedItemProperty);
            Element.ApplyBinding(SelectedValueBinding, Selector.SelectedValueProperty);
            Element.ApplyBinding(TextBinding, ComboBox.TextProperty);

            this.SyncColumnProperty(Element, Selector.SelectedValuePathProperty, SelectedValuePathProperty);
            this.SyncColumnProperty(Element, ItemsControl.DisplayMemberPathProperty, DisplayMemberPathProperty);
            this.SyncColumnProperty(Element, ItemsControl.ItemsSourceProperty, ItemsSourceProperty);

            RestoreFlowDirection(Element, Cell);
        }

        /// <summary>
        /// Called when SelectedValueBinding changes.
        /// </summary>
        /// <param name="oldBinding">The old binding.</param>
        /// <param name="newBinding">The new binding.</param>
        protected virtual void OnSelectedValueBindingChanged(BindingBase oldBinding, BindingBase newBinding)
            => NotifyPropertyChanged(nameof(SelectedValueBinding));

        /// <summary>
        /// Called when SelectedItemBinding changes.
        /// </summary>
        /// <param name="oldBinding">The old binding.</param>
        /// <param name="newBinding">The new binding.</param>
        protected virtual void OnSelectedItemBindingChanged(BindingBase oldBinding, BindingBase newBinding)
            => NotifyPropertyChanged(nameof(SelectedItemBinding));

        /// <summary>
        /// Called when TextBinding changes.
        /// </summary>
        /// <param name="oldBinding">The old binding.</param>
        /// <param name="newBinding">The new binding.</param>
        protected virtual void OnTextBindingChanged(BindingBase oldBinding, BindingBase newBinding)
            => NotifyPropertyChanged(nameof(TextBinding));

    }
}