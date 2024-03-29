﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MenthaAssembly.Views
{
    public class PasswordBox : TextBox
    {
        public static readonly DependencyProperty IsPasswordShownProperty =
            DependencyProperty.Register("IsPasswordShown", typeof(bool), typeof(PasswordBox), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is PasswordBox This &&
                        This.Password is string Password &&
                        e.NewValue is bool NewValue)
                        This.SetText(NewValue ? Password : new string(This.PasswordChar, Password.Length), 0);
                }));
        public bool IsPasswordShown
        {
            get => (bool)GetValue(IsPasswordShownProperty);
            set => SetValue(IsPasswordShownProperty, value);
        }

        public static readonly DependencyProperty PasswordCharProperty =
            DependencyProperty.Register("PasswordChar", typeof(char), typeof(PasswordBox), new PropertyMetadata('●',
                (d, e) =>
                {
                    if (d is PasswordBox This &&
                        !This.IsPasswordShown &&
                        e.NewValue is char NewValue)
                        This.SetText(new string(NewValue, This.Password.Length), 0);
                }));
        public char PasswordChar
        {
            get => (char)GetValue(PasswordCharProperty);
            set => SetValue(PasswordCharProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(PasswordBox), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is PasswordBox This)
                    {
                        string NewValue = e.NewValue?.ToString() ?? string.Empty;
                        This.SetText(This.IsPasswordShown ? NewValue : new string(This.PasswordChar, NewValue.Length),
                                     This.IsTextChanging ? 0 : 1);
                    }
                }));
        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        private void SetText(string Value, int Offset)
        {
            int Index = this.SelectionStart + Offset;
            this.IsPasswordChanging = true;
            this.Text = Value;
            this.IsPasswordChanging = false;
            this.SelectionStart = Index;
        }

        static PasswordBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PasswordBox), new FrameworkPropertyMetadata(typeof(PasswordBox)));
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!IsPasswordShown)
            {
                Password = (SelectionLength > 0 ? Password.Remove(SelectionStart, SelectionLength) : Password)?.Insert(SelectionStart, e.Text) ?? e.Text;
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (!e.Handled && !IsPasswordShown)
            {
                if (e.Key == Key.Back && SelectionStart > 0)
                {
                    int Index = SelectionStart;
                    Password = SelectionLength > 0 ? Password.Remove(SelectionStart, SelectionLength) : Password.Remove(SelectionStart - 1, 1);
                    SelectionStart = Index;
                    e.Handled = true;
                }

                else if (e.Key == Key.Delete && SelectionStart < Text.Length)
                {
                    int Index = SelectionStart;
                    Password = SelectionLength > 0 ? Password.Remove(SelectionStart, SelectionLength) : Password.Remove(SelectionStart, 1);
                    SelectionStart = Index;
                    e.Handled = true;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (!e.Handled &&
                e.Key == Key.Enter)
                e.Handled = true;
        }

        protected bool IsTextChanging { set; get; } = false;
        protected bool IsPasswordChanging { set; get; } = false;
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (!e.Handled && !IsPasswordChanging)
            {
                IsTextChanging = true;
                Password = Text;
                IsTextChanging = false;
            }
        }

    }
}
