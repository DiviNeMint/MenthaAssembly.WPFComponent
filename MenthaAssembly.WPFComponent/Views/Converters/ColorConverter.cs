using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public sealed class ColorConverter(string Mode) : IValueConverter
    {
        public static ColorConverter ColorToBrush { get; } = new(nameof(ColorToBrush));

        public static ColorConverter HueToColor { get; } = new(nameof(HueToColor));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (Mode)
            {
                case nameof(ColorToBrush):
                    {
                        Color? Color = value as Color?;
                        return Color.HasValue ? new SolidColorBrush(Color.Value) : value;
                    }
                case nameof(HueToColor):
                    {
                        double? Hue = value as double?;
                        return Hue.HasValue ? ToColor(Hue.Value) : Colors.Red;
                    }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (Mode)
            {
                case nameof(ColorToBrush):
                    {
                        return value is SolidColorBrush Brush ? Brush.Color : value;
                    }
                case nameof(HueToColor):
                    {
                        Color? Color = value as Color?;
                        return Color.HasValue ? ToHue(Color.Value) : 0d;
                    }
            }

            return value;
        }

        private static Color ToColor(double Hue)
            => Color.FromRgb(ToRGBByte(Hue, 5), ToRGBByte(Hue, 3), ToRGBByte(Hue, 1));
        private static byte ToRGBByte(double Hue, double n)
        {
            double k = (n + Hue / 60) % 6,
                   value = 1 - MathHelper.Clamp(Math.Min(k, 4 - k), 0d, 1d);
            return (byte)Math.Round(value * 255);
        }

        private static double ToHue(Color Color)
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