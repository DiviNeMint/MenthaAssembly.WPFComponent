using MenthaAssembly.Media.Imaging;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MenthaAssembly.Views.Primitives
{
    /// <summary>
    /// AccessViolationException
    /// Site : https://blog.elmah.io/debugging-system-accessviolationexception/
    /// In most cases (at least in my experience), the AccessViolationException is thrown when calling C++ code through the use of DllImport.
    /// Solution :
    /// 1. Adding [HandleProcessCorruptedStateExceptions] to the Main method, does cause the catch blog to actually catch the exception. 
    /// 2. Set legacyCorruptedStateExceptionsPolicy to true in app/web.config:
    /// <runtime>
    /// <legacyCorruptedStateExceptionsPolicy enabled = "true" />
    /// </ runtime >
    /// </summary>
    public abstract class ImageViewerBase : Control
    {
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        protected static extern void SetMemory(IntPtr dst, int Color, int Length);

        public static readonly DependencyProperty DisplayImageProperty =
            DependencyProperty.RegisterAttached("DisplayImage", typeof(WriteableBitmap), typeof(ImageViewerBase), new PropertyMetadata(default,
                (d, e) =>
                {
                    if (d is ImageViewerBase This &&
                        e.NewValue is WriteableBitmap Bitmap)
                        This.DisplayContext = new BitmapContext(Bitmap);
                }));
        protected static void SetDisplayImage(DependencyObject obj, WriteableBitmap value)
            => obj.SetValue(DisplayImageProperty, value);
        internal protected static WriteableBitmap GetDisplayImage(DependencyObject obj)
            => (WriteableBitmap)obj.GetValue(DisplayImageProperty);

        internal protected virtual BitmapContext DisplayContext { set; get; }

        private IImageContext _SourceContext;
        internal protected virtual unsafe IImageContext SourceContext
        {
            get => _SourceContext;
            set
            {
                _SourceContext = value;

                if (value.Channels == 1)
                {
                    // BGR
                    if (value.StructType == typeof(BGR))
                        DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                        {
                            byte* SourceScan0 = (byte*)value.Scan0;

                            int XStep = FactorStep * sizeof(BGR);
                            Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                            {
                                BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                                int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7),
                                        YDataOffset = Y * value.Stride;

                                if (0 <= Y && Y < value.Height)
                                {
                                    int X = (((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep) * sizeof(BGR),
                                            MaxX = value.Stride << 7;

                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    {
                                        if (0 <= X && X < MaxX)
                                        {
                                            int XDataOffset = X >> 7;
                                            XDataOffset -= XDataOffset % 3;

                                            byte* SourceScan = SourceScan0 + YDataOffset + XDataOffset;
                                            Data->B = *SourceScan++;
                                            Data->G = *SourceScan++;
                                            Data->R = *SourceScan;
                                            Data->A = byte.MaxValue;
                                            Data++;
                                        }
                                        else
                                            *Data++ = new BGRA();

                                        X += XStep;
                                    }
                                }
                                else
                                {
                                    // Clear
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        *Data++ = new BGRA();
                                }
                            });
                        };

                    // BGRA
                    else if (value.StructType == typeof(BGRA))
                        DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                        {
                            Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                            {
                                BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                                int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7),
                                    YDataOffset = Y * value.Stride;

                                if (0 <= Y && Y < value.Height)
                                {
                                    int X = ((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep,
                                        MaxX = value.Width << 7;

                                    BGRA* SourceScan = (BGRA*)(value.Scan0 + Y * value.Stride);
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    {
                                        *Data++ = (0 <= X && X < MaxX) ?
                                                  *(SourceScan + (X >> 7)) :
                                                  new BGRA();

                                        X += FactorStep;
                                    }
                                }
                                else
                                {
                                    // Clear
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        *Data++ = new BGRA();
                                }
                            });
                        };

                    // RGB
                    else if (value.StructType == typeof(RGB))
                        DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                        {
                            byte* SourceScan0 = (byte*)value.Scan0;

                            int XStep = FactorStep * sizeof(RGB);
                            Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                            {
                                BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                                int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7),
                                    YDataOffset = Y * value.Stride;

                                if (0 <= Y && Y < value.Height)
                                {
                                    int X = (((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep) * sizeof(RGB),
                                        MaxX = value.Stride << 7;

                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    {
                                        if (0 <= X && X < MaxX)
                                        {
                                            int XDataOffset = X >> 7;
                                            XDataOffset -= XDataOffset % 3;

                                            byte* SourceScan = SourceScan0 + YDataOffset + XDataOffset;
                                            Data->A = byte.MaxValue;
                                            Data->R = *SourceScan++;
                                            Data->G = *SourceScan++;
                                            Data->B = *SourceScan;
                                            Data++;
                                        }
                                        else
                                            *Data++ = new BGRA();

                                        X += XStep;
                                    }
                                }
                                else
                                {
                                    // Clear
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        *Data++ = new BGRA();
                                }
                            });
                        };

                    // ARGB
                    else if (value.StructType == typeof(ARGB))
                        DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                        {
                            byte* SourceScan0 = (byte*)value.Scan0;

                            int XStep = FactorStep * sizeof(ARGB);
                            Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                            {
                                BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                                int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7),
                                    YDataOffset = Y * value.Stride;

                                if (0 <= Y && Y < value.Height)
                                {
                                    int X = (((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep) * sizeof(ARGB),
                                        MaxX = value.Stride << 7;

                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    {
                                        if (0 <= X && X < MaxX)
                                        {
                                            byte* SourceScan = SourceScan0 + YDataOffset + (X >> 7);
                                            Data->A = *SourceScan++;
                                            Data->R = *SourceScan++;
                                            Data->G = *SourceScan++;
                                            Data->B = *SourceScan;
                                            Data++;
                                        }
                                        else
                                            *Data++ = new BGRA();

                                        X += XStep;
                                    }
                                }
                                else
                                {
                                    // Clear
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        *Data++ = new BGRA();

                                }
                            });
                        };

                    // Gray8
                    else if (value.StructType == typeof(Gray8))
                        DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                        {
                            byte* SourceScan0 = (byte*)value.Scan0;

                            int XStep = FactorStep * sizeof(Gray8);
                            Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                            {
                                BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                                int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7),
                                    YDataOffset = Y * value.Stride;

                                if (0 <= Y && Y < value.Height)
                                {
                                    int X = (((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep) * sizeof(Gray8),
                                        MaxX = value.Stride << 7;

                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    {
                                        if (0 <= X && X < MaxX)
                                        {
                                            byte Gray = *(SourceScan0 + YDataOffset + (X >> 7));
                                            Data->A = byte.MaxValue;
                                            Data->R = Gray;
                                            Data->G = Gray;
                                            Data->B = Gray;
                                            Data++;
                                        }
                                        else
                                            *Data++ = new BGRA();

                                        X += XStep;
                                    }
                                }
                                else
                                {
                                    // Clear
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        *Data++ = new BGRA();
                                }
                            });
                        };

                    // Indexed8
                    else if (value.StructType == typeof(Indexed8))
                        DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                        {
                            byte* SourceScan0 = (byte*)value.Scan0;
                            BGRA[] Palette = value.Palette.Cast<BGRA>().ToArray();

                            int XStep = FactorStep * sizeof(Indexed8);
                            Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                            {
                                BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                                int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7),
                                    YDataOffset = Y * value.Stride;

                                if (0 <= Y && Y < value.Height)
                                {
                                    int X = (((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep) * sizeof(Indexed8),
                                        MaxX = value.Stride << 7;

                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    {
                                        *Data++ = (0 <= X && X < MaxX) ?
                                                  Palette[*(SourceScan0 + YDataOffset + (X >> 7)) >> 0] :
                                                  new BGRA();

                                        X += XStep;
                                    }
                                }
                                else
                                {
                                    // Clear
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        *Data++ = new BGRA();
                                }
                            });
                        };

                    // Indexed4
                    else if (value.StructType == typeof(Indexed4))
                        DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                        {
                            Indexed4* SourceScan0 = (Indexed4*)value.Scan0;
                            BGRA[] Palette = value.Palette.Cast<BGRA>().ToArray();

                            // int XStep = FactorStep * BitsPerPixel / 8;
                            int XStep = FactorStep >> 1;
                            Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                            {
                                BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                                int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7),
                                    YDataOffset = Y * value.Stride;

                                if (0 <= Y && Y < value.Height)
                                {
                                    int X = ((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep,
                                        MaxX = value.Width << 7;
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    {
                                        if (0 <= X && X < MaxX)
                                        {
                                            int XDataOffset = X >> 7,
                                                Index = XDataOffset & 0x01; // Index = XDataOffset % Indexed4.Length;
                                            *Data++ = Palette[(*(SourceScan0 + YDataOffset + (XDataOffset >> 1)))[Index]];
                                        }
                                        else
                                            *Data++ = new BGRA();

                                        X += FactorStep;
                                    }
                                }
                                else
                                {
                                    // Clear
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        *Data++ = new BGRA();
                                }
                            });
                        };

                    // Indexed1
                    else if (value.StructType == typeof(Indexed1))
                        DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                        {
                            Indexed1* SourceScan0 = (Indexed1*)value.Scan0;
                            BGRA[] Palette = value.Palette.Cast<BGRA>().ToArray();

                            int XStep = FactorStep >> 3;
                            Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                            {
                                BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                                int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7),
                                    YDataOffset = Y * value.Stride;

                                if (0 <= Y && Y < value.Height)
                                {
                                    int X = ((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep,
                                        MaxX = value.Width << 7;
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    {
                                        if (0 <= X && X < MaxX)
                                        {
                                            int XDataOffset = X >> 7,
                                                Index = XDataOffset & 0x07; // Index = XDataOffset % Indexed1.Length;
                                            *Data++ = Palette[(*(SourceScan0 + YDataOffset + (XDataOffset >> 3)))[Index]];
                                        }
                                        else
                                            *Data++ = new BGRA();

                                        X += FactorStep;
                                    }
                                }
                                else
                                {
                                    // Clear
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        *Data++ = new BGRA();
                                }
                            });
                        };

                    // Common
                    else
                        DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                            Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                            {
                                BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                                int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7);
                                if (0 <= Y && Y < value.Height)
                                {
                                    int X = ((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep,
                                        MaxX = value.Width << 7;

                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    {
                                        if (0 <= X && X < MaxX)
                                        {
                                            IPixel Pixel = value[X >> 7, Y];
                                            Data->A = Pixel.A;
                                            Data->R = Pixel.R;
                                            Data->G = Pixel.G;
                                            Data->B = Pixel.B;
                                            Data++;
                                        }
                                        else
                                            *Data++ = new BGRA();

                                        X += FactorStep;
                                    }
                                }
                                else
                                {
                                    // Clear
                                    for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        *Data++ = new BGRA();
                                }
                            });
                }
                else if (value.Channels == 3)
                    DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                    {
                        byte* SourceScanR = (byte*)value.ScanR,
                              SourceScanG = (byte*)value.ScanG,
                              SourceScanB = (byte*)value.ScanB;
                        Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                        {
                            BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                            int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7),
                                YDataOffset = Y * value.Stride;
                            if (0 <= Y && Y < value.Height)
                            {
                                int X = ((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep,
                                    MaxX = value.Stride << 7;

                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                {
                                    if (0 <= X && X < MaxX)
                                    {
                                        int DataOffset = YDataOffset + (X >> 7);
                                        Data->B = *(SourceScanB + DataOffset);
                                        Data->G = *(SourceScanG + DataOffset);
                                        Data->R = *(SourceScanR + DataOffset);
                                        Data->A = byte.MaxValue;
                                        Data++;
                                    }
                                    else
                                        *Data++ = new BGRA();

                                    X += FactorStep;
                                }
                            }
                            else
                            {
                                // Clear
                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    *Data++ = new BGRA();

                            }
                        });
                    };
                else if (value.Channels == 4)
                    DrawHandler = (FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2) =>
                    {
                        byte* SourceScanA = (byte*)value.ScanA,
                              SourceScanR = (byte*)value.ScanR,
                              SourceScanG = (byte*)value.ScanG,
                              SourceScanB = (byte*)value.ScanB;
                        Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                        {
                            BGRA* Data = (BGRA*)(DisplayContext.Scan0 + j * DisplayContext.Stride + ((DirtyRectX1 * DisplayContext.BitsPerPixel) >> 3));

                            int Y = Viewport.Y - SourceLocation.Y + ((j * FactorStep) >> 7),
                                YDataOffset = Y * value.Stride;
                            if (0 <= Y && Y < value.Height)
                            {
                                int X = ((Viewport.X - SourceLocation.X) << 7) + DirtyRectX1 * FactorStep,
                                    MaxX = value.Stride << 7;

                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                {
                                    if (0 <= X && X < MaxX)
                                    {
                                        int DataOffset = YDataOffset + (X >> 7);
                                        Data->B = *(SourceScanB + DataOffset);
                                        Data->G = *(SourceScanG + DataOffset);
                                        Data->R = *(SourceScanR + DataOffset);
                                        Data->A = *(SourceScanA + DataOffset);
                                        Data++;
                                    }
                                    else
                                        *Data++ = new BGRA();

                                    X += FactorStep;
                                }
                            }
                            else
                            {
                                // Clear
                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                    *Data++ = new BGRA();
                            }
                        });
                    };
            }
        }
        internal protected virtual Int32Point SourceLocation { set; get; }

        protected virtual Int32Size ViewBox { set; get; }

        protected virtual Int32Rect Viewport { set; get; }

        protected virtual double Scale { set; get; }

        protected Int32Rect LastImageRect;

        /// <summary>
        /// FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2
        /// </summary>
        protected Action<int, int, int, int, int> DrawHandler;
        protected unsafe virtual Int32Rect OnDraw()
        {
            if (SourceContext != null && Scale > 0)
            {
                Int32Point SourceEndPoint = new Int32Point(SourceLocation.X + SourceContext.Width,
                                                           SourceLocation.Y + SourceContext.Height);
                // Calculate Source's Rect in ImageViewer.
                int ImageX1 = Math.Max((int)((SourceLocation.X - Viewport.X) * Scale), 0),
                    ImageY1 = Math.Max((int)((SourceLocation.Y - Viewport.Y) * Scale), 0),
                    ImageX2 = Math.Min((int)((SourceEndPoint.X - Viewport.X) * Scale) + 1, DisplayContext.Width),
                    ImageY2 = Math.Min((int)((SourceEndPoint.Y - Viewport.Y) * Scale) + 1, DisplayContext.Height);

                // Calculate DirtyRect (Compare with LastImageRect)
                int DirtyRectX1 = Math.Min(LastImageRect.X, ImageX1),
                    DirtyRectX2 = Math.Max(LastImageRect.X + LastImageRect.Width, ImageX2),
                    DirtyRectY1 = Math.Min(LastImageRect.Y, ImageY1),
                    DirtyRectY2 = Math.Max(LastImageRect.Y + LastImageRect.Height, ImageY2);

                LastImageRect = new Int32Rect(ImageX1, ImageY1, ImageX2 - ImageX1, ImageY2 - ImageY1);
                Int32Rect Result = new Int32Rect(DirtyRectX1,
                                                 DirtyRectY1,
                                                 Math.Max(DirtyRectX2 - DirtyRectX1, 0),
                                                 Math.Max(DirtyRectY2 - DirtyRectY1, 0));

                int FactorStep = (int)(1 / Scale * 128);
                DrawHandler(FactorStep, DirtyRectX1, DirtyRectY1, DirtyRectX2, DirtyRectY2);

                return Result;
            }
            else
            {
                // Clear
                SetMemory(DisplayContext.Scan0, 0, DisplayContext.Stride * DisplayContext.Height);
            }

            return new Int32Rect(0, 0, DisplayContext.Width, DisplayContext.Height);
        }

    }
}
