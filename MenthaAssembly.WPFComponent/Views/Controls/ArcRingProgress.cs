using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public class ArcRingProgress : Control
    {
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventArgs<double>), typeof(ArcRingProgress));

        public static void AddValueChangedHandler(DependencyObject d, RoutedPropertyChangedEventHandler<double> handler)
        {
            if (d is UIElement This)
                This.AddHandler(ValueChangedEvent, handler);
        }
        public static void RemoveValueChangedHandler(DependencyObject d, RoutedPropertyChangedEventHandler<double> handler)
        {
            if (d is UIElement This)
                This.RemoveHandler(ValueChangedEvent, handler);
        }


        static ArcRingProgress()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ArcRingProgress), new FrameworkPropertyMetadata(typeof(ArcRingProgress)));
        }


        public static readonly DependencyProperty IsAnimationEnabledProperty =
            DependencyProperty.Register("IsAnimationEnabled", typeof(bool), typeof(ArcRingProgress), new PropertyMetadata(true));
        public bool IsAnimationEnabled
        {
            get => (bool)GetValue(IsAnimationEnabledProperty);
            set => SetValue(IsAnimationEnabledProperty, value);
        }

        public static new readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(double), typeof(ArcRingProgress), new PropertyMetadata(1d));
        public new double BorderThickness
        {
            get => (double)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public static readonly DependencyProperty ArcRingThicknessProperty =
            DependencyProperty.Register("ArcRingThickness", typeof(double), typeof(ArcRingProgress), new PropertyMetadata(10d));
        [Category("ArcRing")]
        public double ArcRingThickness
        {
            get => (double)GetValue(ArcRingThicknessProperty);
            set => SetValue(ArcRingThicknessProperty, value);
        }

        public static readonly DependencyProperty ArcRingBackgroundProperty =
            DependencyProperty.Register("ArcRingBackground", typeof(Brush), typeof(ArcRingProgress), new PropertyMetadata(Brushes.LightGray));
        [Category("ArcRing")]
        public Brush ArcRingBackground
        {
            get => (Brush)GetValue(ArcRingBackgroundProperty);
            set => SetValue(ArcRingBackgroundProperty, value);
        }

        public static readonly DependencyProperty ArcRingForegroundProperty =
            DependencyProperty.Register("ArcRingForeground", typeof(Brush), typeof(ArcRingProgress), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x43, 0x43, 0x43))));
        [Category("ArcRing")]
        public Brush ArcRingForeground
        {
            get => (Brush)GetValue(ArcRingForegroundProperty);
            set => SetValue(ArcRingForegroundProperty, value);
        }

        public static readonly DependencyProperty SweepDirectionProperty =
            DependencyProperty.Register("SweepDirection", typeof(SweepDirection), typeof(ArcRingProgress), new PropertyMetadata(SweepDirection.Clockwise,
                (d, e) =>
                {
                    SetDisplayMaxAngle(d, (SweepDirection)e.NewValue == SweepDirection.Clockwise ? (double)d.GetValue(MaxAngleProperty) : -(double)d.GetValue(MaxAngleProperty));
                    RefreshVisual(d);
                }));
        public SweepDirection SweepDirection
        {
            get => (SweepDirection)GetValue(SweepDirectionProperty);
            set => SetValue(SweepDirectionProperty, value);
        }

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(ArcRingProgress), new PropertyMetadata(0d));
        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        public static readonly DependencyProperty MaxAngleProperty =
            DependencyProperty.Register("MaxAngle", typeof(double), typeof(ArcRingProgress), new PropertyMetadata(360d,
                (d, e) =>
                {
                    SetDisplayMaxAngle(d, (SweepDirection)d.GetValue(SweepDirectionProperty) == SweepDirection.Clockwise ? (double)e.NewValue : -(double)e.NewValue);
                    SetDisplayAngle(d, (double)d.GetValue(PercentageProperty) * (double)e.NewValue);
                    RefreshVisual(d);
                }));
        public double MaxAngle
        {
            get => (double)GetValue(MaxAngleProperty);
            set => SetValue(MaxAngleProperty, value);
        }


        public static readonly DependencyProperty DisplayMaxAngleProperty =
            DependencyProperty.RegisterAttached("DisplayMaxAngle", typeof(double), typeof(ArcRingProgress), new PropertyMetadata(360d));
        public static double GetDisplayMaxAngle(DependencyObject obj)
            => (double)obj.GetValue(DisplayMaxAngleProperty);
        protected static void SetDisplayMaxAngle(DependencyObject obj, double value)
            => obj.SetValue(DisplayMaxAngleProperty, value);


        public static readonly DependencyProperty DisplayAngleProperty =
            DependencyProperty.RegisterAttached("DisplayAngle", typeof(double), typeof(ArcRingProgress), new PropertyMetadata(0d));
        public static double GetDisplayAngle(DependencyObject obj)
            => (double)obj.GetValue(DisplayAngleProperty);
        protected static void SetDisplayAngle(DependencyObject obj, double value)
            => obj.SetValue(DisplayAngleProperty, value);


        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(ArcRingProgress), new PropertyMetadata(1d,
                (d, e) => RefreshVisual(d)));
        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(ArcRingProgress), new PropertyMetadata(0d,
                (d, e) => RefreshVisual(d)));
        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(double), typeof(ArcRingProgress), new PropertyMetadata(0d,
                (d, e) => SetDisplayAngle(d, (double)e.NewValue * (double)d.GetValue(MaxAngleProperty))));
        public double Percentage
        {
            get => (double)GetValue(PercentageProperty);
            protected set => SetValue(PercentageProperty, value);
        }

        private static void RefreshVisual(DependencyObject d)
        {
            if (d is ArcRingProgress This)
                OnValueChange(This, new DependencyPropertyChangedEventArgs(ValueProperty, This.Value, This.Value));
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ArcRingProgress), new PropertyMetadata(0d, OnValueChange));
        private static void OnValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ArcRingProgress This)
            {
                double NewValue = (double)e.NewValue;
                double TotalValue = This.MaxValue - This.MinValue;
                double NewPercentage = Math.Max(Math.Min(NewValue - This.MinValue, TotalValue), 0d) / TotalValue;
                if (This.SweepDirection == SweepDirection.Counterclockwise)
                    NewPercentage *= -1;

                if ((decimal)This.Percentage != (decimal)NewPercentage)
                {
                    if (This.IsAnimationEnabled)
                    {
                        DoubleAnimation DA = new DoubleAnimation
                        {
                            From = This.Percentage,
                            To = NewPercentage,
                            Duration = new Duration(TimeSpan.FromMilliseconds(300))
                        };
                        DA.Freeze();
                        This.ApplyAnimationClock(PercentageProperty, DA.CreateClock());
                    }
                    else
                    {
                        This.ApplyAnimationClock(PercentageProperty, null);
                        This.Percentage = NewPercentage;
                    }
                }
                This.RaiseEvent(new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, NewValue, ValueChangedEvent));
            }
        }

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

    }
}
