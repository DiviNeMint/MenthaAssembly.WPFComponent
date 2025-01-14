using System.Windows;
using System.Windows.Input;

namespace MenthaAssembly.Views
{
    internal static class DataGridHelper
    {
        public static void SyncColumnProperty(this DataGridColumn Column, DependencyObject Content, DependencyProperty ContentProperty, DependencyProperty ColumnProperty)
        {
            if (Column.IsDefaultValue(ColumnProperty))
                Content.ClearValue(ContentProperty);
            else
                Content.SetValue(ContentProperty, Column.GetValue(ColumnProperty));
        }

        public static bool IsDataGridTextBoxBeginEdit(CellInputEventArgs e)
        {
            // Text input will start an edit.
            // Escape is meant to be for CancelEdit. But DataGrid
            // may not handle KeyDown event for Escape if nothing
            // is cancelable. Such KeyDown if unhandled by others
            // will ultimately get promoted to TextInput and be handled
            // here. But BeginEdit on escape could be confusing to the user.
            // Hence escape key is special case and BeginEdit is performed if
            // there is atleast one non espace key character.
            if (HasNonEscapeCharacters(e.TriggerEventArgs as TextCompositionEventArgs))
                return true;

            if (e.Cell is DataGridCell Cell && Cell.IsEditing &&
                IsImeProcessed(e.TriggerEventArgs as KeyEventArgs))
            {
                return true;

                ////
                //// The TextEditor for the TextBox establishes contact with the IME
                //// engine lazily at background priority. However in this case we
                //// want to IME engine to know about the TextBox in earnest before
                //// PostProcessing this input event. Only then will the IME key be
                //// recorded in the TextBox. Hence the call to synchronously drain
                //// the Dispatcher queue.
                ////
                //Dispatcher.Invoke(delegate () { }, System.Windows.Threading.DispatcherPriority.Background);
            }

            return false;
        }

        /// <summary>
        ///     Helper to check if TextCompositionEventArgs.Text has any non
        ///     escape characters.
        /// </summary>
        public static bool HasNonEscapeCharacters(TextCompositionEventArgs e)
        {
            const char EscapeChar = '\u001b';
            if (e != null)
            {
                string text = e.Text;
                for (int i = 0, count = text.Length; i < count; i++)
                {
                    if (text[i] != EscapeChar)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Helper to check if KeyEventArgs.Key is ImeProcessed.
        /// </summary>
        public static bool IsImeProcessed(KeyEventArgs e)
            => e != null && e.Key == Key.ImeProcessed;

    }
}