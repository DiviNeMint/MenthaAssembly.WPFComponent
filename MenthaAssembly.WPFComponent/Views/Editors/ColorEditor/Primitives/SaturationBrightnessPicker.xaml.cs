using MenthaAssembly.Views.Primitives.Adorners;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MenthaAssembly.Views.Primitives
{
    internal class SaturationBrightnessPicker : Control
    {
        public static readonly DependencyProperty ShowAdornerProperty =
            DependencyProperty.Register(nameof(ShowAdorner), typeof(bool), typeof(SaturationBrightnessPicker), new PropertyMetadata(true,
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

        public static readonly DependencyProperty HueProperty =
              HuePicker.HueProperty.AddOwner(typeof(SaturationBrightnessPicker), new PropertyMetadata(0d, (d, e) => 
              {

              }));
        public double Hue
        {
            get => (double)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        public static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(SaturationBrightnessPicker), new PropertyMetadata(1d,
                (d, e) =>
                {
                    if (d is SaturationBrightnessPicker This)
                        This.OnSaturationChanged(new ChangedEventArgs<double>(e.OldValue, e.NewValue));
                },
                  CoerceSaturationBrightness));
        public double Saturation
        {
            get => (double)GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        public static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register(nameof(Brightness), typeof(double), typeof(SaturationBrightnessPicker), new PropertyMetadata(1d,
                (d, e) =>
                {
                    if (d is SaturationBrightnessPicker This)
                        This.OnBrightnessChanged(new ChangedEventArgs<double>(e.OldValue, e.NewValue));
                },
                CoerceSaturationBrightness));
        public double Brightness
        {
            get => (double)GetValue(BrightnessProperty);
            set => SetValue(BrightnessProperty, value);
        }

        internal static object CoerceSaturationBrightness(DependencyObject d, object v)
            => v is double Value ? Value < 0 ? 0d : Value > 1 ? 1d : Value : DependencyProperty.UnsetValue;

        static SaturationBrightnessPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SaturationBrightnessPicker), new FrameworkPropertyMetadata(typeof(SaturationBrightnessPicker)));
        }

        protected PickerAdorner Adorner;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_GradientStopHueColor") is GradientStop PART_GradientStopHueColor)
            {
                BindingOperations.SetBinding(PART_GradientStopHueColor, GradientStop.ColorProperty, new Binding(nameof(Hue))
                {
                    Source = this,
                    Converter = ColorConverter.HueToColor,
                    FallbackValue = Colors.Red
                });
            }

            CreateAdorner();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // Update
            if (Adorner != null)
                Adorner.Position = new Point(ActualWidth * Saturation,
                                             ActualHeight * (1 - Brightness));
        }

        protected virtual PickerAdorner CreateAdorner()
        {
            try
            {
                Adorner = new SaturationBrightnessPickerAdorner(this);
                if (!ShowAdorner)
                    Adorner.Visibility = Visibility.Collapsed;

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
            if (e.LeftButton == MouseButtonState.Pressed &&
                !IsLeftMouseDown)
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
                    Adorner.Position = new Point(ActualWidth * e.NewValue, Adorner.Position.Y);
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
                    Adorner.Position = new Point(Adorner.Position.X, ActualHeight * (1 - e.NewValue));
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
                Point FixPoint = new(MathHelper.Clamp(Position.X, 0d, ActualWidth),
                                     MathHelper.Clamp(Position.Y, 0d, ActualHeight));

                Adorner.Position = FixPoint;
                Saturation = FixPoint.X / ActualWidth;
                Brightness = 1 - FixPoint.Y / ActualHeight;
            }
            finally
            {
                IsUpdating = false;
            }
        }

    }
}