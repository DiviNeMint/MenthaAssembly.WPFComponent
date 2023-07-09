using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Utils;
using MenthaAssembly.Win32;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    public static unsafe class ImageHelper
    {
        public static IImageContext ToImageContext(this ImageSource This)
        {
            if (This is WriteableBitmap SharedBmp)
                return new BitmapContext(SharedBmp);

            if (This is BitmapSource Bmp)
            {
                IImageContext Image;
                int Iw = Bmp.PixelWidth,
                    Ih = Bmp.PixelHeight;

                if (PixelFormats.Bgr24.Equals(Bmp.Format))
                    Image = new ImageContext<BGR>(Iw, Ih);

                else if (PixelFormats.Bgr32.Equals(Bmp.Format) ||
                         PixelFormats.Bgra32.Equals(Bmp.Format) ||
                         PixelFormats.Pbgra32.Equals(Bmp.Format))
                    Image = new ImageContext<BGRA>(Iw, Ih);

                else Image = PixelFormats.Rgb24.Equals(Bmp.Format)
                    ? new ImageContext<RGB>(Iw, Ih)
                    : PixelFormats.Gray8.Equals(Bmp.Format) ? (IImageContext)new ImageContext<Gray8>(Iw, Ih) : throw new NotImplementedException();

                long Stride = Image.Stride;
                Bmp.CopyPixels(Int32Rect.Empty, Image.Scan0[0], (int)(Stride * Ih), (int)Stride);
                return Image;
            }

            if (This is DrawingImage Drawing)
            {
                int Iw = (int)Math.Ceiling(Drawing.Width),
                    Ih = (int)Math.Ceiling(Drawing.Height);
                IImageContext Image = new ImageContext<BGRA>(Iw, Ih);

                DrawingVisual Visual = new();
                using (DrawingContext Context = Visual.RenderOpen())
                {
                    Context.DrawDrawing(Drawing.Drawing);
                }

                RenderTargetBitmap RenderBitmap = new(Iw, Ih, 96, 96, PixelFormats.Pbgra32);
                RenderBitmap.Render(Visual);

                long Stride = Image.Stride;
                RenderBitmap.CopyPixels(Int32Rect.Empty, Image.Scan0[0], (int)(Stride * Ih), (int)Stride);
                ReapplyOpacity(Image, 1d);
                return Image;
            }

            throw new NotSupportedException();
        }

        public static WriteableBitmap ToBitmapSource(this IImageContext This)
        {
            int Iw = This.Width,
                Ih = This.Height;
            WriteableBitmap Bitmap = new(Iw, Ih, 96d, 96d, PixelFormats.Bgra32, null);
            try
            {
                Bitmap.Lock();
                This.BlockCopy<BGRA>(0, 0, Iw, Ih, Bitmap.BackBuffer, Bitmap.BackBufferStride, null);
                Bitmap.AddDirtyRect(new Int32Rect(0, 0, Iw, Ih));
            }
            finally
            {
                Bitmap.Unlock();
            }

            return Bitmap;
        }

        public static RenderTargetBitmap ToBitmapSource(this UIElement This)
            => ToBitmapSource(This, 0, 0);
        public static RenderTargetBitmap ToBitmapSource(this UIElement This, int Width, int Height)
        {
            bool IsCalculate = false;
            if (!This.IsMeasureValid ||
                !This.IsArrangeValid)
            {
                This.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                This.Arrange(new Rect(This.DesiredSize));
                IsCalculate = true;
            }

            HwndSource Source = This.IsVisible ? null : new HwndSource(new HwndSourceParameters()) { RootVisual = This };

            try
            {
                Rect Bound = VisualTreeHelper.GetDescendantBounds(This);

                DrawingVisual Drawing = new();
                using (DrawingContext Context = Drawing.RenderOpen())
                {
                    if ((Width == 0 && Height == 0) ||
                        (Width == Bound.Width && Height == Bound.Height))
                    {
                        Context.DrawRectangle(new VisualBrush(This), null, Bound);
                    }
                    else
                    {
                        Context.PushTransform(new ScaleTransform(Width / Bound.Width, Height / Bound.Height, 0, 0));
                        Context.DrawRectangle(new VisualBrush(This), null, Bound);
                        Context.Pop();
                    }
                }

                RenderTargetBitmap Image = new(Width, Height, 96d, 96d, PixelFormats.Pbgra32);
                Image.Render(Drawing);
                return Image;
            }
            finally
            {
                Source?.Dispose();
                if (IsCalculate)
                {
                    This.InvalidateMeasure();
                    This.InvalidateArrange();
                }
            }
        }

        public static IntPtr CreateHBitmap(this UIElement This, int Width, int Height)
            => CreateHBitmap(This, Width, Height, 1d);
        public static IntPtr CreateHBitmap(this UIElement This, int Width, int Height, double Opacity)
        {
            IImageContext Image = This.ToBitmapSource(Width, Height)
                                      .ToImageContext();

            ReapplyOpacity(Image, Opacity);
            return Image.CreateHBitmap();
        }

        public static IntPtr CreateHBitmap(this DrawingImage This, int Width, int Height, double Angle)
            => CreateHBitmap(This, Width, Height, Angle, 1d);
        public static IntPtr CreateHBitmap(this DrawingImage This, int Width, int Height, double Angle, double Opacity)
        {
            double Iw = This.Width,
                   Ih = This.Height;

            DrawingVisual Visual = new();
            using (DrawingContext Context = Visual.RenderOpen())
            {
                bool HasPush = false;
                if ((Width != 0 || Height != 0) &&
                    (Width != Iw || Height != Ih))
                {
                    Context.PushTransform(new ScaleTransform(Width / Iw, Height / Ih, 0, 0));
                    HasPush = true;
                }

                if (Angle != 0)
                {
                    Context.PushTransform(new RotateTransform(Angle, Iw / 2d, Ih / 2d));
                    HasPush = true;
                }

                Context.DrawDrawing(This.Drawing);

                if (HasPush)
                    Context.Pop();
            }

            RenderTargetBitmap RenderBitmap = new(Width, Height, 96, 96, PixelFormats.Pbgra32);
            RenderBitmap.Render(Visual);

            IImageContext Image = RenderBitmap.ToImageContext();
            ReapplyOpacity(Image, Opacity);
            return Image.CreateHBitmap();
        }

        private static void ReapplyOpacity(IImageContext Image, double Opacity)
        {
            Opacity = MathHelper.Clamp(Opacity, 0d, 1d);
            byte Alpha = (byte)Math.Round(byte.MaxValue * Opacity);
            int Iw = Image.Width,
                Ih = Image.Height;

            BGRA Color;
            PixelAdapter<BGRA> Adapter = Image.GetAdapter<BGRA>(0, 0);
            for (int y = 0; y < Ih; y++, Adapter.DangerousMoveNextY())
            {
                for (int x = 0; x < Iw; x++, Adapter.DangerousMoveNextX())
                {
                    Adapter.OverrideTo(&Color);

                    byte A = Color.A;
                    if (A == byte.MaxValue)
                    {
                        // Apply
                        if (Alpha < byte.MaxValue)
                        {
                            Color.A = Alpha;
                            Adapter.Override(Color);
                        }
                    }
                    else
                    {
                        // Invert
                        double Factor = 255d / A;
                        Color.R = (byte)Math.Round(Color.R * Factor);
                        Color.G = (byte)Math.Round(Color.G * Factor);
                        Color.B = (byte)Math.Round(Color.B * Factor);

                        // Apply
                        if (Opacity < 1d)
                            Color.A = (byte)Math.Round(A * Opacity);

                        Adapter.Override(Color);
                    }
                }

                Adapter.DangerousOffsetX(-Iw);
            }
        }

        public static IntPtr CreateHIcon(this UIElement This, int Width, int Height)
            => CreateHIcon(This, Width, Height, 1d);
        public static IntPtr CreateHIcon(this UIElement This, int Width, int Height, double Opacity)
        {
            IntPtr HColor = This.CreateHBitmap(Width, Height, Opacity),
                   HMask = Graphic.CreateBitmap(Width, Height, 1, 1, IntPtr.Zero);
            try
            {
                return Graphic.CreateHIcon(HColor, HMask);
            }
            finally
            {
                Graphic.DeleteObject(HColor);
                Graphic.DeleteObject(HMask);
            }
        }
        public static IntPtr CreateHIcon(this UIElement This, int Width, int Height, int xHotSpot, int yHotSpot)
            => CreateHIcon(This, Width, Height, xHotSpot, yHotSpot, 1d);
        public static IntPtr CreateHIcon(this UIElement This, int Width, int Height, int xHotSpot, int yHotSpot, double Opacity)
        {
            IntPtr HColor = This.CreateHBitmap(Width, Height, Opacity),
                   HMask = Graphic.CreateBitmap(Width, Height, 1, 1, IntPtr.Zero);
            try
            {
                return Graphic.CreateHIcon(HColor, HMask, xHotSpot, yHotSpot);
            }
            finally
            {
                Graphic.DeleteObject(HColor);
                Graphic.DeleteObject(HMask);
            }
        }

        public static IntPtr CreateHIcon(this DrawingImage This, int Width, int Height, double Angle)
            => CreateHIcon(This, Width, Height, Angle, 1d);
        public static IntPtr CreateHIcon(this DrawingImage This, int Width, int Height, double Angle, double Opacity)
        {
            IntPtr HColor = This.CreateHBitmap(Width, Height, Angle, Opacity),
                   HMask = Graphic.CreateBitmap(Width, Height, 1, 1, IntPtr.Zero);
            try
            {
                return Graphic.CreateHIcon(HColor, HMask);
            }
            finally
            {
                Graphic.DeleteObject(HColor);
                Graphic.DeleteObject(HMask);
            }
        }
        public static IntPtr CreateHIcon(this DrawingImage This, int Width, int Height, double Angle, int xHotSpot, int yHotSpot)
            => CreateHIcon(This, Width, Height, Angle, xHotSpot, yHotSpot, 1d);
        public static IntPtr CreateHIcon(this DrawingImage This, int Width, int Height, double Angle, int xHotSpot, int yHotSpot, double Opacity)
        {
            IntPtr HColor = This.CreateHBitmap(Width, Height, Angle, Opacity),
                   HMask = Graphic.CreateBitmap(Width, Height, 1, 1, IntPtr.Zero);
            try
            {
                return Graphic.CreateHIcon(HColor, HMask, xHotSpot, yHotSpot);
            }
            finally
            {
                Graphic.DeleteObject(HColor);
                Graphic.DeleteObject(HMask);
            }
        }

    }
}