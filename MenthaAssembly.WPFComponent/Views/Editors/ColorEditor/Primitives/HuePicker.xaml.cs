using MenthaAssembly.Views.Primitives.Adorners;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MenthaAssembly.Views.Primitives
{
    internal class HuePicker : Control
    {
        public static readonly DependencyProperty ShowAdornerProperty =
              DependencyProperty.Register(nameof(ShowAdorner), typeof(bool), typeof(HuePicker), new PropertyMetadata(true,
                  (d, e) =>
                  {
                      if (d is HuePicker This &&
                          This.Adorner != null)
                          This.Adorner.Visibility = e.NewValue is true ? Visibility.Visible : Visibility.Collapsed;
                  }));
        public bool ShowAdorner
        {
            get => (bool)GetValue(ShowAdornerProperty);
            set => SetValue(ShowAdornerProperty, value);
        }

        public static readonly DependencyProperty HueProperty =
              DependencyProperty.Register(nameof(Hue), typeof(double), typeof(HuePicker), new PropertyMetadata(0d,
                  (d, e) =>
                  {
                      if (d is HuePicker This)
                          This.OnHueChanged(new ChangedEventArgs<double>(e.OldValue, e.NewValue));
                  },
                  CoerceHueValue));
        internal static object CoerceHueValue(DependencyObject d, object v)
            => v is double Value ? Value < 0 ? 0d : Value > 360 ? 360d : Value : DependencyProperty.UnsetValue;
        public double Hue
        {
            get => (double)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        static HuePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HuePicker), new FrameworkPropertyMetadata(typeof(HuePicker)));
        }

        private Rectangle PART_HuePalette;
        protected PickerAdorner Adorner;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_HuePalette") is Rectangle HuePalette)
                PART_HuePalette = HuePalette;

            CreateAdorner();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateAdornerPosition();
        }

        protected virtual PickerAdorner CreateAdorner()
        {
            try
            {
                Adorner = new HuePickerAdorner(this);
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
                Hue = Math.Max(Math.Min(e.GetPosition(PART_HuePalette).Y, PART_HuePalette.ActualHeight), 0d) * 360d / PART_HuePalette.ActualHeight;
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsLeftMouseDown)
                Hue = Math.Max(Math.Min(e.GetPosition(PART_HuePalette).Y, PART_HuePalette.ActualHeight), 0d) * 360d / PART_HuePalette.ActualHeight;
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

        protected virtual void OnHueChanged(ChangedEventArgs<double> e)
            => UpdateAdornerPosition();

        protected void UpdateAdornerPosition()
        {
            if (Adorner is null)
                return;

            Adorner.Position = new Point(10d, ActualHeight * Hue / 360d);
        }

        protected static Color ToColor(double Hue)
            => Color.FromRgb(ToRGBByte(Hue, 5), ToRGBByte(Hue, 3), ToRGBByte(Hue, 1));
        private static byte ToRGBByte(double Hue, double n)
        {
            double k = (n + Hue / 60) % 6,
                   value = 1 - MathHelper.Clamp(Math.Min(k, 4 - k), 0d, 1d);
            return (byte)Math.Round(value * 255);
        }

        protected static double ToHue(Color Color)
        {
            // R > G >= B
            if (Color.R > Color.G && Color.G >= Color.B)
                return (Color.G - Color.B) * 60d / (Color.R - Color.B);

            // R > B > G
            if (Color.R > Color.B && Color.B > Color.G)
                return (Color.G - Color.B) * 60d / (Color.R - Color.G) + 360d;

            // G > B > R or G > R > B
            if (Color.G > Color.B)
                return (Color.B - Color.R) * 60d / (Color.G - (Color.B > Color.R ? Color.R : Color.B)) + 120d;

            // B > G > R or B > R > G
            return (Color.R - Color.G) * 60d / (Color.B - (Color.G > Color.R ? Color.R : Color.G)) + 240d;
        }

    }
}