using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    public class BitmapContext : ImageContext
    {
        protected WriteableBitmap Bitmap { get; }

        public BitmapContext(WriteableBitmap Bitmap) : base(Bitmap.PixelWidth, Bitmap.PixelHeight, Bitmap.BackBuffer, Bitmap.BackBufferStride, (Bitmap.Format.BitsPerPixel + 7) >> 3)
            => this.Bitmap = Bitmap;

        protected bool IsLocked { set; get; }
        public bool TryLock(int Timeout)
        {
            if (Bitmap?.TryLock(TimeSpan.FromMilliseconds(Timeout)) ?? false)
            {
                IsLocked = true;
                return true;
            }
            return false;
        }

        public void AddDirtyRect(Int32Rect Rect)
        {
            if (IsLocked)
                Bitmap?.AddDirtyRect(Rect);
        }
        public void AddDirtyRect(int X, int Y, int Width, int Height)
            => AddDirtyRect(new Int32Rect(X, Y, Width, Height));
        public void AddDirtyRect(Int32Point Point, int Width, int Height)
            => AddDirtyRect(new Int32Rect(Point.X, Point.Y, Width, Height));
        public void AddDirtyRect(int X, int Y, Int32Size Size)
            => AddDirtyRect(new Int32Rect(X, Y, Size.Width, Size.Height));
        public void AddDirtyRect(Int32Point Point, Int32Size Size)
            => AddDirtyRect(new Int32Rect(Point.X, Point.Y, Size.Width, Size.Height));


        public void Unlock()
        {
            if (IsLocked)
                Bitmap?.Unlock();
        }

    }
}
