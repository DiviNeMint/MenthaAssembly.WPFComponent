using System;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    public static class ImageHelper
    {
        public static ImageContext ToImageContext(this BitmapSource This)
        {
            int PixelBytes = (This.Format.BitsPerPixel + 7) >> 3;
            int Stride = (((This.PixelWidth + 1) >> 1) << 1) * PixelBytes;
            byte[] Datas = new byte[Stride * This.PixelHeight];
            This.CopyPixels(Datas, Stride, 0);
            return new ImageContext(This.PixelWidth, This.PixelHeight, Datas);
        }

        //public static BitmapSource ToBitmapSource(this ImageContext This)
        //{
        //    WriteableBitmap Bitmap = new WriteableBitmap(This.Width, This.Height, 96, 96, PixelFormats.Bgra32, null);
        //    unsafe
        //    {
        //        byte* Scan0 = (byte*)Bitmap.BackBuffer;
        //        switch (This.Channels)
        //        {
        //            case 1:
        //                Parallel.For(0, Bitmap.PixelHeight, (j) =>
        //                {
        //                    byte* Scan = Scan0 + j * Bitmap.BackBufferStride;
        //                    for (int i = 0; i < This.Stride; i++)
        //                    {

        //                    }
        //                });
        //                break;
        //            case 3:
        //                break;
        //            case 4:
        //                break;
        //        }

        //        //int PixelBytes = (This.Format.BitsPerPixel + 7) >> 3;
        //        //int Stride = (((This.PixelWidth + 1) >> 1) << 1) * PixelBytes;

        //        //byte[] Datas = new byte[Stride * This.PixelHeight];
        //        //This.CopyPixels(Datas, Stride, 0);
        //    }
        //    return Bitmap;
        //}


        public static BitmapContext ToBitmapContext(this BitmapSource This)
           => new BitmapContext(new WriteableBitmap(This));

        public static BitmapContext ToBitmapContext(this WriteableBitmap This)
            => new BitmapContext(This);
    }
}
