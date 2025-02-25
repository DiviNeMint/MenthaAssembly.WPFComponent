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

        public static readonly DependencyProperty DoubleClickToEditProperty =
              DependencyProperty.Register("DoubleClickToEdit", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(true,
                  (d, e) =>
                  {
                      if (d is EditableTextBlock This)
                          This.OnDoubleClickToEditChanged(e.ToChangedEventArgs<bool>());
                  }));
        public bool DoubleClickToEdit
        {
            get => (bool)GetValue(DoubleClickToEditProperty);
            set => SetValue(DoubleClickToEditProperty, value);
        }

        public static readonly DependencyPropertyKey IsEditingPropertyKey =
            DependencyProperty.RegisterReadOnly("IsEditing", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(false));
        public bool IsEditing
            => (bool)GetValue(IsEditingPropertyKey.DependencyProperty);

        static EditableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock), new FrameworkPropertyMetadata(typeof(EditableTextBlock)));
        }

        private void OnDoubleClickToEditChanged(ChangedEventArgs<bool> e)
            => IsHitTestVisible = e.NewValue;

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            SetValue(IsEditingPropertyKey, true);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            SetValue(IsEditingPropertyKey, false);

            if (!DoubleClickToEdit)
                IsHitTestVisible = false;

            LastFocusedElement = null;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Enter)
            {
                SetValue(IsEditingPropertyKey, false);

                if (LastFocusedElement is null)
                {
                    Keyboard.ClearFocus();
                    return;
                }

                Keyboard.Focus(LastFocusedElement);
            }
        }

        private IInputElement LastFocusedElement;
        public void StartEditing()
        {
            if (!DoubleClickToEdit)
                IsHitTestVisible = true;

            SetValue(IsEditingPropertyKey, true);

            LastFocusedElement = Keyboard.FocusedElement;
            Focus();
        }

    }
}