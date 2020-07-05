using MenthaAssembly.Views.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public class ColorEditor : Control
    {
        public static readonly DependencyProperty ColorProperty =
              DependencyProperty.Register("Color", typeof(Color), typeof(ColorEditor), new PropertyMetadata(Colors.Red,
                  (d, e) =>
                  {
                      if (d is ColorEditor This &&
                          !This.IsUpdating)
                          This.OnColorChanged(new ChangedEventArgs<Color>(e.OldValue, e.NewValue));
                  }));
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty IsColorCapturingProperty =
              ColorEyedropper.IsCapturingProperty.AddOwner(typeof(ColorEditor), new PropertyMetadata(false));
        public bool IsColorCapturing
        {
            get => (bool)GetValue(IsColorCapturingProperty);
            set => SetValue(IsColorCapturingProperty, value);
        }

        public static readonly DependencyProperty HueProperty =
              HuePicker.HueProperty.AddOwner(typeof(ColorEditor), new PropertyMetadata(0d,
                  (d, e) =>
                  {
                      if (d is ColorEditor This &&
                          !This.IsUpdating)
                          This.OnHSBChanged();
                  },
                  HuePicker.CoerceHueValue));
        public double Hue
        {
            get => (double)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        public static readonly DependencyProperty SaturationProperty =
              SaturationBrightnessPicker.SaturationProperty.AddOwner(typeof(ColorEditor), new PropertyMetadata(1d,
                  (d, e) =>
                  {
                      if (d is ColorEditor This &&
                          !This.IsUpdating)
                          This.OnHSBChanged();
                  },
                  SaturationBrightnessPicker.CoerceSaturationBrightnessValue));
        public double Saturation
        {
            get => (double)GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        public static readonly DependencyProperty BrightnessProperty =
              SaturationBrightnessPicker.BrightnessProperty.AddOwner(typeof(ColorEditor), new PropertyMetadata(1d,
                  (d, e) =>
                  {
                      if (d is ColorEditor This &&
                          !This.IsUpdating)
                          This.OnHSBChanged();
                  },
                  SaturationBrightnessPicker.CoerceSaturationBrightnessValue));
        public double Brightness
        {
            get => (double)GetValue(BrightnessProperty);
            set => SetValue(BrightnessProperty, value);
        }

        public static readonly DependencyProperty AlphaProperty =
              DependencyProperty.Register("Alpha", typeof(byte), typeof(ColorEditor), new PropertyMetadata(byte.MaxValue,
                  (d, e) =>
                  {
                      if (d is ColorEditor This &&
                          !This.IsUpdating)
                          This.OnARGBChanged();
                  }));
        public byte Alpha
        {
            get => (byte)GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }

        public static readonly DependencyProperty RProperty =
              DependencyProperty.Register("R", typeof(byte), typeof(ColorEditor), new PropertyMetadata(byte.MaxValue,
                  (d, e) =>
                  {
                      if (d is ColorEditor This &&
                          !This.IsUpdating)
                          This.OnARGBChanged();
                  }));
        public byte R
        {
            get => (byte)GetValue(RProperty);
            set => SetValue(RProperty, value);
        }

        public static readonly DependencyProperty GProperty =
              DependencyProperty.Register("G", typeof(byte), typeof(ColorEditor), new PropertyMetadata(byte.MinValue,
                  (d, e) =>
                  {
                      if (d is ColorEditor This &&
                          !This.IsUpdating)
                          This.OnARGBChanged();
                  }));
        public byte G
        {
            get => (byte)GetValue(GProperty);
            set => SetValue(GProperty, value);
        }

        public static readonly DependencyProperty BProperty =
              DependencyProperty.Register("B", typeof(byte), typeof(ColorEditor), new PropertyMetadata(byte.MinValue,
                  (d, e) =>
                  {
                      if (d is ColorEditor This &&
                          !This.IsUpdating)
                          This.OnARGBChanged();
                  }));
        public byte B
        {
            get => (byte)GetValue(BProperty);
            set => SetValue(BProperty, value);
        }


        static ColorEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorEditor), new FrameworkPropertyMetadata(typeof(ColorEditor)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.GetTemplateChild("PART_OriginalColorRect") is Rectangle OriginalColorRect)
            {
                bool IsRectLeftMouseDown = false;
                OriginalColorRect.MouseDown += (s, e) =>
                {
                    if (e.ChangedButton == MouseButton.Left &&
                        !IsRectLeftMouseDown)
                    {
                        IsRectLeftMouseDown = true;
                        OriginalColorRect.CaptureMouse();
                    }
                };
                OriginalColorRect.MouseUp += (s, e) =>
                {
                    if (IsRectLeftMouseDown)
                    {
                        OriginalColorRect.ReleaseMouseCapture();
                        IsRectLeftMouseDown = false;

                        if (OriginalColorRect.IsMouseOver &&
                             OriginalColorRect.Fill is SolidColorBrush Brush)
                            Color = Brush.Color;
                    }
                };
            }
        }

        protected bool IsUpdating = false;
        protected virtual void OnColorChanged(ChangedEventArgs<Color> e)
        {
            try
            {
                IsUpdating = true;
                ToHSB(e.NewValue, out double H, out double S, out double B);
                this.Hue = H;
                this.Saturation = S;
                this.Brightness = B;
                this.Alpha = e.NewValue.A;
                this.R = e.NewValue.R;
                this.G = e.NewValue.G;
                this.B = e.NewValue.B;
            }
            finally
            {
                IsUpdating = false;
            }
        }
        protected virtual void OnHSBChanged()
        {
            try
            {
                IsUpdating = true;
                double H = this.Hue,
                       S = this.Saturation,
                       B = this.Brightness;
                Color NewColor = Color.FromArgb(this.Alpha, GetRGBByte(H, S, B, 5), GetRGBByte(H, S, B, 3), GetRGBByte(H, S, B, 1));

                this.R = NewColor.R;
                this.G = NewColor.G;
                this.B = NewColor.B;
                Color = NewColor;
            }
            finally
            {
                IsUpdating = false;
            }
        }
        protected virtual void OnARGBChanged()
        {
            try
            {
                IsUpdating = true;
                Color NewColor = Color.FromArgb(this.Alpha, this.R, this.G, this.B);
                ToHSB(NewColor, out double H, out double S, out double B);
                this.Hue = H;
                this.Saturation = S;
                this.Brightness = B;
                Color = NewColor;
            }
            finally
            {
                IsUpdating = false;
            }
        }

        private byte GetRGBByte(double Hue, double S, double B, double n)
        {
            double k = (n + Hue / 60) % 6,
                   value = B - B * S * Math.Max(Math.Min(Math.Min(k, 4 - k), 1), 0);
            return (byte)Math.Round(value * 255);
        }

        private void ToHSB(Color Color, out double H, out double S, out double B)
        {
            byte Max = Math.Max(Math.Max(Color.R, Color.G), Color.B),
                 Min = Math.Min(Math.Min(Color.R, Color.G), Color.B);

            double Delta = Max - Min;
            B = Max / 255d;
            S = Max == 0 ? 0 : Delta / Max;

            if (Delta == 0)
                H = this.Hue;
            else if (Max == Color.R && Color.G >= Color.B)
                H = (Color.G - Color.B) * 60d / Delta;
            else if (Max == Color.R && Color.G < Color.B)
                H = (Color.G - Color.B) * 60d / Delta + 360d;
            else if (Max == Color.G)
                H = (Color.B - Color.R) * 60d / Delta + 120d;
            else if (Max == Color.B)
                H = (Color.R - Color.G) * 60d / Delta + 240d;
            else
                H = 0d;
        }

    }
}
