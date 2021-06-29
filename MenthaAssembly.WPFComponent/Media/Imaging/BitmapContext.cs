using MenthaAssembly.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
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

        public long Stride => Context.Stride;

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

            if ((Bitmap.Dispatcher.CheckAccess() && Bitmap.TryLock(TimeSpan.FromMilliseconds(Timeout))) ||
                Bitmap.Dispatcher.Invoke(() => Bitmap.TryLock(TimeSpan.FromMilliseconds(Timeout))))
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
            {
                if (Bitmap.Dispatcher.CheckAccess())
                    Bitmap.Unlock();
                else
                    Bitmap.Dispatcher.Invoke(() => Bitmap.Unlock());
            }
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
        {
            return Context.Cast<T, U>();
        }

        public IImageContext ParallelCast<T>() where T : unmanaged, IPixel
        {
            return Context.ParallelCast<T>();
        }
        public IImageContext ParallelCast<T>(ParallelOptions Options) where T : unmanaged, IPixel
        {
            return Context.ParallelCast<T>(Options);
        }
        public IImageContext ParallelCast<T, U>()
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            return Context.ParallelCast<T, U>();
        }
        public IImageContext ParallelCast<T, U>(ParallelOptions Options)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            return Context.ParallelCast<T, U>(Options);
        }

        public IntPtr CreateHBitmap()
            => Context.CreateHBitmap();


        #region BlockCopy
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0)
            => Context.BlockCopy(X, Y, Width, Height, Dest0);
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride)
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride)
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0)
            => Context.BlockCopy(X, Y, Width, Height, Dest0);
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride)
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0)
            => Context.BlockCopy(X, Y, Width, Height, Dest0);
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);

        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);

        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestOffset, DestStride);
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);

        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestOffset, DestStride);
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);

        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestStride);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestStride);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestStride);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, Options);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride, Options);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, Options);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, Options);
        }
        public void ParallelBlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        }

        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        }

        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, DestOffset, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, Options);
        }
        public void ParallelBlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.ParallelBlockCopy<T>(X, Y, Width, Height, Dest0, DestStride, Options);
        }

        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestOffset, DestStride);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Options);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride, Options);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestOffset, DestStride, Options);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Options);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride, Options);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Options);
        }
        public void ParallelBlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride, Options);
        }

        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestOffset, DestStride);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Options);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride, Options);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestOffset, DestStride, Options);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Options);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride, Options);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Options);
        }
        public void ParallelBlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
        {
            Context.ParallelBlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride, Options);
        }

        #endregion

        #region ScanLineCopy
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] Dest0)
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset)
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0, DestOffset);
        public void ScanLineCopy(int OffsetX, int Y, int Length, IntPtr Dest0)
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0);
        public void ScanLineCopy(int OffsetX, int Y, int Length, byte* Dest0)
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0);

        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T* Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, T[] Dest0, int DestOffset) where T : unmanaged, IPixel
            => Context.ScanLineCopy(OffsetX, Y, Length, Dest0, DestOffset);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy<T>(OffsetX, Y, Length, Dest0);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte[] Dest0, int DestOffset) where T : unmanaged, IPixel
            => Context.ScanLineCopy<T>(OffsetX, Y, Length, Dest0, DestOffset);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, IntPtr Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy<T>(OffsetX, Y, Length, Dest0);
        public void ScanLineCopy<T>(int OffsetX, int Y, int Length, byte* Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy<T>(OffsetX, Y, Length, Dest0);

        public void ScanLineCopy3(int OffsetX, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.ScanLineCopy3(OffsetX, Y, Length, DestR, DestG, DestB);
        public void ScanLineCopy3(int OffsetX, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset)
            => Context.ScanLineCopy3(OffsetX, Y, Length, DestR, DestG, DestB, DestOffset);
        public void ScanLineCopy3(int OffsetX, int Y, int Length, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => Context.ScanLineCopy3(OffsetX, Y, Length, DestR, DestG, DestB);
        public void ScanLineCopy3(int OffsetX, int Y, int Length, byte* DestR, byte* DestG, byte* DestB)
            => Context.ScanLineCopy3(OffsetX, Y, Length, DestR, DestG, DestB);

        public void ScanLineCopy4(int OffsetX, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.ScanLineCopy4(OffsetX, Y, Length, DestA, DestR, DestG, DestB);
        public void ScanLineCopy4(int OffsetX, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset)
            => Context.ScanLineCopy4(OffsetX, Y, Length, DestA, DestR, DestG, DestB, DestOffset);
        public void ScanLineCopy4(int OffsetX, int Y, int Length, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => Context.ScanLineCopy4(OffsetX, Y, Length, DestA, DestR, DestG, DestB);
        public void ScanLineCopy4(int OffsetX, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => Context.ScanLineCopy4(OffsetX, Y, Length, DestA, DestR, DestG, DestB);

        #endregion

        #region BlockOverlayTo
        void IImageContext.BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
            => Context.BlockOverlayTo<T>(X, Y, Width, Height, Dest0, DestStride);
        void IImageContext.BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride)
            => Context.BlockOverlayTo<T>(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        void IImageContext.BlockOverlayTo<T>(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride)
            => Context.BlockOverlayTo<T>(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);

        #endregion

        public object Clone()
            => Context.Clone();

        public IImageContext ParallelCrop(int X, int Y, int Width, int Height)
        {
            return Context.ParallelCrop(X, Y, Width, Height);
        }
        public IImageContext ParallelCrop(int X, int Y, int Width, int Height, ParallelOptions Options)
        {
            return Context.ParallelCrop(X, Y, Width, Height, Options);
        }


        public void Clear(IPixel Color)
        {
            Context.Clear(Color);
        }
        public void ParallelClear(IPixel Color)
        {
            Context.ParallelClear(Color);
        }

        public void DrawLine(Int32Point P0, Int32Point P1, IPixel Color)
        {
            Context.DrawLine(P0, P1, Color);
        }
        public void DrawLine(int X0, int Y0, int X1, int Y1, IPixel Color)
        {
            Context.DrawLine(X0, Y0, X1, Y1, Color);
        }
        public void DrawLine(Int32Point P0, Int32Point P1, IImageContext Pen)
        {
            Context.DrawLine(P0, P1, Pen);
        }
        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen)
        {
            Context.DrawLine(X0, Y0, X1, Y1, Pen);
        }
        public void DrawLine(Int32Point P0, Int32Point P1, ImageContour Contour, IPixel Fill)
        {
            Context.DrawLine(P0, P1, Contour, Fill);
        }
        public void DrawLine(int X0, int Y0, int X1, int Y1, ImageContour Contour, IPixel Fill)
        {
            Context.DrawLine(X0, Y0, X1, Y1, Contour, Fill);
        }

        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, IPixel Color)
        {
            Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Color);
        }
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IPixel Color)
        {
            Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Color);
        }
        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, IImageContext Pen)
        {
            Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Pen);
        }
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen)
        {
            Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Pen);
        }
        public void DrawArc(Int32Point Start, Int32Point End, Int32Point Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, IPixel Fill)
        {
            Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Contour, Fill);
        }
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, IPixel Fill)
        {
            Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Contour, Fill);
        }

        public void DrawEllipse(Int32Bound Bound, IPixel Color)
        {
            Context.DrawEllipse(Bound, Color);
        }
        public void DrawEllipse(Int32Point Center, int Rx, int Ry, IPixel Color)
        {
            Context.DrawEllipse(Center, Rx, Ry, Color);
        }
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IPixel Color)
        {
            Context.DrawEllipse(Cx, Cy, Rx, Ry, Color);
        }
        public void DrawEllipse(Int32Bound Bound, IImageContext Pen)
        {
            Context.DrawEllipse(Bound, Pen);
        }
        public void DrawEllipse(Int32Point Center, int Rx, int Ry, IImageContext Pen)
        {
            Context.DrawEllipse(Center, Rx, Ry, Pen);
        }
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IImageContext Pen)
        {
            Context.DrawEllipse(Cx, Cy, Rx, Ry, Pen);
        }
        public void DrawEllipse(Int32Bound Bound, ImageContour Contour, IPixel Fill)
        {
            Context.DrawEllipse(Bound, Contour, Fill);
        }
        public void DrawEllipse(Int32Point Center, int Rx, int Ry, ImageContour Contour, IPixel Fill)
        {
            Context.DrawEllipse(Center, Rx, Ry, Contour, Fill);
        }
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, ImageContour Contour, IPixel Fill)
        {
            Context.DrawEllipse(Cx, Cy, Rx, Ry, Contour, Fill);
        }

        public void FillEllipse(Int32Bound Bound, IPixel Fill)
        {
            Context.FillEllipse(Bound, Fill);
        }
        public void FillEllipse(Int32Point Center, int Rx, int Ry, IPixel Fill)
        {
            Context.FillEllipse(Center, Rx, Ry, Fill);
        }
        public void FillEllipse(int Cx, int Cy, int Rx, int Ry, IPixel Fill)
        {
            Context.FillEllipse(Cx, Cy, Rx, Ry, Fill);
        }

        public void DrawRegularPolygon(Int32Point Center, double Radius, int VertexNum, IPixel Color, double StartAngle)
        {
            Context.DrawRegularPolygon(Center, Radius, VertexNum, Color, StartAngle);
        }
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IPixel Color, double StartAngle)
        {
            Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Color, StartAngle);
        }
        public void DrawRegularPolygon(Int32Point Center, double Radius, int VertexNum, IImageContext Pen, double StartAngle)
        {
            Context.DrawRegularPolygon(Center, Radius, VertexNum, Pen, StartAngle);
        }
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IImageContext Pen, double StartAngle)
        {
            Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Pen, StartAngle);
        }
        public void DrawRegularPolygon(Int32Point Center, double Radius, int VertexNum, ImageContour Contour, IPixel Fill, double StartAngle)
        {
            Context.DrawRegularPolygon(Center, Radius, VertexNum, Contour, Fill, StartAngle);
        }
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, ImageContour Contour, IPixel Fill, double StartAngle)
        {
            Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Contour, Fill, StartAngle);
        }

        public void FillPolygon(IList<Int32Point> Vertices, IPixel Fill, int OffsetX, int OffsetY)
        {
            Context.FillPolygon(Vertices, Fill, OffsetX, OffsetY);
        }
        public void FillPolygon(IList<int> VerticeDatas, IPixel Fill, int OffsetX, int OffsetY)
        {
            Context.FillPolygon(VerticeDatas, Fill, OffsetX, OffsetY);
        }

        public void DrawStamp(Int32Point Position, IImageContext Stamp)
        {
            Context.DrawStamp(Position, Stamp);
        }
        public void DrawStamp(int X, int Y, IImageContext Stamp)
        {
            Context.DrawStamp(X, Y, Stamp);
        }

        public void FillContour(ImageContour Contour, IPixel Fill, int OffsetX, int OffsetY)
        {
            Context.FillContour(Contour, Fill, OffsetX, OffsetY);
        }

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
