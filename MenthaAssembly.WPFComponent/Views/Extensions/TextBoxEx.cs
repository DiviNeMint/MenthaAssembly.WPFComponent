#if !NET
using System.Linq;
#endif
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
        private delegate object TypeConvertFunc(string Value, out bool IsOverflow);

        private static readonly ConcurrentDictionary<Type, TypeConvertFunc> ValueConverters = new();
        private static readonly ConcurrentDictionary<Type, object> MaxValues = new(),
                                                                   MinValues = new();

        public static readonly DependencyProperty ValueTypeProperty =
            DependencyProperty.RegisterAttached("ValueType", typeof(Type), typeof(TextBoxEx), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is TextBox This)
                        OnValueTypeChanged(This, e.OldValue as Type, e.NewValue as Type);
                }));
        public static Type GetValueType(TextBox obj)
            => (Type)obj.GetValue(ValueTypeProperty);
        public static void SetValueType(TextBox obj, Type value)
            => obj.SetValue(ValueTypeProperty, value);

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.RegisterAttached("Minimum", typeof(object), typeof(TextBoxEx), new PropertyMetadata(null));
        public static object GetMinimum(TextBox obj)
            => obj.GetValue(MinimumProperty);
        public static void SetMinimum(TextBox obj, object value)
            => obj.SetValue(MinimumProperty, value);

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.RegisterAttached("Maximum", typeof(object), typeof(TextBoxEx), new PropertyMetadata(null));
        public static object GetMaximum(TextBox obj)
            => obj.GetValue(MaximumProperty);
        public static void SetMaximum(TextBox obj, object value)
            => obj.SetValue(MaximumProperty, value);

        public static readonly DependencyProperty DeltaProperty =
            DependencyProperty.RegisterAttached("Delta", typeof(object), typeof(TextBoxEx), new PropertyMetadata(1));
        public static object GetDelta(TextBox obj)
            => obj.GetValue(DeltaProperty);
        public static void SetDelta(TextBox obj, object value)
            => obj.SetValue(DeltaProperty, value);

        public static readonly DependencyProperty CombineDeltaProperty =
            DependencyProperty.RegisterAttached("CombineDelta", typeof(object), typeof(TextBoxEx), new PropertyMetadata(10));
        public static object GetCombineDelta(TextBox obj)
            => obj.GetValue(CombineDeltaProperty);
        public static void SetCombineDelta(TextBox obj, object value)
            => obj.SetValue(CombineDeltaProperty, value);

        private static void OnValueTypeChanged(TextBox This, Type OldType, Type NewType)
        {
            if (OldType != null)
            {
                SetInputMode(This, KeyboardInputMode.All);
                This.PreviewMouseWheel -= OnPreviewMouseWheel;
                This.PreviewKeyDown -= OnPreviewKeyDown;
            }

            if (NewType != null)
            {
                bool EnableAttach = false;
                KeyboardInputMode InputMode = KeyboardInputMode.All;

                if (NewType.IsSignedIntegerType())
                {
                    InputMode = KeyboardInputMode.NegativeNumber;
                    EnableAttach = true;
                }

                else if (NewType.IsUnsignedIntegerType())
                {
                    InputMode = KeyboardInputMode.Number;
                    EnableAttach = true;
                }

                else if (NewType.IsDecimalType())
                {
                    InputMode = KeyboardInputMode.NegativeNumberAndDot;
                    EnableAttach = true;
                }

                if (EnableAttach)
                {
                    // Max
                    if (!MaxValues.TryGetValue(NewType, out object Max) &&
                        NewType.TryGetConstant("MaxValue", out Max))
                        MaxValues.AddOrUpdate(NewType, Max, (k, v) => Max);

                    // Min
                    if (!MinValues.TryGetValue(NewType, out object Min) &&
                        NewType.TryGetConstant("MinValue", out Min))
                        MinValues.AddOrUpdate(NewType, Min, (k, v) => Min);

                    // Converter
                    if (!ValueConverters.TryGetValue(NewType, out TypeConvertFunc Converter))
                    {
                        Converter = CreateTypeConverter(NewType, Max, Min, InputMode);
                        ValueConverters.AddOrUpdate(NewType, Converter, (k, v) => Converter);
                    }

                    // Attach Events
                    SetDefaultInputMode(This, InputMode);
                    This.PreviewMouseWheel += OnPreviewMouseWheel;
                    This.PreviewKeyDown += OnPreviewKeyDown;

                    // Check current value
                    object Value = Converter(This.Text, out bool IsOverflow);
                    bool IsChanged = IsOverflow;
                    if (!Value.Equals(Min) && OperatorHelper.LessThan(Value, Min))
                    {
                        IsChanged = true;
                        Value = Min;
                    }
                    else if (!Value.Equals(Max) && OperatorHelper.LessThan(Max, Value))
                    {
                        IsChanged = true;
                        Value = Max;
                    }

                    if (IsChanged)
                        This.Text = Value.ToString();
                }
            }
        }
        private static TypeConvertFunc CreateTypeConverter(Type ValueType, object Max, object Min, KeyboardInputMode InputMode)
        {
            bool IsSigned = (InputMode | KeyboardInputMode.Negative) > 0;
            string[] MaxStrings = Max.ToString().Split('.'),
                     MinStrings = Min.ToString().Split('.');

            string MaxInteger = MaxStrings[0],
                   MinInteger = MinStrings[0];
            int MaxIntegerLength = MaxInteger.Length,
                MinIntegerLength = MinInteger.Length;

            object ParseInteger(string Content, bool IsSigned, object Limit, string LimitString, int LimitStringLength, out bool IsOverflow, out bool CheckDecimal)
            {
                int Length = Content.Length;
                if (LimitStringLength < Length)
                {
                    IsOverflow = true;
                    CheckDecimal = false;
                    return Limit;
                }
                else if (LimitStringLength == Length)
                {
                    int Index = IsSigned ? 1 : 0;
                    for (; Index < LimitStringLength; Index++)
                    {
                        char Char = LimitString[Index],
                             ContentChar = Content[Index];
                        if (ContentChar < Char)
                            break;

                        if (Char < ContentChar)
                        {
                            IsOverflow = true;
                            CheckDecimal = false;
                            return Limit;
                        }
                    }

                    if (LimitStringLength <= Index)
                    {
                        IsOverflow = true;
                        CheckDecimal = true;
                        return Limit;
                    }
                }

                IsOverflow = false;
                CheckDecimal = false;
                return Convert.ChangeType(Content, ValueType);
            }

            return (string Content, out bool IsOverflow) =>
            {
                if (string.IsNullOrEmpty(Content))
                {
                    IsOverflow = true;
                    return Activator.CreateInstance(ValueType);
                }

                string[] ContentDatas = Content.Split('.');
                if (Content[0] == '-')
                {
                    if (!IsSigned)
                    {
                        IsOverflow = true;
                        return Min;
                    }

                    return ParseInteger(Content, true, Min, MinInteger, MinIntegerLength, out IsOverflow, out _);
                }

                return ParseInteger(Content, false, Max, MaxInteger, MaxIntegerLength, out IsOverflow, out _);
            };
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is TextBox This &&
                This.IsKeyboardFocusWithin)
            {
                Type ValueType = GetValueType(This);
                TypeConvertFunc Converter = GetConverter(ValueType);
                object Value = Converter(This.Text, out bool IsOverflow),
                       Delta = Convert.ChangeType(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? GetCombineDelta(This) :
                                                                                                                          GetDelta(This), ValueType);

                bool IsChanged = IsOverflow;
                if (e.Delta > 0)
                {
                    object Max = GetMaxValue(This, ValueType);
                    if (!Value.Equals(Max))
                    {
                        IsChanged = true;
                        Value = OperatorHelper.LessThan(OperatorHelper.Subtract(Max, Delta), Value) ? Max : OperatorHelper.Add(Value, Delta);
                    }
                }
                else
                {
                    object Min = GetMinValue(This, ValueType);
                    if (!Value.Equals(Min))
                    {
                        IsChanged = true;
                        Value = OperatorHelper.LessThan(Value, OperatorHelper.Add(Min, Delta)) ? Min : OperatorHelper.Subtract(Value, Delta);
                    }
                }

                if (IsChanged)
                    This.Text = Value.ToString();

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
                    Type ValueType = GetValueType(This);
                    TypeConvertFunc Converter = GetConverter(ValueType);
                    object Min = GetMinValue(This, ValueType),
                           Max = GetMaxValue(This, ValueType),
                           Value = Converter(This.Text, out bool IsOverflow);

                    bool IsChanged = IsOverflow;
                    if (!Value.Equals(Min) && OperatorHelper.LessThan(Value, Min))
                    {
                        IsChanged = true;
                        Value = Min;
                    }
                    else if (!Value.Equals(Max) && OperatorHelper.LessThan(Max, Value))
                    {
                        IsChanged = true;
                        Value = Max;
                    }

                    if (IsChanged)
                        This.Text = Value.ToString();
                }

                else if (e.Key is Key.Up)
                {
                    Type ValueType = GetValueType(This);
                    TypeConvertFunc Converter = GetConverter(ValueType);
                    object Max = GetMaxValue(This, ValueType),
                           Value = Converter(This.Text, out bool IsOverflow),
                           Delta = Convert.ChangeType(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? GetCombineDelta(This) :
                                                                                                                              GetDelta(This), ValueType);

                    bool IsChanged = IsOverflow;
                    if (!Value.Equals(Max))
                    {
                        IsChanged = true;
                        Value = OperatorHelper.LessThan(OperatorHelper.Subtract(Max, Delta), Value) ? Max : OperatorHelper.Add(Value, Delta);
                    }

                    if (IsChanged)
                        This.Text = Value.ToString();

                    e.Handled = true;
                }

                else if (e.Key is Key.Down)
                {
                    Type ValueType = GetValueType(This);
                    TypeConvertFunc Converter = GetConverter(ValueType);
                    object Min = GetMinValue(This, ValueType),
                           Value = Converter(This.Text, out bool IsOverflow),
                           Delta = Convert.ChangeType(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ? GetCombineDelta(This) :
                                                                                                                              GetDelta(This), ValueType);

                    bool IsChanged = IsOverflow;
                    if (!Value.Equals(Min))
                    {
                        IsChanged = true;
                        Value = OperatorHelper.LessThan(Value, OperatorHelper.Add(Min, Delta)) ? Min : OperatorHelper.Subtract(Value, Delta);
                    }

                    if (IsChanged)
                        This.Text = Value.ToString();

                    e.Handled = true;
                }

                else if (e.Key is Key.PageUp)
                {
                    Type ValueType = GetValueType(This);
                    TypeConvertFunc Converter = GetConverter(ValueType);
                    object Max = GetMaxValue(This, ValueType),
                           Value = Converter(This.Text, out bool IsOverflow),
                           Delta = Convert.ChangeType(GetCombineDelta(This), ValueType);

                    bool IsChanged = IsOverflow;
                    if (!Value.Equals(Max))
                    {
                        IsChanged = true;
                        Value = OperatorHelper.LessThan(OperatorHelper.Subtract(Max, Delta), Value) ? Max : OperatorHelper.Add(Value, Delta);
                    }

                    if (IsChanged)
                        This.Text = Value.ToString();

                    e.Handled = true;
                }

                else if (e.Key is Key.PageDown)
                {
                    Type ValueType = GetValueType(This);
                    TypeConvertFunc Converter = GetConverter(ValueType);
                    object Min = GetMinValue(This, ValueType),
                           Value = Converter(This.Text, out bool IsOverflow),
                           Delta = Convert.ChangeType(GetCombineDelta(This), ValueType);

                    bool IsChanged = IsOverflow;
                    if (!Value.Equals(Min))
                    {
                        IsChanged = true;
                        Value = OperatorHelper.LessThan(Value, OperatorHelper.Add(Min, Delta)) ? Min : OperatorHelper.Subtract(Value, Delta);
                    }

                    if (IsChanged)
                        This.Text = Value.ToString();

                    e.Handled = true;
                }
            }
        }

        private static object GetMaxValue(TextBox obj, Type ValueType)
        {
            object Max = GetMaximum(obj);
            if (Max != null)
                return Convert.ChangeType(Max, ValueType);

            if (MaxValues.TryGetValue(ValueType, out Max))
                return Max;

            throw new NotSupportedException();
        }
        private static object GetMinValue(TextBox obj, Type ValueType)
        {
            object Min = GetMinimum(obj);
            if (Min != null)
                return Convert.ChangeType(Min, ValueType);

            if (MinValues.TryGetValue(ValueType, out Min))
                return Min;

            throw new NotSupportedException();
        }
        private static TypeConvertFunc GetConverter(Type ValueType)
            => ValueConverters.TryGetValue(ValueType, out TypeConvertFunc Converter) ? Converter : throw new NotSupportedException();

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
            if (e.Key is Key.Tab || e.Key is Key.Enter)
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
                    (e.Key is Key.Decimal || e.Key is Key.OemPeriod))
                {
                    if (!(GetValueType(This)?.IsDecimalType() ?? false) || !This.Text.Contains('.'))
                        return;
                }

                // -
                if ((Mode & KeyboardInputMode.Negative) > 0 &&
                    (e.Key is Key.Subtract || e.Key is Key.OemMinus) &&
                    !This.Text.Contains('-'))
                    return;

                e.Handled = true;
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