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
        public static IImageContext ToImageContext(this ImageSource This)
        {
            if (This is WriteableBitmap SharedBmp)
                return new BitmapContext(SharedBmp);

            if (This is BitmapSource Bmp)
            {
                IImageContext Image;
                int Width = Bmp.PixelWidth,
                    Height = Bmp.PixelHeight;

                if (PixelFormats.Bgr24.Equals(Bmp.Format))
                    Image = new ImageContext<BGR>(Width, Height);
                else if (PixelFormats.Bgr32.Equals(Bmp.Format) ||
                         PixelFormats.Bgra32.Equals(Bmp.Format) ||
                         PixelFormats.Pbgra32.Equals(Bmp.Format))
                    Image = new ImageContext<BGRA>(Width, Height);
                else if (PixelFormats.Rgb24.Equals(Bmp.Format))
                    Image = new ImageContext<RGB>(Width, Height);
                else if (PixelFormats.Gray8.Equals(Bmp.Format))
                    Image = new ImageContext<Gray8>(Width, Height);
                else
                    throw new NotImplementedException();

                int Stride = (int)Image.Stride;
                Bmp.CopyPixels(Int32Rect.Empty, Image.Scan0, Stride * Image.Height, Stride);
                return Image;
            }

            if (This is DrawingImage Drawing)
            {
                int Width = (int)Drawing.Width,
                    Height = (int)Drawing.Height;
                IImageContext Image = new ImageContext<BGRA>(Width, Height);

                DrawingVisual Visual = new DrawingVisual();
                using (DrawingContext Context = Visual.RenderOpen())
                {
                    Context.DrawDrawing(Drawing.Drawing);
                    Context.Pop();
                }

                RenderTargetBitmap RenderBitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
                RenderBitmap.Render(Visual);

                int Stride = (int)Image.Stride;
                RenderBitmap.CopyPixels(Int32Rect.Empty, Image.Scan0, Stride * Height, Stride);
                return Image;
            }

            throw new NotSupportedException();
        }

        public static RenderTargetBitmap ToBitmapSource(this UIElement This)
        {
            bool IsCalculate = false;
            if (!This.IsMeasureValid ||
                !This.IsArrangeValid)
            {
                This.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                This.Arrange(new Rect(This.DesiredSize));
                IsCalculate = true;
            }

            try
            {
                Rect Bound = VisualTreeHelper.GetDescendantBounds(This);

                DrawingVisual Drawing = new DrawingVisual();
                using (DrawingContext Context = Drawing.RenderOpen())
                {
                    Context.DrawRectangle(new VisualBrush(This), null, Bound);
                }

                RenderTargetBitmap Image = new RenderTargetBitmap((int)Math.Round(Bound.Right, MidpointRounding.AwayFromZero),
                                                                  (int)Math.Round(Bound.Bottom, MidpointRounding.AwayFromZero),
                                                                  96d, 96d, PixelFormats.Pbgra32);

                Image.Render(Drawing);
                return Image;
            }
            finally
            {
                if (IsCalculate)
                {
                    This.InvalidateMeasure();
                    This.InvalidateArrange();
                }
            }
        }
        public static RenderTargetBitmap ToBitmapSource(UIElement Element, double Dpi)
        {
            bool IsCalculate = false;
            if (!Element.IsMeasureValid ||
                !Element.IsArrangeValid)
            {
                Element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Element.Arrange(new Rect(Element.DesiredSize));
                IsCalculate = true;
            }

            try
            {
                Rect Bound = VisualTreeHelper.GetDescendantBounds(Element);

                double Scale = Dpi / 96d,
                       Width = Bound.Right * Scale,
                       Height = Bound.Bottom * Scale;

                DrawingVisual Drawing = new DrawingVisual();
                using (DrawingContext Context = Drawing.RenderOpen())
                {
                    Context.DrawRectangle(new VisualBrush(Element), null, new Rect(Bound.TopLeft, new Point(Width / Scale, Height / Scale)));
                }

                RenderTargetBitmap Bitmap = new RenderTargetBitmap((int)Math.Round(Width, MidpointRounding.AwayFromZero),
                                                                   (int)Math.Round(Height, MidpointRounding.AwayFromZero),
                                                                   Dpi, Dpi, PixelFormats.Pbgra32);

                Bitmap.Render(Drawing);
                return Bitmap;
            }
            finally
            {
                if (IsCalculate)
                {
                    Element.InvalidateMeasure();
                    Element.InvalidateArrange();
                }
            }
        }

        public static IntPtr CreateHBitmap(this UIElement This, int Width, int Height)
        {
            Rect Bound = VisualTreeHelper.GetDescendantBounds(This);

            DrawingVisual Drawing = new DrawingVisual();
            using (DrawingContext Context = Drawing.RenderOpen())
            {
                double Sx = Width / Bound.Width,
                       Sy = Height / Bound.Height;
                Context.PushTransform(new ScaleTransform(Sx, Sy, 0, 0));
                Context.DrawRectangle(new VisualBrush(This), null, Bound);
                Context.Pop();
            }

            RenderTargetBitmap RenderBitmap = new RenderTargetBitmap(Width, Height, 96d, 96d, PixelFormats.Pbgra32);
            RenderBitmap.Render(Drawing);

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