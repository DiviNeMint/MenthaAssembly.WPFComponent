using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MenthaAssembly.MarkupExtensions
{
    public static class TextBoxEx
    {
        #region ValueType
        private static readonly ConcurrentDictionary<Type, dynamic> MaxValues = new ConcurrentDictionary<Type, dynamic>(),
                                                                    MinValues = new ConcurrentDictionary<Type, dynamic>();

        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.RegisterAttached("ValueType", typeof(Type), typeof(TextBoxEx), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is TextBox This &&
                        e.NewValue is Type ValueType)
                    {
                        bool EnableAttach = false;
                        KeyboardInputMode InputMode = KeyboardInputMode.All;

                        if (ValueType.IsSignedIntegerType())
                        {
                            InputMode = KeyboardInputMode.NegativeNumber;
                            EnableAttach = true;
                        }
                        else if (ValueType.IsUnsignedIntegerType())
                        {
                            InputMode = KeyboardInputMode.Number;
                            EnableAttach = true;
                        }
                        else if (ValueType.IsDecimalType())
                        {
                            InputMode = KeyboardInputMode.NegativeNumberAndDot;
                            EnableAttach = true;
                        }

                        if (EnableAttach)
                        {
                            if (!MaxValues.ContainsKey(ValueType) &&
                                ValueType.TryGetConstant("MaxValue", out object Max))
                                MaxValues.AddOrUpdate(ValueType, Max, (k, v) => Max);

                            if (!MinValues.ContainsKey(ValueType) &&
                                ValueType.TryGetConstant("MinValue", out object Min))
                                MinValues.AddOrUpdate(ValueType, Min, (k, v) => Min);

                            SetDefaultInputMode(This, InputMode);
                            This.PreviewMouseWheel += OnPreviewMouseWheel;
                            This.PreviewKeyDown += OnPreviewKeyDown;
                        }
                        else
                        {
                            SetInputMode(This, InputMode);
                            This.PreviewMouseWheel -= OnPreviewMouseWheel;
                            This.PreviewKeyDown -= OnPreviewKeyDown;
                        }
                    }
                }));
        public static Type GetValueType(TextBox obj)
            => (Type)obj.GetValue(ValueTypeProperty);
        public static void SetValueType(TextBox obj, Type value)
            => obj.SetValue(ValueTypeProperty, value);

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.RegisterAttached("Minimum", typeof(object), typeof(TextBoxEx), new PropertyMetadata(null));
        public static object GetMinimum(TextBox obj)
            => obj.GetValue(MinimumProperty);
        public static void SetMinimum(TextBox obj, object value)
            => obj.SetValue(MinimumProperty, value);

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.RegisterAttached("Maximum", typeof(object), typeof(TextBoxEx), new PropertyMetadata(null));
        public static object GetMaximum(TextBox obj)
            => obj.GetValue(MaximumProperty);
        public static void SetMaximum(TextBox obj, object value)
            => obj.SetValue(MaximumProperty, value);

        public static readonly DependencyProperty DeltaProperty = DependencyProperty.RegisterAttached("Delta", typeof(object), typeof(TextBoxEx), new PropertyMetadata(1));
        public static object GetDelta(TextBox obj)
            => obj.GetValue(DeltaProperty);
        public static void SetDelta(TextBox obj, object value)
            => obj.SetValue(DeltaProperty, value);

        public static readonly DependencyProperty CombineDeltaProperty = DependencyProperty.RegisterAttached("CombineDelta", typeof(object), typeof(TextBoxEx), new PropertyMetadata(10));
        public static object GetCombineDelta(TextBox obj)
            => obj.GetValue(CombineDeltaProperty);
        public static void SetCombineDelta(TextBox obj, object value)
            => obj.SetValue(CombineDeltaProperty, value);

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TextBox This &&
                This.IsKeyboardFocusWithin)
            {
                Type ValueType = GetValueType(This);
                dynamic Value = This.Text.ToValueType(ValueType),
                        Delta = Convert.ChangeType(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? GetCombineDelta(This) :
                                                                                                                           GetDelta(This), ValueType);

                dynamic Temp;
                if (e.Delta > 0)
                {
                    dynamic Max = GetMaxValue(This, ValueType);
                    Temp = Value + Delta;
                    if (Max < Temp)
                        Temp = Max;
                }
                else
                {
                    dynamic Min = GetMinValue(This, ValueType);
                    Temp = Value - Delta;
                    if (Temp < Min)
                        Temp = Min;
                }

                AvoidDelayNotifyBlock(This, () => This.Text = Temp.ToString());

                if (!GetEnableDelayNotifyText(This))
                    This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

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

                    Type ValueType = GetValueType(This);
                    dynamic Min = GetMinValue(This, ValueType),
                            Max = GetMaxValue(This, ValueType),
                            Value = This.Text.ToValueType(ValueType);

                    if (Value < Min)
                        AvoidDelayNotifyBlock(This, () => This.Text = Min.ToString());
                    else if (Max < Value)
                        AvoidDelayNotifyBlock(This, () => This.Text = Max.ToString());

                    This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                }

                else if (e.Key is Key.Up)
                {
                    Type ValueType = GetValueType(This);
                    dynamic Max = GetMaxValue(This, ValueType),
                            Value = This.Text.ToValueType(ValueType),
                            Delta = Convert.ChangeType(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? GetCombineDelta(This) :
                                                                                                                               GetDelta(This), ValueType);

                    dynamic Temp = Value + Delta;
                    if (Max < Temp)
                        Temp = Max;

                    AvoidDelayNotifyBlock(This, () => This.Text = Temp.ToString());

                    if (!GetEnableDelayNotifyText(This))
                        This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

                    e.Handled = true;
                }

                else if (e.Key is Key.Down)
                {
                    Type ValueType = GetValueType(This);
                    dynamic Min = GetMinValue(This, ValueType),
                            Value = This.Text.ToValueType(ValueType),
                            Delta = Convert.ChangeType(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? GetCombineDelta(This) :
                                                                                                                               GetDelta(This), ValueType);

                    dynamic Temp = Value - Delta;
                    if (Temp < Min)
                        Temp = Min;

                    AvoidDelayNotifyBlock(This, () => This.Text = Temp.ToString());

                    if (!GetEnableDelayNotifyText(This))
                        This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

                    e.Handled = true;
                }

                else if (e.Key is Key.PageUp)
                {
                    Type ValueType = GetValueType(This);
                    dynamic Max = GetMaxValue(This, ValueType),
                            Value = This.Text.ToValueType(ValueType),
                            Delta = Convert.ChangeType(GetCombineDelta(This), ValueType);

                    dynamic Temp = Value + Delta;
                    if (Max < Temp)
                        Temp = Max;

                    AvoidDelayNotifyBlock(This, () => This.Text = Temp.ToString());

                    if (!GetEnableDelayNotifyText(This))
                        This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

                    e.Handled = true;
                }

                else if (e.Key is Key.PageDown)
                {
                    Type ValueType = GetValueType(This);
                    dynamic Min = GetMinValue(This, ValueType),
                            Value = This.Text.ToValueType(ValueType),
                            Delta = Convert.ChangeType(GetCombineDelta(This), ValueType);

                    dynamic Temp = Value - Delta;
                    if (Temp < Min)
                        Temp = Min;

                    AvoidDelayNotifyBlock(This, () => This.Text = Temp.ToString());

                    if (!GetEnableDelayNotifyText(This))
                        This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

                    e.Handled = true;
                }
            }
        }

        private static dynamic GetMaxValue(TextBox obj, Type ValueType)
        {
            object Max = GetMaximum(obj);
            if (Max != null)
                return Convert.ChangeType(Max, ValueType);

            if (MaxValues.TryGetValue(ValueType, out Max))
                return Max;

            throw new NotSupportedException();
        }
        private static dynamic GetMinValue(TextBox obj, Type ValueType)
        {
            object Min = GetMinimum(obj);
            if (Min != null)
                return Convert.ChangeType(Min, ValueType);

            if (MinValues.TryGetValue(ValueType, out Min))
                return Min;

            throw new NotSupportedException();
        }
        private static object ToValueType(this string This, Type ValueType)
        {
            try
            {
                if (!string.IsNullOrEmpty(This))
                    return Convert.ChangeType(This, ValueType);
            }
            catch
            {

            }
            return Activator.CreateInstance(ValueType);
        }

        #endregion

        #region InputMode
        private static readonly DependencyProperty DefaultInputModeProperty =
            DependencyProperty.RegisterAttached("DefaultInputMode", typeof(KeyboardInputMode), typeof(TextBoxEx), new PropertyMetadata(KeyboardInputMode.All,
                (d, e) =>
                {
                    if (d is TextBox This &&
                        This.GetValue(InputModeProperty) is null)
                        OnInputModeChanged(This, (KeyboardInputMode)e.OldValue, (KeyboardInputMode)e.NewValue);
                }));
        private static KeyboardInputMode GetDefaultInputMode(DependencyObject obj)
            => (KeyboardInputMode)obj.GetValue(DefaultInputModeProperty);
        private static void SetDefaultInputMode(DependencyObject obj, KeyboardInputMode value)
            => obj.SetValue(DefaultInputModeProperty, value);

        public static readonly DependencyProperty InputModeProperty =
            DependencyProperty.RegisterAttached("InputMode", typeof(KeyboardInputMode?), typeof(TextBoxEx), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is TextBox This)
                        OnInputModeChanged(This, (KeyboardInputMode)(e.OldValue ?? This.GetValue(DefaultInputModeProperty)),
                                                 (KeyboardInputMode)(e.NewValue ?? This.GetValue(DefaultInputModeProperty)));
                }));
        public static KeyboardInputMode GetInputMode(TextBox obj)
            => obj.GetValue(InputModeProperty) is KeyboardInputMode Mode ? Mode : GetDefaultInputMode(obj);
        public static void SetInputMode(TextBox obj, KeyboardInputMode value)
            => obj.SetValue(InputModeProperty, value);

        private static void OnInputModeChanged(TextBox This, KeyboardInputMode OldValue, KeyboardInputMode NewValue)
        {
            bool IsModeAll = NewValue is KeyboardInputMode.All;
            InputMethod.SetIsInputMethodEnabled(This, IsModeAll);
            if (!IsModeAll)
            {
                if (OldValue is KeyboardInputMode.All)
                    This.KeyDown += OnInputMode_KeyDown;
            }
            else
            {
                This.KeyDown -= OnInputMode_KeyDown;
            }
        }
        private static void OnInputMode_KeyDown(object sender, KeyEventArgs e)
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
                    (e.Key is Key.Decimal || e.Key is Key.OemPeriod) &&
                    !This.Text.Contains("."))
                    return;

                // -
                if ((Mode & KeyboardInputMode.Negative) > 0 &&
                    (e.Key is Key.Subtract || e.Key is Key.OemMinus) &&
                    !This.Text.Contains("-"))
                    return;

                e.Handled = true;
            }
        }

        #endregion

        #region EnableDelayNotifyText
        public static double DelayNotifyInterval { get; set; } = 500d;

        public static readonly DependencyProperty EnableDelayNotifyTextProperty =
            DependencyProperty.RegisterAttached("EnableDelayNotifyText", typeof(bool), typeof(TextBoxEx), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is TextBox This)
                    {
                        if (e.NewValue is true)
                        {
                            This.TextChanged += OnDelayNotify_TextChanged;
                        }
                        else
                        {
                            This.TextChanged -= OnDelayNotify_TextChanged;

                            if (GetDelayToken(This) is DelayActionToken Token)
                                Token.Cancel();
                        }
                    }
                }));
        public static bool GetEnableDelayNotifyText(TextBox obj)
            => (bool)obj.GetValue(EnableDelayNotifyTextProperty);
        public static void SetEnableDelayNotifyText(TextBox obj, bool value)
            => obj.SetValue(EnableDelayNotifyTextProperty, value);

        private static readonly DependencyProperty DelayTokenProperty =
            DependencyProperty.RegisterAttached("DelayToken", typeof(DelayActionToken), typeof(TextBoxEx), new PropertyMetadata(null));
        private static DelayActionToken GetDelayToken(DependencyObject obj)
            => (DelayActionToken)obj.GetValue(DelayTokenProperty);
        private static void SetDelayToken(DependencyObject obj, DelayActionToken value)
            => obj.SetValue(DelayTokenProperty, value);

        private static void OnDelayNotify_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox This)
            {
                if (GetDelayToken(This) is DelayActionToken OldToken)
                    OldToken.Cancel();

                void DelayTask()
                {
                    SetDelayToken(This, null);

                    if (!string.IsNullOrEmpty(This.Text) &&
                        GetValueType(This) is Type ValueType)
                    {
                        dynamic Min = GetMinValue(This, ValueType),
                                Max = GetMaxValue(This, ValueType),
                                Value = This.Text.ToValueType(ValueType);
                        if (Value < Min)
                            AvoidDelayNotifyBlock(This, () => This.Text = Min.ToString());
                        else if (Max < Value)
                            AvoidDelayNotifyBlock(This, () => This.Text = Max.ToString());

                        This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                    }
                }

                DelayActionToken Token = DispatcherHelper.DelayAction(DelayNotifyInterval, DelayTask, () => SetDelayToken(This, null));
                SetDelayToken(This, Token);
            }
        }

        internal static void AvoidDelayNotifyBlock(TextBox ThisBox, Action Action)
        {
            bool Enable = GetEnableDelayNotifyText(ThisBox);

            try
            {
                SetEnableDelayNotifyText(ThisBox, false);
                Action();
            }
            finally
            {
                SetEnableDelayNotifyText(ThisBox, Enable);
            }
        }

        #endregion

        #region EnableAutoSelectAllText
        public static readonly DependencyProperty EnableAutoSelectAllTextProperty =
            DependencyProperty.RegisterAttached("EnableAutoSelectAllText", typeof(bool), typeof(TextBoxEx), new PropertyMetadata(false, (d, e) =>
            {
                if (d is TextBox This)
                {
                    if (e.NewValue is true)
                    {
                        This.GotKeyboardFocus += OnAutoSelectAll_GotKeyboardFocus;
                        This.PreviewMouseLeftButtonDown += OnAutoSelectAll_MousePreviewLeftButtonDown;
                    }
                    else
                    {
                        This.GotKeyboardFocus -= OnAutoSelectAll_GotKeyboardFocus;
                        This.PreviewMouseLeftButtonDown -= OnAutoSelectAll_MousePreviewLeftButtonDown;
                    }
                }
            }));
        public static bool GetEnableAutoSelectAllText(TextBox obj)
            => (bool)obj.GetValue(EnableAutoSelectAllTextProperty);
        public static void SetEnableAutoSelectAllText(TextBox obj, bool value)
            => obj.SetValue(EnableAutoSelectAllTextProperty, value);

        private static void OnAutoSelectAll_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox This)
                This.SelectAll();
        }
        private static void OnAutoSelectAll_MousePreviewLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox This &&
                !This.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                This.Focus();
            }
        }

        #endregion

        public static void UpdateTextBindingWhenEnterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter &&
                sender is TextBox This)
                This.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }

    }
}