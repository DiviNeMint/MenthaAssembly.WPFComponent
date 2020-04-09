using MenthaAssembly.Views.Primitives.Adorners;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace MenthaAssembly.Views.Primitives
{
    internal class SaturationBrightnessPicker : Control
    {
        public static readonly DependencyProperty ShowAdornerProperty =
              DependencyProperty.Register("ShowAdorner", typeof(bool), typeof(SaturationBrightnessPicker), new PropertyMetadata(true,
                  (d, e) =>
                  {
                      if (d is SaturationBrightnessPicker This &&
                          This.Adorner != null)
                          This.Adorner.Visibility = e.NewValue is true ? Visibility.Visible : Visibility.Collapsed;
                  }));
        public bool ShowAdorner
        {
            get => (bool)GetValue(ShowAdornerProperty);
            set => SetValue(ShowAdornerProperty, value);
        }

        public static readonly DependencyProperty HuePickerProperty =
              DependencyProperty.Register("HuePicker", typeof(HuePicker), typeof(SaturationBrightnessPicker), new PropertyMetadata(null));
        public HuePicker HuePicker
        {
            get => (HuePicker)GetValue(HuePickerProperty);
            set => SetValue(HuePickerProperty, value);
        }

        public static readonly DependencyProperty SaturationProperty =
              DependencyProperty.Register("Saturation", typeof(double), typeof(SaturationBrightnessPicker), new PropertyMetadata(1d,
                  (d, e) =>
                  {
                      if (d is SaturationBrightnessPicker This)
                          This.OnSaturationChanged(new ChangedEventArgs<double>(e.OldValue, e.NewValue));
                  },
                  CoerceSaturationBrightnessValue));
        public double Saturation
        {
            get => (double)GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        public static readonly DependencyProperty BrightnessProperty =
              DependencyProperty.Register("Brightness", typeof(double), typeof(SaturationBrightnessPicker), new PropertyMetadata(1d,
                  (d, e) =>
                  {
                      if (d is SaturationBrightnessPicker This)
                          This.OnBrightnessChanged(new ChangedEventArgs<double>(e.OldValue, e.NewValue));
                  },
                  CoerceSaturationBrightnessValue));
        public double Brightness
        {
            get => (double)GetValue(BrightnessProperty);
            set => SetValue(BrightnessProperty, value);
        }

        internal static object CoerceSaturationBrightnessValue(DependencyObject d, object v)
        {
            if (v is double Value)
            {
                if (Value < 0)
                    return 0d;

                if (Value > 1)
                    return 1d;

                return Value;
            }
            return DependencyProperty.UnsetValue;
        }

        static SaturationBrightnessPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SaturationBrightnessPicker), new FrameworkPropertyMetadata(typeof(SaturationBrightnessPicker)));
        }

        protected PickerAdorner Adorner;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            CreateAdorner();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // Update
            if (Adorner != null)
                Adorner.Position = new Point(this.ActualWidth * Saturation,
                                             this.ActualHeight * (1 - Brightness));
        }

        protected virtual PickerAdorner CreateAdorner()
        {
            try
            {
                Adorner = new SaturationBrightnessPickerAdorner(this);
                AdornerLayer.GetAdornerLayer(this).Add(Adorner);
            }
            catch
            {
            }
            return Adorner;
        }

        private bool IsLeftMouseDown = false;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsLeftMouseDown = true;
                CaptureMouse();
                UpdateAdorner(e.GetPosition(this));
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsLeftMouseDown)
                UpdateAdorner(e.GetPosition(this));
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (IsLeftMouseDown)
            {
                ReleaseMouseCapture();
                IsLeftMouseDown = false;
            }
        }

        protected void OnSaturationChanged(ChangedEventArgs<double> e)
        {
            if (IsUpdating)
                return;

            try
            {
                IsUpdating = true;
                if (Adorner != null)
                    Adorner.Position = new Point(this.ActualWidth * e.NewValue, Adorner.Position.Y);
            }
            finally
            {
                IsUpdating = false;
            }
        }
        protected void OnBrightnessChanged(ChangedEventArgs<double> e)
        {
            if (IsUpdating)
                return;

            try
            {
                IsUpdating = true;
                if (Adorner != null)
                    Adorner.Position = new Point(Adorner.Position.X, this.ActualHeight * (1 - e.NewValue));
            }
            finally
            {
                IsUpdating = false;
            }
        }

        protected bool IsUpdating = false;
        protected virtual void UpdateAdorner(Point Position)
        {
            try
            {
                IsUpdating = true;
                Point FixPoint = new Point(Math.Max(Math.Min(Position.X, ActualWidth), 0d),
                                           Math.Max(Math.Min(Position.Y, ActualHeight), 0d));

                Adorner.Position = FixPoint;
                Saturation = FixPoint.X / ActualWidth;
                Brightness = 1 - (FixPoint.Y / ActualHeight);
            }
            finally
            {
                IsUpdating = false;
            }
        }

    }
}
