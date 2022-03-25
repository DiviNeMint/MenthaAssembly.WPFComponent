using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    public static class ImageHelper
    {
        public static IImageContext ToImageContext(this BitmapSource This)
        {
            if (This is WriteableBitmap bmp)
                return ToImageContext(bmp);

            IImageContext Image;
            int Width = This.PixelWidth,
                Height = This.PixelHeight;

            if (PixelFormats.Bgr24.Equals(This.Format))
                Image = new ImageContext<BGR>(Width, Height);
            else if (PixelFormats.Bgr32.Equals(This.Format) ||
                     PixelFormats.Bgra32.Equals(This.Format) ||
                     PixelFormats.Pbgra32.Equals(This.Format))
                Image = new ImageContext<BGRA>(Width, Height);
            else if (PixelFormats.Rgb24.Equals(This.Format))
                Image = new ImageContext<RGB>(Width, Height);
            else if (PixelFormats.Gray8.Equals(This.Format))
                Image = new ImageContext<Gray8>(Width, Height);
            else
                throw new NotImplementedException();

            int Stride = (int)Image.Stride;
            This.CopyPixels(Int32Rect.Empty, Image.Scan0, Stride * Image.Height, Stride);
            return Image;
        }
        private static IImageContext ToImageContext(WriteableBitmap This)
        {
            int Width = This.PixelWidth,
                Height = This.PixelHeight,
                Stride = This.BackBufferStride;

            try
            {
                This.Lock();

                if (PixelFormats.Bgr24.Equals(This.Format))
                    return new ImageContext<BGR>(Width, Height, This.BackBuffer, Stride).Clone();
                else if (PixelFormats.Bgr32.Equals(This.Format) ||
                         PixelFormats.Bgra32.Equals(This.Format) ||
                         PixelFormats.Pbgra32.Equals(This.Format))
                    return new ImageContext<BGRA>(Width, Height, This.BackBuffer, Stride).Clone();
                else if (PixelFormats.Rgb24.Equals(This.Format))
                    return new ImageContext<RGB>(Width, Height, This.BackBuffer, Stride).Clone();
                else if (PixelFormats.Gray8.Equals(This.Format))
                    return new ImageContext<Gray8>(Width, Height, This.BackBuffer, Stride).Clone();

                throw new NotImplementedException();
            }
            finally
            {
                This.Unlock();
            }
        }

        public static IntPtr CreateHBitmap(this UIElement This, int Width, int Height)
        {
            This.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            This.Arrange(new Rect(new Point(), This.DesiredSize));

            RenderTargetBitmap RenderBitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            RenderBitmap.Render(This);

            return RenderBitmap.ToImageContext().CreateHBitmap();
        }
        public static IntPtr CreateHBitmap(this DrawingImage This, int Width, int Height, double Angle)
            => CreateHBitmap(This, Width, Height, Width, Height, Angle);
        public static IntPtr CreateHBitmap(this DrawingImage This, int RenderWidth, int RenderHeight, int IconWidth, int IconHeight, double Angle)
        {
            DrawingVisual Visual = new DrawingVisual();
            using (DrawingContext Context = Visual.RenderOpen())
            {
                double Sx = RenderWidth / This.Width,
                       Sy = RenderHeight / This.Height;
                Context.PushTransform(new ScaleTransform(Sx, Sy, 0, 0));

                if (Angle != 0)
                {
                    double Cx = This.Width * 0.5d,
                           Cy = This.Height * 0.5d;
                    Context.PushTransform(new RotateTransform(Angle, Cx, Cy));
                }

                Context.DrawDrawing(This.Drawing);
                Context.Pop();
            }

            RenderTargetBitmap RenderBitmap = new RenderTargetBitmap(IconWidth, IconHeight, 96, 96, PixelFormats.Pbgra32);
            RenderBitmap.Render(Visual);

            return RenderBitmap.ToImageContext().CreateHBitmap();
        }

        public static IntPtr CreateHIcon(this UIElement This, int Width, int Height)
            => Graphic.CreateHIcon(This.CreateHBitmap(Width, Height), new ImageContext<BGRA>(Width, Height).CreateHBitmap());
        public static IntPtr CreateHIcon(this UIElement This, int Width, int Height, int xHotSpot, int yHotSpot)
            => Graphic.CreateHIcon(This.CreateHBitmap(Width, Height), new ImageContext<BGRA>(Width, Height).CreateHBitmap(), xHotSpot, yHotSpot);
        public static IntPtr CreateHIcon(this DrawingImage This, int Width, int Height, double Angle)
            => Graphic.CreateHIcon(This.CreateHBitmap(Width, Height, Angle), new ImageContext<BGRA>(Width, Height).CreateHBitmap());
        public static IntPtr CreateHIcon(this DrawingImage This, int Width, int Height, double Angle, int xHotSpot, int yHotSpot)
            => Graphic.CreateHIcon(This.CreateHBitmap(Width, Height, Angle), new ImageContext<BGRA>(Width, Height).CreateHBitmap(), xHotSpot, yHotSpot);
        public static IntPtr CreateHIcon(this DrawingImage This, int RenderWidth, int RenderHeight, int IconWidth, int IconHeight, double Angle, int xHotSpot, int yHotSpot)
            => Graphic.CreateHIcon(This.CreateHBitmap(RenderWidth, RenderHeight, IconWidth, IconHeight, Angle),
                                   new ImageContext<BGRA>(IconWidth, IconHeight).CreateHBitmap(),
                                   xHotSpot,
                                   yHotSpot);

    }
}
