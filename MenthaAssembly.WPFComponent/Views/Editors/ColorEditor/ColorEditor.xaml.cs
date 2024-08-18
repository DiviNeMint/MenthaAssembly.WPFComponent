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
            DependencyProperty.Register(nameof(Color), typeof(Color?), typeof(ColorEditor), new PropertyMetadata(Colors.Red,
                  (d, e) =>
                  {
                      if (d is ColorEditor This &&
                          !This.IsUpdating)
                          This.OnColorChanged(new ChangedEventArgs<Color?>(e.OldValue, e.NewValue));
                  }));
        public Color? Color
        {
            get => (Color?)GetValue(ColorProperty);
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
            HuePicker.HueProperty.AddOwner(typeof(ColorEditor), new PropertyMetadata(0d, OnHSBChanged, HuePicker.CoerceHueValue));
        public double Hue
        {
            get => (double)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        public static readonly DependencyProperty SaturationProperty =
            SaturationBrightnessPicker.SaturationProperty.AddOwner(typeof(ColorEditor),
                new PropertyMetadata(1d, OnHSBChanged, SaturationBrightnessPicker.CoerceSaturationBrightness));
        public double Saturation
        {
            get => (double)GetValue(SaturationProperty);
            set => SetValue(SaturationProperty, value);
        }

        public static readonly DependencyProperty BrightnessProperty =
            SaturationBrightnessPicker.BrightnessProperty.AddOwner(typeof(ColorEditor),
                new PropertyMetadata(1d, OnHSBChanged, SaturationBrightnessPicker.CoerceSaturationBrightness));
        public double Brightness
        {
            get => (double)GetValue(BrightnessProperty);
            set => SetValue(BrightnessProperty, value);
        }

        public static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register(nameof(Alpha), typeof(byte), typeof(ColorEditor),
                new PropertyMetadata(byte.MaxValue, OnARGBChanged));
        public byte Alpha
        {
            get => (byte)GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }

        public static readonly DependencyProperty RProperty =
            DependencyProperty.Register(nameof(R), typeof(byte), typeof(ColorEditor),
                new PropertyMetadata(byte.MaxValue, OnARGBChanged));
        public byte R
        {
            get => (byte)GetValue(RProperty);
            set => SetValue(RProperty, value);
        }

        public static readonly DependencyProperty GProperty =
            DependencyProperty.Register(nameof(G), typeof(byte), typeof(ColorEditor),
                new PropertyMetadata(byte.MinValue, OnARGBChanged));
        public byte G
        {
            get => (byte)GetValue(GProperty);
            set => SetValue(GProperty, value);
        }

        public static readonly DependencyProperty BProperty =
            DependencyProperty.Register(nameof(B), typeof(byte), typeof(ColorEditor),
                new PropertyMetadata(byte.MinValue, OnARGBChanged));
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

            if (GetTemplateChild("PART_OriginalColorRect") is Rectangle OriginalColorRect &&
                GetTemplateChild("PART_ColorEyedropper") is ColorEyedropper ColorEyedropper)
            {
                BindingOperations.SetBinding(OriginalColorRect, Shape.FillProperty, new Binding(nameof(ColorEyedropper.OriginalColor))
                {
                    Source = ColorEyedropper,
                    TargetNullValue = Brushes.Transparent,
                    Converter = ColorConverter.ColorToBrush
                });

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

                        if (OriginalColorRect.IsMouseOver)
                            Color = ColorEyedropper.OriginalColor;
                    }
                };
            }

            if (GetTemplateChild("PART_CurrentColorRect") is Rectangle CurrentColorRect)
            {
                BindingOperations.SetBinding(CurrentColorRect, Shape.FillProperty, new Binding(nameof(Color))
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                    Converter = ColorConverter.ColorToBrush
                });
            }

        }

        protected bool IsUpdating = false;
        protected virtual void OnColorChanged(ChangedEventArgs<Color?> e)
        {
            try
            {
                IsUpdating = true;
                Color? New = e.NewValue;
                if (New.HasValue)
                {
                    Color Value = New.Value;
                    ToHSB(Value, out double H, out double S, out double B);
                    Hue = H;
                    Saturation = S;
                    Brightness = B;
                    Alpha = Value.A;
                    R = Value.R;
                    G = Value.G;
                    this.B = Value.B;
                }
                else
                {
                    Hue = 0d;
                    Saturation = 1d;
                    Brightness = 1d;
                    Alpha = byte.MaxValue;
                    R = byte.MaxValue;
                    G = byte.MinValue;
                    B = byte.MinValue;
                }
            }
            finally
            {
                IsUpdating = false;
            }
        }

        private static void OnHSBChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorEditor This &&
                !This.IsUpdating)
                This.OnHSBChanged();
        }
        protected virtual void OnHSBChanged()
        {
            try
            {
                IsUpdating = true;
                double H = Hue,
                       S = Saturation,
                       B = Brightness;
                Color NewColor = System.Windows.Media.Color.FromArgb(Alpha, ToRGBByte(H, S, B, 5), ToRGBByte(H, S, B, 3), ToRGBByte(H, S, B, 1));

                R = NewColor.R;
                G = NewColor.G;
                this.B = NewColor.B;
                Color = NewColor;
            }
            finally
            {
                IsUpdating = false;
            }
        }

        private static void OnARGBChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorEditor This &&
                !This.IsUpdating)
                This.OnARGBChanged();
        }
        protected virtual void OnARGBChanged()
        {
            try
            {
                IsUpdating = true;
                Color NewColor = System.Windows.Media.Color.FromArgb(Alpha, R, G, this.B);
                ToHSB(NewColor, out double H, out double S, out double B);
                Hue = H;
                Saturation = S;
                Brightness = B;
                Color = NewColor;
            }
            finally
            {
                IsUpdating = false;
            }
        }

        private static byte ToRGBByte(double Hue, double S, double B, double n)
        {
            double k = (n + Hue / 60) % 6,
                   value = B - B * S * MathHelper.Clamp(Math.Min(k, 4 - k), 0d, 1d);
            return (byte)Math.Round(value * 255);
        }

        private void ToHSB(Color Color, out double H, out double S, out double B)
        {
            MathHelper.MinAndMax(out byte Min, out byte Max, Color.R, Color.G, Color.B);

            double Delta = Max - Min;
            B = Max / 255d;
            S = Max == 0 ? 0 : Delta / Max;
            H = Delta == 0
                ? Hue
                : Max == Color.R && Color.G >= Color.B
                ? (Color.G - Color.B) * 60d / Delta
                : Max == Color.R && Color.G < Color.B
                ? (Color.B - Color.G) * -60d / Delta + 360d
                : Max == Color.G
                ? (Color.B > Color.R ? ((Color.B - Color.R) * 60d) : ((Color.R - Color.B) * -60d)) / Delta + 120d
                : Max == Color.B ? (Color.R > Color.G ? ((Color.R - Color.G) * 60d) : ((Color.G - Color.R) * -60d)) / Delta + 240d : 0d;
        }

    }
}