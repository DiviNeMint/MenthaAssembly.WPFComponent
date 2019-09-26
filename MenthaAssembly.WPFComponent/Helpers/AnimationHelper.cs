using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MenthaAssembly
{

    public class AnimationHelper
    {
        public static readonly RoutedEvent UpdateEvent =
            EventManager.RegisterRoutedEvent("Update", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(AnimationHelper));

        public static void AddUpdateHandler(DependencyObject d, RoutedPropertyChangedEventHandler<double> handler)
        {
            if (d is UIElement This)
                This.AddHandler(UpdateEvent, handler);
        }
        public static void RemoveUpdateHandler(DependencyObject d, RoutedPropertyChangedEventHandler<double> handler)
        {
            if (d is UIElement This)
                This.RemoveHandler(UpdateEvent, handler);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool?), typeof(AnimationHelper), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is UIElement This &&
                        GetBegin(This) is double Begin &&
                        GetEnd(This) is double End)
                    {
                        int Interval = GetInterval(d);
                        bool IsEnabled = e.NewValue is true;
                        if (Interval > 0)
                        {
                            DoubleAnimation DA = new DoubleAnimation
                            {
                                From = IsEnabled ? Begin : End,
                                To = IsEnabled ? End : Begin,
                                Duration = new Duration(TimeSpan.FromMilliseconds(Interval))
                            };
                            DA.Freeze();
                            This.ApplyAnimationClock(PercentageProperty, DA.CreateClock());
                        }
                        else
                        {
                            OnAnimationUpdated(This, null, IsEnabled ? End : Begin);
                        }
                    }
                }));
        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);


        public static readonly DependencyProperty TargeProperty =
            DependencyProperty.RegisterAttached("Targe", typeof(string), typeof(AnimationHelper), new PropertyMetadata(string.Empty,
                (d, e) => OnAnimationUpdated(d, null, GetPercentage(d))));
        public static string GetTarge(DependencyObject obj)
            => (string)obj.GetValue(TargeProperty);
        public static void SetTarge(DependencyObject obj, string value)
            => obj.SetValue(TargeProperty, value);


        public static readonly DependencyProperty BeginProperty =
            DependencyProperty.RegisterAttached("Begin", typeof(double), typeof(AnimationHelper), new PropertyMetadata(0d));
        public static double GetBegin(DependencyObject obj)
            => (double)obj.GetValue(BeginProperty);
        public static void SetBegin(DependencyObject obj, double value)
            => obj.SetValue(BeginProperty, value);


        public static readonly DependencyProperty EndProperty =
            DependencyProperty.RegisterAttached("End", typeof(double), typeof(AnimationHelper), new PropertyMetadata(1d));
        public static double GetEnd(DependencyObject obj)
            => (double)obj.GetValue(EndProperty);
        public static void SetEnd(DependencyObject obj, double value)
            => obj.SetValue(EndProperty, value);


        public static readonly DependencyProperty FromProperty =
            DependencyProperty.RegisterAttached("From", typeof(object), typeof(AnimationHelper), new PropertyMetadata(null,
                (d, e) => OnAnimationUpdated(d, null, GetPercentage(d))));
        public static object GetFrom(DependencyObject obj)
            => obj.GetValue(FromProperty);
        public static void SetFrom(DependencyObject obj, object value)
            => obj.SetValue(FromProperty, value);


        public static readonly DependencyProperty ToProperty =
            DependencyProperty.RegisterAttached("To", typeof(object), typeof(AnimationHelper), new PropertyMetadata(null,
                (d, e) => OnAnimationUpdated(d, null, GetPercentage(d))));
        public static object GetTo(DependencyObject obj)
            => obj.GetValue(ToProperty);
        public static void SetTo(DependencyObject obj, object value)
            => obj.SetValue(ToProperty, value);


        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.RegisterAttached("Parameter", typeof(object), typeof(AnimationHelper), new PropertyMetadata(null,
                (d, e) => OnAnimationUpdated(d, null, GetPercentage(d))));
        public static object GetParameter(DependencyObject obj)
            => obj.GetValue(ParameterProperty);
        public static void SetParameter(DependencyObject obj, object value)
            => obj.SetValue(ParameterProperty, value);


        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.RegisterAttached("Interval", typeof(int), typeof(AnimationHelper), new PropertyMetadata(300));
        public static int GetInterval(DependencyObject obj)
            => (int)obj.GetValue(IntervalProperty);
        public static void SetInterval(DependencyObject obj, int value)
            => obj.SetValue(IntervalProperty, value);


        protected static readonly DependencyProperty PercentageProperty =
            DependencyProperty.RegisterAttached("Percentage", typeof(double), typeof(AnimationHelper), new PropertyMetadata(0d,
                (d, e) => OnAnimationUpdated(d, (double)e.OldValue, (double)e.NewValue)));

        protected static double GetPercentage(DependencyObject obj)
            => (double)obj.GetValue(PercentageProperty);
        protected static void SetPercentage(DependencyObject obj, double value)
            => obj.SetValue(PercentageProperty, value);


        public static Dictionary<string, Action<DependencyObject, double?, double>> AnimationFunction { get; } = new Dictionary<string, Action<DependencyObject, double?, double>>
        {
            { "Width", (d, o, n) =>
                {
                    if (GetTo(d) is double To && d.GetType().GetProperty("Width") is PropertyInfo WidthProperty)
                    {
                        object From = GetFrom(d);
                        WidthProperty.SetValue(d, n <= 0d ? (From is null ? double.NaN : Convert.ToDouble(From)) : To * n);
                    }
                }},
            { "Height", (d, o, n) =>
                {
                    if (GetTo(d) is double To && d.GetType().GetProperty("Height") is PropertyInfo HeightProperty)
                    {
                        object From = GetFrom(d);
                        HeightProperty.SetValue(d, n <= 0d ? (From is null ? double.NaN : Convert.ToDouble(From)) : To * n);
                    }
                }},
            { "Margin.Left", (d, o, n) =>
                {
                    if (GetTo(d) is double To && d.GetType().GetProperty("Margin") is PropertyInfo MarginProperty)
                    {
                        Thickness Margin = (Thickness)MarginProperty.GetValue(d);
                        Margin.Left = Convert.ToDouble(To) * (1d - n);
                        MarginProperty.SetValue(d, Margin);
                    }
                }},
            { "Margin.Right", (d, o, n) =>
                {
                    if (GetTo(d) is double To && d.GetType().GetProperty("Margin") is PropertyInfo MarginProperty)
                    {
                        Thickness Margin = (Thickness)MarginProperty.GetValue(d);
                        Margin.Right = Convert.ToDouble(To) * n;
                        MarginProperty.SetValue(d, Margin);
                    }
                }},
            { "Margin.Top", (d, o, n) =>
                {
                    if (GetTo(d) is double To && d.GetType().GetProperty("Margin") is PropertyInfo MarginProperty)
                    {
                        Thickness Margin = (Thickness)MarginProperty.GetValue(d);
                        Margin.Top = Convert.ToDouble(To) * (1d - n);
                        MarginProperty.SetValue(d, Margin);
                    }
                }},
            { "Margin.Bottom", (d, o, n) =>
                {
                    if (GetTo(d) is double To && d.GetType().GetProperty("Margin") is PropertyInfo MarginProperty)
                    {
                        Thickness Margin = (Thickness)MarginProperty.GetValue(d);
                        Margin.Bottom = Convert.ToDouble(To) * n;
                        MarginProperty.SetValue(d, Margin);
                    }
                }},
            { "Background", (d, o, n) =>
                {
                    object To = GetTo(d);
                    if (To != null && d.GetType().GetProperty("Background") is PropertyInfo BackgroundPorperty)
                    {
                        object From = GetFrom(d);
                        if (To is SolidColorBrush ToBrush)
                        {

                            Color FromColor = From is null ? Colors.White : (From as SolidColorBrush).Color;
                            BackgroundPorperty.SetValue(d, new SolidColorBrush(Color.FromArgb(
                                (byte)(FromColor.A + (ToBrush.Color.A - FromColor.A) * n),
                                (byte)(FromColor.R + (ToBrush.Color.R - FromColor.R) * n),
                                (byte)(FromColor.G + (ToBrush.Color.G - FromColor.G) * n),
                                (byte)(FromColor.B + (ToBrush.Color.B - FromColor.B) * n))));
                        }
                        //if (To is LinearGradientBrush ToLinearBrush)
                        //{

                        //}
                        //if (To is RadialGradientBrush)
                        //{

                        //}
                    }
                }},
        };

        protected static void OnAnimationUpdated(DependencyObject d, double? OldValue, double NewValue)
        {
            if (AnimationFunction.TryGetValue(GetTarge(d).ToString(), out Action<DependencyObject, double?, double> Function))
            {
                Function.Invoke(d, OldValue, NewValue);
            }
            else
            {
                if (d is UIElement UI)
                    UI.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(OldValue ?? 0d, NewValue, UpdateEvent));
            }
        }

    }
}
