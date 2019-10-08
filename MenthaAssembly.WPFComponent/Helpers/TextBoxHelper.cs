using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MenthaAssembly
{
    public static class TextBoxHelper
    {
        public static readonly DependencyProperty InputModeProperty =
            DependencyProperty.RegisterAttached("InputMode", typeof(KeyboardInputMode), typeof(TextBox), new PropertyMetadata(KeyboardInputMode.All,
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

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox This)
            {
                switch (GetInputMode(This))
                {
                    case KeyboardInputMode.Number:
                        if (Key.D0 <= e.Key && e.Key <= Key.D9)
                            return;
                        if (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9)
                            return;
                        break;
                    case KeyboardInputMode.NegativeNumber:
                        if (Key.D0 <= e.Key && e.Key <= Key.D9)
                            return;
                        if (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9)
                            return;
                        if (!This.Text.Contains("-") &&
                           (e.Key is Key.Subtract || e.Key is Key.OemMinus))
                            return;
                        break;
                    case KeyboardInputMode.NumberAndDot:
                        if (Key.D0 <= e.Key && e.Key <= Key.D9)
                            return;
                        if (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9)
                            return;
                        if (!This.Text.Contains(".") &&
                            (e.Key is Key.Decimal || e.Key is Key.OemPeriod))
                            return;
                        break;
                    case KeyboardInputMode.NegativeNumberAndDot:
                        if (Key.D0 <= e.Key && e.Key <= Key.D9)
                            return;
                        if (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9)
                            return;
                        if (!This.Text.Contains(".") &&
                            (e.Key is Key.Decimal || e.Key is Key.OemPeriod))
                            return;
                        if (!This.Text.Contains("-") &&
                           (e.Key is Key.Subtract || e.Key is Key.OemMinus))
                            return;
                        break;
                }
            }
            e.Handled = true;
        }

        public static KeyboardInputMode GetInputMode(DependencyObject obj)
            => (KeyboardInputMode)obj.GetValue(InputModeProperty);
        public static void SetInputMode(DependencyObject obj, KeyboardInputMode value)
            => obj.SetValue(InputModeProperty, value);

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.RegisterAttached("Minimum", typeof(object), typeof(TextBox), new PropertyMetadata(default));
        public static object GetMinimum(DependencyObject obj)
            => obj.GetValue(MinimumProperty);
        public static void SetMinimum(DependencyObject obj, object value)
            => obj.SetValue(MinimumProperty, value);

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.RegisterAttached("Maximum", typeof(object), typeof(TextBox), new PropertyMetadata(default));
        public static object GetMaximum(DependencyObject obj)
            => obj.GetValue(MaximumProperty);
        public static void SetMaximum(DependencyObject obj, object value)
            => obj.SetValue(MaximumProperty, value);

        public static readonly DependencyProperty DeltaProperty =
            DependencyProperty.RegisterAttached("Delta", typeof(object), typeof(TextBox), new PropertyMetadata(default));
        public static object GetDelta(DependencyObject obj)
            => obj.GetValue(DeltaProperty);
        public static void SetDelta(DependencyObject obj, object value)
            => obj.SetValue(DeltaProperty, value);

        public static readonly DependencyProperty CombineDeltaProperty =
            DependencyProperty.RegisterAttached("CombineDelta", typeof(object), typeof(TextBox), new PropertyMetadata(default));
        public static object GetCombineDelta(DependencyObject obj)
            => obj.GetValue(CombineDeltaProperty);
        public static void SetCombineDelta(DependencyObject obj, object value)
            => obj.SetValue(CombineDeltaProperty, value);

        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.RegisterAttached("ValueType", typeof(Type), typeof(TextBox), new PropertyMetadata(default,
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
                                SetDelta(d, 1);
                                SetCombineDelta(d, 10);
                                SetMaximum(d, ValueType.GetField("MaxValue").GetValue(null));
                                SetMinimum(d, ValueType.GetField("MinValue").GetValue(null));
                                SetInputMode(d, KeyboardInputMode.NegativeNumber);
                                This.PreviewMouseWheel += OnPreviewMouseWheel;
                                This.PreviewKeyDown += OnPreviewKeyDown;
                                break;
                            case nameof(Byte):
                            case nameof(UInt16):
                            case nameof(UInt32):
                            case nameof(UInt64):
                                SetDelta(d, 1);
                                SetCombineDelta(d, 10);
                                SetMaximum(d, ValueType.GetField("MaxValue").GetValue(null));
                                SetMinimum(d, ValueType.GetField("MinValue").GetValue(null));
                                SetInputMode(d, KeyboardInputMode.Number);
                                This.PreviewMouseWheel += OnPreviewMouseWheel;
                                This.PreviewKeyDown += OnPreviewKeyDown;
                                break;
                            case nameof(Decimal):
                            case nameof(Single):
                            case nameof(Double):
                                SetDelta(d, 1);
                                SetCombineDelta(d, 10);
                                SetMaximum(d, ValueType.GetField("MaxValue").GetValue(null));
                                SetMinimum(d, ValueType.GetField("MinValue").GetValue(null));
                                SetInputMode(d, KeyboardInputMode.NegativeNumberAndDot);
                                This.PreviewMouseWheel += OnPreviewMouseWheel;
                                This.PreviewKeyDown += OnPreviewKeyDown;
                                break;
                            default:
                                SetInputMode(d, KeyboardInputMode.All);
                                This.PreviewMouseWheel -= OnPreviewMouseWheel;
                                This.PreviewKeyDown -= OnPreviewKeyDown;
                                break;
                        }
                    }
                }));

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TextBox This &&
                This.IsKeyboardFocusWithin)
            {
                dynamic Max = GetMaximum(This);
                dynamic Min = GetMinimum(This);
                Type ValueType = GetValueType(This);
                dynamic Value = Convert.ChangeType(This.Text, GetValueType(This));
                dynamic Delta = Convert.ChangeType(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? GetCombineDelta(This) : GetDelta(This), ValueType);

                This.Text = e.Delta > 0 ? (Max - Delta < Value ? Max : Value + Delta).ToString() :
                                          (Min + Delta > Value ? Min : Value - Delta).ToString();
                e.Handled = true;
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox This)
            {
                if (e.Key is Key.Up || e.Key is Key.Down)
                {
                    dynamic Max = GetMaximum(This);
                    dynamic Min = GetMinimum(This);
                    Type ValueType = GetValueType(This);
                    dynamic Value = Convert.ChangeType(This.Text, ValueType);
                    dynamic Delta = Convert.ChangeType(GetDelta(This), ValueType);

                    This.Text = e.Key is Key.Up ? (Max - Delta < Value ? Max : Value + Delta).ToString() :
                                                  (Min + Delta > Value ? Min : Value - Delta).ToString();
                    return;
                }
                if (e.Key is Key.PageUp || e.Key is Key.PageDown)
                {
                    dynamic Max = GetMaximum(This);
                    dynamic Min = GetMinimum(This);
                    Type ValueType = GetValueType(This);
                    dynamic Value = Convert.ChangeType(This.Text, GetValueType(This));
                    dynamic Delta = Convert.ChangeType(GetCombineDelta(This), ValueType);

                    This.Text = e.Key is Key.PageUp ? (Max - Delta < Value ? Max : Value + Delta).ToString() :
                                                      (Min + Delta > Value ? Min : Value - Delta).ToString();
                    return;
                }
            }
        }

        public static Type GetValueType(DependencyObject obj)
            => (Type)obj.GetValue(ValueTypeProperty);
        public static void SetValueType(DependencyObject obj, Type value)
            => obj.SetValue(ValueTypeProperty, value);



    }
}
