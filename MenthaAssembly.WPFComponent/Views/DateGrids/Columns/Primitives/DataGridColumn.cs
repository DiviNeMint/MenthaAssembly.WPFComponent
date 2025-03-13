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

        protected internal abstract bool AllowEditingMode { get; }

        public bool InputOverrideContent { set; get; }

        public static readonly DependencyProperty EditableNewItemPlaceholderProperty =
              DependencyProperty.Register(nameof(EditableNewItemPlaceholder), typeof(bool), typeof(DataGridColumn), new FrameworkPropertyMetadata(true, NotifyPropertyChangeForRefreshContent));
        public bool EditableNewItemPlaceholder
        {
            get => (bool)GetValue(EditableNewItemPlaceholderProperty);
            set => SetValue(EditableNewItemPlaceholderProperty, value);
        }

        public StringCollection DependencyMemberPath { get; } = [];

        static DataGridColumn()
        {
            if (ReflectionHelper.TryGetType("DataGridHelper", "System.Windows.Controls", out Type Helper))
                _ = Helper.TryGetStaticInternalMethod(nameof(RestoreFlowDirection), out RestoreFlowDirectionMethod);
        }

        protected sealed override FrameworkElement GenerateElement(System.Windows.Controls.DataGridCell cell, object dataItem)
            => GenerateElement((DataGridCell)cell, dataItem);

        protected sealed override FrameworkElement GenerateEditingElement(System.Windows.Controls.DataGridCell cell, object dataItem)
            => GenerateEditingElement((DataGridCell)cell, dataItem);

        protected abstract FrameworkElement GenerateElement(DataGridCell cell, object dataItem);

        protected abstract FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem);

        protected internal virtual void RaiseBeforeEditing(CellBeforeEditingEventArgs e)
            => BeforeEditing?.Invoke(this, e);

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

        //protected override bool CommitCellEdit(FrameworkElement Element)
        //    => Element.BindingGroup?.Validate() ?? base.CommitCellEdit(Element);

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
        {
            if (ClipboardContentBinding is not BindingBase Binding)
                return;

            // Raise the event to give a chance for external listeners to modify the cell content
            // before it gets stored into the cell
            DataGridCellClipboardEventArgs e = new(Item, this, CellContent);
            ReflectionHelper.RaiseEvent(this, nameof(PastingCellClipboardContent), e);

            // Event handlers can cancel Paste of a cell by setting its content to null
            if (e.Content != null &&
                DataGridOwner.GetCell(Item, this) is DataGridCell Cell)
            {
                DependencyProperty dp = DataGridCell.CellClipboardProperty;
                BindingOperations.SetBinding(Cell, dp, Binding.Clone(BindingMode.TwoWay));

                // Set the new value
                Cell.SetValue(dp, e.Content);

                // Update the source
                BindingOperations.GetBindingExpression(Cell, dp).UpdateSource();

                // Whether valid or not, remove the binding.  The binding group will
                // remember the proposed value
                BindingOperations.ClearBinding(Cell, dp);
            }
        }

        /// <summary>
        /// Method used as property changed callback for properties which need RefreshCellContent to be called
        /// </summary>
        protected static void NotifyPropertyChangeForRefreshContent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(d is DataGridColumn, "d should be a DataGridColumn");
            ((DataGridColumn)d).NotifyPropertyChanged(e.Property.Name);
        }

        private static readonly MethodInfo RestoreFlowDirectionMethod;
        protected static void RestoreFlowDirection(FrameworkElement Element, DataGridCell Cell)
            => RestoreFlowDirectionMethod?.Invoke(null, [Element, Cell]);

    }
}