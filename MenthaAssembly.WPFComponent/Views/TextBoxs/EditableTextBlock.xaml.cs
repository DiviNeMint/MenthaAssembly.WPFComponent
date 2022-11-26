using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MenthaAssembly.Views
{
    public class EditableTextBlock : TextBox
    {
        public static readonly DependencyProperty TextTrimmingProperty =
            TextBlock.TextTrimmingProperty.AddOwner(typeof(EditableTextBlock), new PropertyMetadata(TextTrimming.CharacterEllipsis));
        public TextTrimming TextTrimming
        {
            get => (TextTrimming)GetValue(TextTrimmingProperty);
            set => SetValue(TextTrimmingProperty, value);
        }

        public static readonly DependencyPropertyKey IsEditingPropertyKey =
            DependencyProperty.RegisterReadOnly("IsEditing", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(false));
        public bool IsEditing
            => (bool)GetValue(IsEditingPropertyKey.DependencyProperty);

        static EditableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock), new FrameworkPropertyMetadata(typeof(EditableTextBlock)));
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            SetValue(IsEditingPropertyKey, true);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            SetValue(IsEditingPropertyKey, false);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Enter)
            {
                SetValue(IsEditingPropertyKey, false);
                Keyboard.ClearFocus();
            }
        }

    }
}