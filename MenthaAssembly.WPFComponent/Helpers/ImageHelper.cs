using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            return new ImageContext(This.PixelWidth, This.PixelHeight, Datas, Stride, PixelBytes);
        }

        public static BitmapContext ToBitmapContext(this BitmapSource This)
           => new BitmapContext(new WriteableBitmap(This));

        public static BitmapContext ToBitmapContext(this WriteableBitmap This)
            => new BitmapContext(This);
    }
}
