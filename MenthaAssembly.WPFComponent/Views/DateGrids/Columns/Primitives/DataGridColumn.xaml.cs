using MenthaAssembly.MarkupExtensions;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(DependencyMemberPath))]
    public abstract class DataGridColumn : System.Windows.Controls.DataGridColumn
    {
        public event EventHandler<CellInputEventArgs> Input;
        public event EventHandler<CellBeforeEditingEventArgs> BeforeEditing;
        public event EventHandler<CellCancelEditingEventArgs> CancelEditing;

        internal static ComponentResourceKey DefaultStyleKey { get; } = new ComponentResourceKey(typeof(DataGridColumn), nameof(DefaultStyle));

        public static Style DefaultStyle
            => Application.Current.TryFindResource(DefaultStyleKey) as Style;

        protected internal abstract bool AllowEditingMode { get; }

        public bool InputOverrideContent { set; get; }

        public static readonly DependencyProperty EditableNewItemPlaceholderProperty =
              DependencyProperty.Register(nameof(EditableNewItemPlaceholder), typeof(bool), typeof(DataGridColumn), new FrameworkPropertyMetadata(true, NotifyPropertyChangeForRefreshContent));
        public bool EditableNewItemPlaceholder
        {
            get => (bool)GetValue(EditableNewItemPlaceholderProperty);
            set => SetValue(EditableNewItemPlaceholderProperty, value);
        }

        public static readonly DependencyProperty CanUserFilterProperty =
              DependencyProperty.Register(nameof(CanUserFilter), typeof(bool), typeof(DataGridColumn), new FrameworkPropertyMetadata(true, NotifyPropertyChangeForRefreshContent, OnCoerceCanUserFilter));
        public bool CanUserFilter
        {
            get => (bool)GetValue(CanUserFilterProperty);
            set => SetValue(CanUserFilterProperty, value);
        }

        public static readonly DependencyProperty FilterMemberPathProperty =
              DependencyProperty.Register(nameof(FilterMemberPath), typeof(string), typeof(DataGridColumn),
                  new FrameworkPropertyMetadata(null, (d, e) => ((DataGridColumn)d).CoerceValue(CanUserFilterProperty)));
        public string FilterMemberPath
        {
            get => (string)GetValue(FilterMemberPathProperty);
            set => SetValue(FilterMemberPathProperty, value);
        }

        private static readonly DependencyPropertyKey IsFilterActivePropertyKey =
              DependencyProperty.RegisterReadOnly(nameof(IsFilterActive), typeof(bool), typeof(DataGridColumn), new FrameworkPropertyMetadata(false, NotifyPropertyChangeForRefreshContent));
        public static readonly DependencyProperty IsFilterActiveProperty = IsFilterActivePropertyKey.DependencyProperty;
        public bool IsFilterActive
            => (bool)GetValue(IsFilterActiveProperty);

        public StringCollection DependencyMemberPath { get; } = [];

        protected internal new DataGrid DataGridOwner
            => (DataGrid)base.DataGridOwner;

        static DataGridColumn()
        {
            if (ReflectionHelper.TryGetType("DataGridHelper", "System.Windows.Controls", out Type Helper))
                _ = Helper.TryGetStaticInternalMethod(nameof(RestoreFlowDirection), out RestoreFlowDirectionMethod);
        }

        protected virtual bool OnCoerceCanUserFilter(bool baseValue)
        {
            if (!baseValue)
                return false;

            if (string.IsNullOrEmpty(FilterMemberPath))
                return false;

            return DataGridOwner is DataGrid Grid &&
                   Grid.CanUserFilterColumns &&
                   !Grid.IsExternalColumnFilterUsed;
        }

        protected sealed override FrameworkElement GenerateElement(System.Windows.Controls.DataGridCell cell, object dataItem)
            => GenerateElement((DataGridCell)cell, dataItem);

        protected sealed override FrameworkElement GenerateEditingElement(System.Windows.Controls.DataGridCell cell, object dataItem)
            => GenerateEditingElement((DataGridCell)cell, dataItem);

        protected abstract FrameworkElement GenerateElement(DataGridCell cell, object dataItem);

        protected abstract FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem);

        protected internal virtual void RaiseBeforeEditing(CellBeforeEditingEventArgs e)
            => BeforeEditing?.Invoke(this, e);

        protected internal void SetIsFilterActive(bool Value)
            => SetValue(IsFilterActivePropertyKey, Value);

        protected override object PrepareCellForEdit(FrameworkElement Element, RoutedEventArgs e)
        {
            if (e == null)
                return base.PrepareCellForEdit(Element, e);

            // Get Focusable Editing Element
            Element = GetFocusableElement(Element);

            // Focus
            Element?.Focus();

            // TextBox
            if (Element is TextBox ThisBox)
            {
                string Value = ThisBox.Text;
                if (e is TextCompositionEventArgs textArgs)
                {
                    // If text input started the edit, then replace the text with what was typed.
                    if (string.IsNullOrEmpty(Value) ||
                        InputOverrideContent)
                    {
                        // Convert text the user has typed into the appropriate string to enter into the editable TextBox
                        static string ConvertTextForEdit(string s)
                            => s == "\b" ? string.Empty : s;    // Backspace becomes the empty string

                        ThisBox.Text = ConvertTextForEdit(textArgs.Text);
                    }

                    // Place the caret after the end of the text.
                    string Content = ThisBox.Text;
                    if (!string.IsNullOrEmpty(Content))
                        ThisBox.Select(Content.Length, 0);
                }
                else
                {
                    static bool PlaceCaretOnTextBox(TextBox textBox, Point position)
                    {
                        int characterIndex = textBox.GetCharacterIndexFromPoint(position, false);
                        if (characterIndex >= 0)
                        {
                            textBox.Select(characterIndex, 0);
                            return true;
                        }

                        return false;
                    }

                    // If a mouse click started the edit, then place the caret under the mouse.
                    if ((e is not MouseButtonEventArgs) || !PlaceCaretOnTextBox(ThisBox, Mouse.GetPosition(ThisBox)))
                    {
                        // If the mouse isn't over the textbox or something else started the edit, then select the text.
                        ThisBox.SelectAll();

                        // Avoid override content
                        if (!string.IsNullOrEmpty(Value))
                            e.Handled = true;
                    }
                }

                return Value;
            }

            return base.PrepareCellForEdit(Element, e);
        }
        private static FrameworkElement GetFocusableElement(FrameworkElement Element)
        {
            if (Element is TextBox)
                return Element;

            if (Element is ContentPresenter)
                return VisualTreeHelper.GetChildrenCount(Element) == 1 &&
                       VisualTreeHelper.GetChild(Element, 0) is FrameworkElement Child ? GetFocusableElement(Child) : null;

            if (Element is Panel)
                return Element.FindVisualChildren<FrameworkElement>().FirstOrDefault(i => i.Focusable is true);

            return Element.Focusable ? Element : null;
        }

        protected override void CancelCellEdit(FrameworkElement EditingElement, object UneditedValue)
        {
            CellCancelEditingEventArgs e = new(this, EditingElement, UneditedValue);
            CancelEditing?.Invoke(this, e);

            if (!e.Handled)
                base.CancelCellEdit(EditingElement, UneditedValue);
        }

        protected internal void RaiseInput(DataGridCell Cell, InputEventArgs TriggerEventArgs, object DataContext)
        {
            CellInputEventArgs e = new(Cell, TriggerEventArgs, DataContext);
            Input?.Invoke(this, e);

            if (!e.Handled)
                OnInput(e);

            if (e.BeginEdit)
                DataGridOwner.BeginEdit(TriggerEventArgs);
        }

        protected virtual void OnInput(CellInputEventArgs e)
        {

        }

        public override void OnPastingCellClipboardContent(object Item, object CellContent)
            => PasteCellClipboardContent(Item, CellContent, out _);

        internal bool PasteCellClipboardContent(object Item, object CellContent, out object Content)
        {
            Content = null;

            if (ClipboardContentBinding is not BindingBase Binding)
                return false;

            if (!TryRaisePastingCellClipboardContent(Item, CellContent, out object PastingContent))
                return false;

            Content = PastingContent;
            if (DataGridOwner.GetCell(Item, this) is not DataGridCell Cell)
                return false;

            PasteCellClipboardContent(Cell, Binding, Content);
            return true;
        }
        internal bool PasteDetachedCellClipboardContent(object Item, object CellContent, out object Content)
        {
            Content = null;

            if (ClipboardContentBinding is not BindingBase Binding)
                return false;

            if (!TryRaisePastingCellClipboardContent(Item, CellContent, out object PastingContent))
                return false;

            Content = PastingContent;

            ClipboardBindingTarget Target = new()
            {
                DataContext = Item,
            };

            DependencyProperty dp = ClipboardBindingTarget.CellClipboardProperty;
            BindingOperations.SetBinding(Target, dp, Binding.CloneBindingWithoutValidation(BindingMode.TwoWay));
            Target.SetValue(dp, Content);
            BindingOperations.GetBindingExpression(Target, dp).UpdateSource();
            BindingOperations.ClearBinding(Target, dp);
            return true;
        }

        internal void PasteDetachedCellClipboardContent(object Item, object CellContent)
            => PasteDetachedCellClipboardContent(Item, CellContent, out _);

        private static void PasteCellClipboardContent(DependencyObject Target, BindingBase Binding, object Content)
        {
            DependencyProperty dp = DataGridCell.CellClipboardProperty;
            BindingOperations.SetBinding(Target, dp, Binding.CloneBindingWithoutValidation(BindingMode.TwoWay));
            Target.SetValue(dp, Content);

            BindingExpressionBase Expression = BindingOperations.GetBindingExpressionBase(Target, dp);
            Expression.UpdateSource();
            BindingOperations.ClearBinding(Target, dp);
        }

        private bool TryRaisePastingCellClipboardContent(object Item, object CellContent, out object Content)
        {
            // Raise the event to give a chance for external listeners to modify the cell content
            // before it gets stored into the cell.
            DataGridCellClipboardEventArgs e = new(Item, this, CellContent);
            ReflectionHelper.RaiseEvent(this, nameof(PastingCellClipboardContent), e);
            Content = e.Content;
            return true;
        }

        /// <summary>
        /// Method used as property changed callback for properties which need RefreshCellContent to be called
        /// </summary>
        protected static void NotifyPropertyChangeForRefreshContent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(d is DataGridColumn, "d should be a DataGridColumn");
            ((DataGridColumn)d).NotifyPropertyChanged(e.Property.Name);
        }

        private static object OnCoerceCanUserFilter(DependencyObject d, object baseValue)
            => ((DataGridColumn)d).OnCoerceCanUserFilter((bool)baseValue);

        private static readonly MethodInfo RestoreFlowDirectionMethod;
        protected static void RestoreFlowDirection(FrameworkElement Element, DataGridCell Cell)
            => RestoreFlowDirectionMethod?.Invoke(null, [Element, Cell]);

        private sealed class ClipboardBindingTarget : FrameworkElement
        {
            public static readonly DependencyProperty CellClipboardProperty =
                DependencyProperty.Register(nameof(CellClipboard), typeof(object), typeof(ClipboardBindingTarget));

            public object CellClipboard
            {
                get => GetValue(CellClipboardProperty);
                set => SetValue(CellClipboardProperty, value);
            }

        }

    }
}
