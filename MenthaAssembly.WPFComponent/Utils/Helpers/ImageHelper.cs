using MenthaAssembly.Media.Imaging;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;

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

        public static Bitmap CreateBitmap(UIElement Element, int Width, int Height)
        {
            Element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Element.Arrange(new Rect(new Point(), Element.DesiredSize));

            RenderTargetBitmap RenderBitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            RenderBitmap.Render(Element);

            return CreateBitmap(RenderBitmap.ToImageContext());
        }
        public static Bitmap CreateBitmap(DrawingImage Image, int Width, int Height, double Angle)
        {
            DrawingVisual Visual = new DrawingVisual();
            using (DrawingContext Context = Visual.RenderOpen())
            {
                double Cx = Image.Width * 0.5d,
                       Cy = Image.Height * 0.5d;
                if (Angle != 0)
                    Context.PushTransform(new RotateTransform(Angle, Cx, Cy));

                Context.PushTransform(new ScaleTransform(Width / Image.Width, Height / Image.Height, Cx, Cy));

                Context.DrawDrawing(Image.Drawing);
            }

            RenderTargetBitmap RenderBitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            RenderBitmap.Render(Visual);

            return CreateBitmap(RenderBitmap.ToImageContext());
        }
        public static Bitmap CreateBitmap(IImageContext Image)
        {
            using MemoryStream memoryStream = new MemoryStream();
            PngCoder.Encode(Image, memoryStream, true);
            return new Bitmap(memoryStream);
        }

    }
}
