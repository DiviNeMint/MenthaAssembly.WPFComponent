using MenthaAssembly.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    public unsafe class BitmapContext : IImageContext
    {
        public WriteableBitmap Bitmap { get; }

        protected IImageContext Context { get; }

        public BitmapContext(string Path) : this(new Uri(Path)) { }
        public BitmapContext(Uri Path) : this(new BitmapImage(Path)) { }
        public BitmapContext(BitmapSource Source) : this(new WriteableBitmap(Source)) { }
        public BitmapContext(WriteableBitmap Bitmap)
        {
            this.Bitmap = Bitmap;
            if (PixelFormats.Bgr24.Equals(Bitmap.Format))
            {
                Context = new ImageContext<BGR>(Bitmap.PixelWidth,
                                                Bitmap.PixelHeight,
                                                Bitmap.BackBuffer,
                                                Bitmap.BackBufferStride,
                                                Bitmap.Palette?.Colors.Select(i => new BGR(i.B, i.G, i.R))
                                                                      .ToList());
            }
            else if (PixelFormats.Bgr32.Equals(Bitmap.Format))
            {
                Context = new ImageContext<BGRA>(Bitmap.PixelWidth,
                                                 Bitmap.PixelHeight,
                                                 Bitmap.BackBuffer,
                                                 Bitmap.BackBufferStride,
                                                 Bitmap.Palette?.Colors.Select(i => new BGRA(i.B, i.G, i.R, i.A))
                                                                       .ToList());
            }
            else if (PixelFormats.Bgra32.Equals(Bitmap.Format) ||
                     PixelFormats.Pbgra32.Equals(Bitmap.Format))
            {
                Context = new ImageContext<BGRA>(Bitmap.PixelWidth,
                                                 Bitmap.PixelHeight,
                                                 Bitmap.BackBuffer,
                                                 Bitmap.BackBufferStride,
                                                 Bitmap.Palette?.Colors.Select(i => new BGRA(i.B, i.G, i.R, i.A))
                                                                       .ToList());
            }
            else if (PixelFormats.Rgb24.Equals(Bitmap.Format))
            {
                Context = new ImageContext<RGB>(Bitmap.PixelWidth,
                                                Bitmap.PixelHeight,
                                                Bitmap.BackBuffer,
                                                Bitmap.BackBufferStride,
                                                Bitmap.Palette?.Colors.Select(i => new RGB(i.R, i.G, i.B))
                                                                      .ToList());
            }
            else if (PixelFormats.Gray8.Equals(Bitmap.Format))
            {
                Context = new ImageContext<Gray8>(Bitmap.PixelWidth,
                                                  Bitmap.PixelHeight,
                                                  Bitmap.BackBuffer,
                                                  Bitmap.BackBufferStride,
                                                  Bitmap.Palette?.Colors.Select(i => new Gray8(i.B, i.G, i.R))
                                                                        .ToList());
            }
        }

        public int Width => Context.Width;

        public int Height => Context.Height;

        public int Stride => Context.Stride;

        public int BitsPerPixel => Context.BitsPerPixel;

        public int Channels => Context.Channels;

        public Type PixelType => Context.PixelType;
        Type IImageContext.PixelType => Context.PixelType;

        Type IImageContext.StructType => Context.StructType;

        public IntPtr Scan0 => Context.Scan0;
        IntPtr IImageContext.ScanA => throw new NotImplementedException();
        IntPtr IImageContext.ScanR => throw new NotImplementedException();
        IntPtr IImageContext.ScanG => throw new NotImplementedException();
        IntPtr IImageContext.ScanB => throw new NotImplementedException();

        public IList<IPixel> Palette => Context.Palette;

        public IPixel this[int X, int Y]
        {
            get => Context[X, Y];
            set => Context[X, Y] = value;
        }

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

        public IImageContext Flip(FlipMode Mode)
            => Context.Flip(Mode);
        public IImageContext Crop(int X, int Y, int Width, int Height)
            => Context.Crop(X, Y, Width, Height);
        public IImageContext Convolute(ConvoluteKernel Kernel)
            => Context.Convolute(Kernel);
        public IImageContext Convolute(int[,] Kernel, int KernelFactorSum, int KernelOffsetSum)
            => Context.Convolute(Kernel, KernelFactorSum, KernelOffsetSum);
        public IImageContext Cast<T>() where T : unmanaged, IPixel
            => Context.Cast<T>();
        public IImageContext Cast<T, U>()
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
            => Context.Cast<T, U>();

        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0);
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0)
            => Context.BlockCopy(X, Y, Width, Height, Dest0);
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset)
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestOffset);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestOffset);
        public void BlockCopy(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB)
            => Context.BlockCopy(X, Y, Width, Height, DestR, DestG, DestB);
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.BlockCopy(X, Y, Width, Height, DestR, DestG, DestB);
        public void BlockCopy(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => Context.BlockCopy(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.BlockCopy(X, Y, Width, Height, DestA, DestR, DestG, DestB);

        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T* Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* Dest0)
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset)
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0, DestOffset);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset) where T : unmanaged, IPixel
            => Context.ScanLineCopy<T>(OffsetX, Y, Length, Dest0, DestOffset);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* DestR, byte* DestG, byte* DestB)
            => Context.ScanLineCopy(OffsetX, Y, Length, DestR, DestG, DestB);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.ScanLineCopy(OffsetX, Y, Length, DestR, DestG, DestB);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => Context.ScanLineCopy(OffsetX, Y, Length, DestA, DestR, DestG, DestB);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.ScanLineCopy(OffsetX, Y, Length, DestA, DestR, DestG, DestB);

        public void BlockPaste<T>(int X, int Y, int Width, int Height, T* Source) where T : unmanaged, IPixel
            => Context.BlockPaste(X, Y, Width, Height, Source);
        public void BlockPaste<T>(int X, int Y, int Width, int Height, T[] Source) where T : unmanaged, IPixel
            => Context.BlockPaste(X, Y, Width, Height, Source);
        public void BlockPaste<T>(int X, int Y, int Width, int Height, byte[] Source, int SourceOffset) where T : unmanaged, IPixel
            => Context.BlockPaste<T>(X, Y, Width, Height, Source, SourceOffset);
        public void BlockPaste(int X, int Y, int Width, int Height, byte* SourceR, byte* SourceG, byte* SourceB)
            => Context.BlockPaste(X, Y, Width, Height, SourceR, SourceG, SourceB);
        public void BlockPaste(int X, int Y, int Width, int Height, byte[] SourceR, byte[] SourceG, byte[] SourceB)
            => Context.BlockPaste(X, Y, Width, Height, SourceR, SourceG, SourceB);
        public void BlockPaste(int X, int Y, int Width, int Height, byte* SourceA, byte* SourceR, byte* SourceG, byte* SourceB)
            => Context.BlockPaste(X, Y, Width, Height, SourceA, SourceR, SourceG, SourceB);
        public void BlockPaste(int X, int Y, int Width, int Height, byte[] SourceA, byte[] SourceR, byte[] SourceG, byte[] SourceB)
            => Context.BlockPaste(X, Y, Width, Height, SourceA, SourceR, SourceG, SourceB);

        public void ScanLinePaste<T>(int OffsetX, int Y, int Length, T* Source) where T : unmanaged, IPixel
            => Context.ScanLinePaste(OffsetX, Y, Length, Source);
        public void ScanLinePaste<T>(int OffsetX, int Y, IEnumerable<T> Source) where T : unmanaged, IPixel
            => Context.ScanLinePaste(OffsetX, Y, Source);
        public void ScanLinePaste<T>(int OffsetX, int Y, int Length, byte[] Source, int SourceOffset) where T : unmanaged, IPixel
            => Context.ScanLinePaste<T>(OffsetX, Y, Length, Source, SourceOffset);
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte* SourceR, byte* SourceG, byte* SourceB)
            => Context.ScanLinePaste(OffsetX, Y, Length, SourceR, SourceG, SourceB);
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte[] SourceR, byte[] SourceG, byte[] SourceB)
            => Context.ScanLinePaste(OffsetX, Y, Length, SourceR, SourceG, SourceB);
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte* SourceA, byte* SourceR, byte* SourceG, byte* SourceB)
            => Context.ScanLinePaste(OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB);
        public void ScanLinePaste(int OffsetX, int Y, int Length, byte[] SourceA, byte[] SourceR, byte[] SourceG, byte[] SourceB)
            => Context.ScanLinePaste(OffsetX, Y, Length, SourceA, SourceR, SourceG, SourceB);

        public object Clone()
            => Context.Clone();

        public static implicit operator ImageSource(BitmapContext Target) => Target?.Bitmap;
        public static implicit operator BitmapContext(BitmapSource Target) => Target is null ? null : new BitmapContext(Target);
        public static implicit operator BitmapContext(WriteableBitmap Target) => Target is null ? null : new BitmapContext(Target);

    }

    ////public class BitmapContext : ImageContext<BGRA>
    ////{
    ////    protected WriteableBitmap Bitmap { get; }

    ////    public BitmapContext(WriteableBitmap Bitmap) :
    ////        base(Bitmap.PixelWidth,
    ////             Bitmap.PixelHeight,
    ////             Bitmap.BackBuffer,
    ////             Bitmap.BackBufferStride,
    ////             Bitmap.Palette?.Colors.Select(i => new BGRA(i.B, i.G, i.R, i.A))
    ////                                   .ToList())
    ////        => this.Bitmap = Bitmap;

    ////    protected bool IsLocked { set; get; }
    ////    public bool TryLock(int Timeout)
    ////    {
    ////        if (Bitmap is null)
    ////            return false;

    ////        if (Bitmap.Dispatcher.Invoke(() => Bitmap.TryLock(TimeSpan.FromMilliseconds(Timeout))))
    ////        {
    ////            IsLocked = true;
    ////            return true;
    ////        }
    ////        return false;
    ////    }
    ////    public void Unlock()
    ////    {
    ////        if (Bitmap is null)
    ////            return;

    ////        if (IsLocked)
    ////            Bitmap.Dispatcher.Invoke(() => Bitmap.Unlock());
    ////    }

    ////    public void AddDirtyRect(Int32Rect Rect)
    ////    {
    ////        if (Bitmap is null)
    ////            return;

    ////        if (IsLocked)
    ////            Bitmap.Dispatcher.Invoke(() => Bitmap.AddDirtyRect(Rect));
    ////    }
    ////    public void AddDirtyRect(int X, int Y, int Width, int Height)
    ////        => AddDirtyRect(new Int32Rect(X, Y, Width, Height));
    ////    public void AddDirtyRect(Int32Point Point, int Width, int Height)
    ////        => AddDirtyRect(new Int32Rect(Point.X, Point.Y, Width, Height));
    ////    public void AddDirtyRect(int X, int Y, Int32Size Size)
    ////        => AddDirtyRect(new Int32Rect(X, Y, Size.Width, Size.Height));
    ////    public void AddDirtyRect(Int32Point Point, Int32Size Size)
    ////        => AddDirtyRect(new Int32Rect(Point.X, Point.Y, Size.Width, Size.Height));

    ////    public BitmapSource ToBitmapSource()
    ////        => Bitmap;

    ////}
}
