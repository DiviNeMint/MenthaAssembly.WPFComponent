using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    public static class ImageHelper
    {
        public unsafe static IImageContext ToImageContext(this BitmapSource This)
        {
            if (PixelFormats.Bgr24.Equals(This.Format))
            {
                int Stride = This.PixelWidth * sizeof(BGR);
                byte[] Datas = new byte[Stride * This.PixelHeight];
                This.CopyPixels(Datas, Stride, 0);
                return new ImageContext<BGR>(This.PixelWidth,
                                             This.PixelHeight,
                                             Datas,
                                             This.Palette?.Colors.Select(i => new BGR(i.B, i.G, i.R))
                                                                 .ToList());
            }
            else if (PixelFormats.Bgr32.Equals(This.Format))
            {
                int Stride = This.PixelWidth * sizeof(BGRA);
                byte[] Datas = new byte[Stride * This.PixelHeight];
                This.CopyPixels(Datas, Stride, 0);
                return new ImageContext<BGRA>(This.PixelWidth,
                                              This.PixelHeight,
                                              Datas,
                                              This.Palette?.Colors.Select(i => new BGRA(i.B, i.G, i.R, i.A))
                                                                  .ToList());
            }
            else if (PixelFormats.Bgra32.Equals(This.Format) ||
                     PixelFormats.Pbgra32.Equals(This.Format))
            {
                int Stride = This.PixelWidth * sizeof(BGRA);
                byte[] Datas = new byte[Stride * This.PixelHeight];
                This.CopyPixels(Datas, Stride, 0);
                return new ImageContext<BGRA>(This.PixelWidth,
                                              This.PixelHeight,
                                              Datas,
                                              This.Palette?.Colors.Select(i => new BGRA(i.B, i.G, i.R, i.A))
                                                                  .ToList());
            }
            else if (PixelFormats.Rgb24.Equals(This.Format))
            {
                int Stride = This.PixelWidth * sizeof(BGRA);
                byte[] Datas = new byte[Stride * This.PixelHeight];
                This.CopyPixels(Datas, Stride, 0);
                return new ImageContext<RGB>(This.PixelWidth,
                                             This.PixelHeight,
                                             Datas,
                                             This.Palette?.Colors.Select(i => new RGB(i.R, i.G, i.B))
                                                                 .ToList());
            }
            else if (PixelFormats.Gray8.Equals(This.Format))
            {
                int Stride = This.PixelWidth * sizeof(BGRA);
                byte[] Datas = new byte[Stride * This.PixelHeight];
                This.CopyPixels(Datas, Stride, 0);
                return new ImageContext<Gray8>(This.PixelWidth,
                                               This.PixelHeight,
                                               Datas,
                                               This.Palette?.Colors.Select(i => new Gray8(i.B, i.G, i.R))
                                                                   .ToList());
            }

            throw new NotImplementedException();
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
