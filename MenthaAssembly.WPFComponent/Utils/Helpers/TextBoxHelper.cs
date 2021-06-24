using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace MenthaAssembly
{
    public static class TextBoxHelper
    {
        public static double DelayNotifyInterval { get; set; } = 500d;

        public static readonly DependencyProperty InputModeProperty = DependencyProperty.RegisterAttached("InputMode", typeof(KeyboardInputMode), typeof(TextBoxHelper), new PropertyMetadata(KeyboardInputMode.All,
                (d, e) =>
                {
                    if (d is TextBox This)
                    {
                        bool IsModeAll = e.NewValue is KeyboardInputMode.All;
                        InputMethod.SetIsInputMethodEnabled(This, IsModeAll);
                        if (!IsModeAll)
                        {
                            if (e.OldValue is KeyboardInputMode.All)
                                This.KeyDown += OnKeyDown;
                        }
                        else
                        {
                            This.KeyDown -= OnKeyDown;
                        }
                    }
                }));
        public static KeyboardInputMode GetInputMode(TextBox obj)
            => (KeyboardInputMode)obj.GetValue(InputModeProperty);
        public static void SetInputMode(TextBox obj, KeyboardInputMode value)
            => obj.SetValue(InputModeProperty, value);

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.RegisterAttached("Minimum", typeof(object), typeof(TextBoxHelper), new PropertyMetadata(default));
        public static object GetMinimum(TextBox obj)
            => obj.GetValue(MinimumProperty);
        public static void SetMinimum(TextBox obj, object value)
            => obj.SetValue(MinimumProperty, value);

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.RegisterAttached("Maximum", typeof(object), typeof(TextBoxHelper), new PropertyMetadata(default));
        public static object GetMaximum(TextBox obj)
            => obj.GetValue(MaximumProperty);
        public static void SetMaximum(TextBox obj, object value)
            => obj.SetValue(MaximumProperty, value);

        public static readonly DependencyProperty DeltaProperty = DependencyProperty.RegisterAttached("Delta", typeof(object), typeof(TextBoxHelper), new PropertyMetadata(default));
        public static object GetDelta(TextBox obj)
            => obj.GetValue(DeltaProperty);
        public static void SetDelta(TextBox obj, object value)
            => obj.SetValue(DeltaProperty, value);

        public static readonly DependencyProperty CombineDeltaProperty = DependencyProperty.RegisterAttached("CombineDelta", typeof(object), typeof(TextBoxHelper), new PropertyMetadata(default));
        public static object GetCombineDelta(TextBox obj)
            => obj.GetValue(CombineDeltaProperty);
        public static void SetCombineDelta(TextBox obj, object value)
            => obj.SetValue(CombineDeltaProperty, value);

        public static readonly DependencyProperty ValueTypeProperty = DependencyProperty.RegisterAttached("ValueType", typeof(Type), typeof(TextBoxHelper), new PropertyMetadata(default,
                (d, e) =>
                {
                    if (d is TextBox This &&
                        e.NewValue is Type ValueType)
                    {
                        switch (ValueType.Name)
                        {
                            case nameof(SByte):
                            case nameof(Int16):
                            case nameof(Int32):
                            case nameof(Int64):
                                SetDelta(This, 1);
                                SetCombineDelta(This, 10);
                                SetMaximum(This, ValueType.GetField("MaxValue").GetValue(null));
                                SetMinimum(This, ValueType.GetField("MinValue").GetValue(null));
                                SetInputMode(This, KeyboardInputMode.NegativeNumber);
                                This.PreviewMouseWheel += OnPreviewMouseWheel;
                                This.PreviewKeyDown += OnPreviewKeyDown;
                                This.GotKeyboardFocus += OnGotKeyboardFocus;
                                This.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                                break;
                            case nameof(Byte):
                            case nameof(UInt16):
                            case nameof(UInt32):
                            case nameof(UInt64):
                                SetDelta(This, 1);
                                SetCombineDelta(This, 10);
                                SetMaximum(This, ValueType.GetField("MaxValue").GetValue(null));
                                SetMinimum(This, ValueType.GetField("MinValue").GetValue(null));
                                SetInputMode(This, KeyboardInputMode.Number);
                                This.PreviewMouseWheel += OnPreviewMouseWheel;
                                This.PreviewKeyDown += OnPreviewKeyDown;
                                This.GotKeyboardFocus += OnGotKeyboardFocus;
                                This.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                                break;
                            case nameof(Decimal):
                            case nameof(Single):
                            case nameof(Double):
                                SetDelta(This, 1);
                                SetCombineDelta(This, 10);
                                SetMaximum(This, ValueType.GetField("MaxValue").GetValue(null));
                                SetMinimum(This, ValueType.GetField("MinValue").GetValue(null));
                                SetInputMode(This, KeyboardInputMode.NegativeNumberAndDot);
                                This.PreviewMouseWheel += OnPreviewMouseWheel;
                                This.PreviewKeyDown += OnPreviewKeyDown;
                                This.GotKeyboardFocus += OnGotKeyboardFocus;
                                This.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
                                break;
                            default:
                                SetInputMode(This, KeyboardInputMode.All);
                                This.PreviewMouseWheel -= OnPreviewMouseWheel;
                                This.PreviewKeyDown -= OnPreviewKeyDown;
                                This.GotKeyboardFocus -= OnGotKeyboardFocus;
                                This.PreviewMouseLeftButtonDown -= OnMouseLeftButtonDown;
                                break;
                        }
                    }
                }));
        public static Type GetValueType(TextBox obj)
            => (Type)obj.GetValue(ValueTypeProperty);
        public static void SetValueType(TextBox obj, Type value)
            => obj.SetValue(ValueTypeProperty, value);

        private static readonly DependencyProperty DelayTokenProperty =
            DependencyProperty.RegisterAttached("DelayToken", typeof(DelayActionToken), typeof(TextBoxHelper), new PropertyMetadata(null));
        private static DelayActionToken GetDelayToken(DependencyObject obj)
            => (DelayActionToken)obj.GetValue(DelayTokenProperty);
        private static void SetDelayToken(DependencyObject obj, DelayActionToken value)
            => obj.SetValue(DelayTokenProperty, value);

        public static readonly DependencyProperty EnableDelayNotifyTextProperty = DependencyProperty.RegisterAttached("EnableDelayNotifyText", typeof(bool), typeof(TextBoxHelper), new PropertyMetadata(false,
            (d, e) =>
            {
                if (d is TextBox This)
                {
                    if (e.NewValue is true)
                    {
                        This.TextChanged += OnTextChanged;
                    }
                    else
                    {
                        This.TextChanged -= OnTextChanged;

                        if (GetDelayToken(This) is DelayActionToken Token)
                            Token.Cancel();
                    }
                }
            }));

        public static bool GetEnableDelayNotifyText(TextBox obj)
            => (bool)obj.GetValue(EnableDelayNotifyTextProperty);
        public static void SetEnableDelayNotifyText(TextBox obj, bool value)
            => obj.SetValue(EnableDelayNotifyTextProperty, value);

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Tab)
                return;

            if (sender is TextBox This)
            {
                KeyboardInputMode Mode = GetInputMode(This);

                // 0 - 9
                if ((Mode & KeyboardInputMode.Number) > 0 &&
                    ((Key.D0 <= e.Key && e.Key <= Key.D9) || (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9)))
                    return;

                // A - Z
                if ((Mode & KeyboardInputMode.Alphabet) > 0 &&
                    Key.A <= e.Key && e.Key <= Key.Z)
                    return;

                // .
                if ((Mode & KeyboardInputMode.Dot) > 0 &&
                    !This.Text.Contains(".") &&
                    (e.Key is Key.Decimal || e.Key is Key.OemPeriod))
                    return;

                // -
                if ((Mode & KeyboardInputMode.Negative) > 0 &&
                    !This.Text.Contains("-") &&
                    (e.Key is Key.Subtract || e.Key is Key.OemMinus))
                    return;

                e.Handled = true;
            }
        }

        public static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox This)
                This.SelectAll();
        }
        public static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox This &&
                !This.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                This.Focus();
            }
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TextBox This &&
                This.IsKeyboardFocusWithin)
            {
                Type ValueType = GetValueType(This);
                dynamic Max = Convert.ChangeType(GetMaximum(This), ValueType),
                        Min = Convert.ChangeType(GetMinimum(This), ValueType),
                        Value = string.IsNullOrEmpty(This.Text) ? Activator.CreateInstance(ValueType) :
                                                                  This.Text.ToValueType(ValueType),
                        Delta = Convert.ChangeType(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? GetCombineDelta(This) :
                                                                                                                           GetDelta(This), ValueType);

                This.Text = e.Delta > 0 ? (Max - Delta < Value ? Max : Value + Delta).ToString() :
                                          (Min + Delta > Value ? Min : Value - Delta).ToString();
                e.Handled = true;
            }
        }
        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Tab)
                return;

            if (sender is TextBox This)
            {
                if (e.Key is Key.Enter)
                {
                    if (GetDelayToken(This) is DelayActionToken Token)
                        Token.Cancel();

                    This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                }

                else if (e.Key is Key.Up)
                {
                    Type ValueType = GetValueType(This);
                    dynamic Max = Convert.ChangeType(GetMaximum(This), ValueType),
                            Value = string.IsNullOrEmpty(This.Text) ? Activator.CreateInstance(ValueType) : This.Text.ToValueType(ValueType),
                            Delta = Convert.ChangeType(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? GetCombineDelta(This) :
                                                                                                                               GetDelta(This), ValueType);
                    
                    This.Text = (Max - Delta < Value ? Max : Value + Delta).ToString();
                    e.Handled = true;
                }

                else if (e.Key is Key.Down)
                {
                    Type ValueType = GetValueType(This);
                    dynamic Min = Convert.ChangeType(GetMinimum(This), ValueType),
                            Value = string.IsNullOrEmpty(This.Text) ? Activator.CreateInstance(ValueType) : This.Text.ToValueType(ValueType),
                            Delta = Convert.ChangeType(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? GetCombineDelta(This) :
                                                                                                                               GetDelta(This), ValueType);

                    This.Text = (Min + Delta > Value ? Min : Value - Delta).ToString();
                    e.Handled = true;
                }

                else if (e.Key is Key.PageUp)
                {
                    Type ValueType = GetValueType(This);
                    dynamic Max = Convert.ChangeType(GetMaximum(This), ValueType),
                            Value = string.IsNullOrEmpty(This.Text) ? Activator.CreateInstance(ValueType) : This.Text.ToValueType(ValueType),
                            Delta = Convert.ChangeType(GetCombineDelta(This), ValueType);

                    This.Text = (Max - Delta < Value ? Max : Value + Delta).ToString();
                    e.Handled = true;
                }

                else if (e.Key is Key.PageDown)
                {
                    Type ValueType = GetValueType(This);
                    dynamic Min = Convert.ChangeType(GetMinimum(This), ValueType),
                            Value = string.IsNullOrEmpty(This.Text) ? Activator.CreateInstance(ValueType) : This.Text.ToValueType(ValueType),
                            Delta = Convert.ChangeType(GetCombineDelta(This), ValueType);

                    This.Text = (Min + Delta > Value ? Min : Value - Delta).ToString();
                    e.Handled = true;
                }
            }
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox This)
            {
                if (GetDelayToken(This) is DelayActionToken OldToken)
                    OldToken.Cancel();

                void DelayTask()
                {
                    SetDelayToken(This, null);

                    if (!string.IsNullOrEmpty(This.Text))
                        This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                }

                DelayActionToken Token = TimerHelper.DelayAction(DelayNotifyInterval, DelayTask, () => SetDelayToken(This, null));
                SetDelayToken(This, Token);
            }
        }

        public static object ToValueType(this string This, Type ValueType)
        {
            try
            {
                return Convert.ChangeType(This, ValueType);
            }
            catch
            {

            }
            return Activator.CreateInstance(ValueType);
        }

        public static void UpdateTextBindingWhenEnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter &&
                sender is TextBox This)
                This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

    }
}
