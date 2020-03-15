using MenthaAssembly.Media.Imaging;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    public class BitmapContext : ImageContext<BGRA>
    {
        protected WriteableBitmap Bitmap { get; }

        public BitmapContext(WriteableBitmap Bitmap) :
            base(Bitmap.PixelWidth,
                 Bitmap.PixelHeight,
                 Bitmap.BackBuffer,
                 Bitmap.BackBufferStride,
                 Bitmap.Palette?.Colors.Select(i => new BGRA(i.B, i.G, i.R, i.A))
                                       .ToList())
            => this.Bitmap = Bitmap;

        protected bool IsLocked { set; get; }
        public bool TryLock(int Timeout)
        {
            if (Bitmap is null)
                return false;

            if (Bitmap.Dispatcher.Invoke(() => Bitmap.TryLock(TimeSpan.FromMilliseconds(Timeout))))
            {
                IsLocked = true;
                return true;
            }
            return false;
        }
        public void Unlock()
        {
            if (Bitmap is null)
                return;

            if (IsLocked)
                Bitmap.Dispatcher.Invoke(() => Bitmap.Unlock());
        }

        public void AddDirtyRect(Int32Rect Rect)
        {
            if (Bitmap is null)
                return;

            if (IsLocked)
                Bitmap.Dispatcher.Invoke(() => Bitmap.AddDirtyRect(Rect));
        }
        public void AddDirtyRect(int X, int Y, int Width, int Height)
            => AddDirtyRect(new Int32Rect(X, Y, Width, Height));
        public void AddDirtyRect(Int32Point Point, int Width, int Height)
            => AddDirtyRect(new Int32Rect(Point.X, Point.Y, Width, Height));
        public void AddDirtyRect(int X, int Y, Int32Size Size)
            => AddDirtyRect(new Int32Rect(X, Y, Size.Width, Size.Height));
        public void AddDirtyRect(Int32Point Point, Int32Size Size)
            => AddDirtyRect(new Int32Rect(Point.X, Point.Y, Size.Width, Size.Height));

        public BitmapSource ToBitmapSource()
            => Bitmap;

    }
}
