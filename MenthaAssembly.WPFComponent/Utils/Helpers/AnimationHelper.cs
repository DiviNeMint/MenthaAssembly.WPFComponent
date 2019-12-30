using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
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
                    if (GetBegin(d) is double Begin &&
                        GetEnd(d) is double End &&
                        e.NewValue is bool IsEnabled)
                    {
                        if (d is UIElement UIElement)
                        {
                            DoubleAnimation DA = new DoubleAnimation
                            {
                                From = IsEnabled ? Begin : End,
                                To = IsEnabled ? End : Begin,
                                Duration = new Duration(TimeSpan.FromMilliseconds(GetInterval(d)))
                            };
                            DA.Freeze();
                            UIElement.ApplyAnimationClock(PercentageProperty, DA.CreateClock());
                            return;
                        }

                        if (d is ContentElement ContentElement)
                        {
                            DoubleAnimation DA = new DoubleAnimation
                            {
                                From = IsEnabled ? Begin : End,
                                To = IsEnabled ? End : Begin,
                                Duration = new Duration(TimeSpan.FromMilliseconds(GetInterval(d)))
                            };
                            DA.Freeze();
                            ContentElement.ApplyAnimationClock(PercentageProperty, DA.CreateClock());
                        }
                    }
                }));
        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);


        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.RegisterAttached("Target", typeof(string), typeof(AnimationHelper), new PropertyMetadata(string.Empty,
                (d, e) => OnAnimationUpdated(d, null, GetPercentage(d))));
        public static string GetTarget(DependencyObject obj)
            => (string)obj.GetValue(TargetProperty);
        public static void SetTarget(DependencyObject obj, string value)
            => obj.SetValue(TargetProperty, value);


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


        public static IMultiValueConverter ParametersConverter { get; } = new _ParametersConverter();

        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.RegisterAttached("Parameters", typeof(object), typeof(AnimationHelper), new PropertyMetadata(null,
                (d, e) => OnAnimationUpdated(d, null, GetPercentage(d))));
        public static object GetParameters(DependencyObject obj)
            => obj.GetValue(ParametersProperty);
        public static void SetParameters(DependencyObject obj, object value)
            => obj.SetValue(ParametersProperty, value);

        public static readonly DependencyProperty IntervalProperty =
            DependencyProperty.RegisterAttached("Interval", typeof(int), typeof(AnimationHelper), new PropertyMetadata(0));
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

        protected static void OnAnimationUpdated(DependencyObject d, double? OldValue, double NewValue)
        {
            string Target = GetTarget(d);
            switch (Target)
            {
                case "X":
                case "Y":
                    {
                        if (double.TryParse(GetFrom(d)?.ToString() ?? "0", out double From) &&
                            double.TryParse(GetTo(d)?.ToString() ?? "0", out double To) &&
                            d.GetType().GetProperty("RenderTransform") != null &&
                            DependencyPropertyDescriptor.FromName(Target,
                                                                  typeof(TranslateTransform),
                                                                  typeof(TranslateTransform))?.DependencyProperty is DependencyProperty Dp)
                        {
                            TransformGroup TransformGroup = d.GetValue(UIElement.RenderTransformProperty) as TransformGroup;
                            if (TransformGroup is null)
                            {
                                TransformGroup = new TransformGroup();
                                d.SetValue(UIElement.RenderTransformProperty, TransformGroup);
                            }
                            TranslateTransform Transform = TransformGroup.Children.OfType<TranslateTransform>().FirstOrDefault();
                            if (Transform is null)
                            {
                                Transform = new TranslateTransform();
                                TransformGroup.Children.Add(Transform);
                            }

                            Transform.SetValue(Dp, GetBegin(d) >= 0 ? From - (From - To) * NewValue : (From - To) * NewValue);
                        }
                        break;
                    }
                case "Width":
                case "Height":
                    {
                        if (double.TryParse(GetTo(d)?.ToString(), out double To) &&
                            d.GetType().GetProperty(Target) is PropertyInfo Property)
                        {
                            if (Property.PropertyType.Equals(typeof(GridLength)))
                                Property.SetValue(d, NewValue <= 0d ?
                                                  (double.TryParse(GetFrom(d)?.ToString(), out double From) ? new GridLength(From) : new GridLength(1, GridUnitType.Auto)) :
                                                  new GridLength(To * NewValue));
                            else
                                Property.SetValue(d, NewValue <= 0d ?
                                                  (double.TryParse(GetFrom(d)?.ToString(), out double From) ? From : double.NaN) :
                                                  To * NewValue);
                        }
                        break;
                    }
                case "Margin.Left":
                case "Margin.Right":
                case "Margin.Top":
                case "Margin.Bottom":
                    {
                        if (double.TryParse(GetTo(d)?.ToString(), out double To) &&
                            d.GetType().GetProperty("Margin") != null)
                        {
                            Thickness Margin = (Thickness)d.GetValue(FrameworkElement.MarginProperty);
                            switch (Target)
                            {
                                case "Margin.Left":
                                    Margin.Left = To * (1d - NewValue);
                                    break;
                                case "Margin.Right":
                                    Margin.Right = To * NewValue;
                                    break;
                                case "Margin.Top":
                                    Margin.Top = To * (1d - NewValue);
                                    break;
                                case "Margin.Bottom":
                                    Margin.Bottom = To * NewValue;
                                    break;
                            }
                            d.SetValue(FrameworkElement.MarginProperty, Margin);
                        }
                        break;
                    }
                case "Background":
                    if (GetTo(d) is Brush BrushTo &&
                        d.GetType().GetProperty(Target) is PropertyInfo BrushPorperty)
                    {
                        object From = GetFrom(d);
                        if (BrushTo is SolidColorBrush ToBrush)
                        {
                            Color FromColor = From is null ? Colors.White : (From as SolidColorBrush).Color;
                            BrushPorperty.SetValue(d, new SolidColorBrush(Color.FromArgb(
                                (byte)(FromColor.A + (ToBrush.Color.A - FromColor.A) * NewValue),
                                (byte)(FromColor.R + (ToBrush.Color.R - FromColor.R) * NewValue),
                                (byte)(FromColor.G + (ToBrush.Color.G - FromColor.G) * NewValue),
                                (byte)(FromColor.B + (ToBrush.Color.B - FromColor.B) * NewValue))));

                        }
                        //if (To is LinearGradientBrush ToLinearBrush)
                        //{

                        //}
                        //if (To is RadialGradientBrush)
                        //{

                        //}
                    }
                    break;
                default:
                    if (d is UIElement UI)
                        UI.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(OldValue ?? 0d, NewValue, UpdateEvent));
                    break;
            }
        }

        protected class _ParametersConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
                => string.Join(",", values);

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
               => new object[0];
        }

    }

}
