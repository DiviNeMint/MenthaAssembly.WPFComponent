using MenthaAssembly.Media.Imaging;
using System;
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
    public unsafe abstract class ImageViewerBase : Control
    {
        private static readonly ParallelOptions DefaultParallelOptions = new ParallelOptions();

        public ParallelOptions RenderParallelOptions { set; get; }

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
        internal protected virtual IImageContext SourceContext
        {
            get => _SourceContext;
            set
            {
                _SourceContext = value;

                if (value.PixelType.Equals(typeof(BGRA)))
                    DrawHandler = (FactorStep, DirtyX1, DirtyY1, DirtyX2, DirtyY2) =>
                    {
                        byte* DisplayScan0 = (byte*)DisplayContext.Scan0;

                        int SourceW = value.Width,
                            SourceH = value.Height,
                            IntViewportX = (int)Viewport.X,
                            IntViewportY = (int)Viewport.Y,
                            ISx = IntViewportX - SourceLocation.X,
                            ISy = IntViewportY - SourceLocation.Y;
                        float FracX0 = (float)(Viewport.X - IntViewportX),
                              FracY0 = (float)(Viewport.Y - IntViewportY),
                              FracX1 = DirtyX2 * FactorStep + FracX0;

                        FracX0 += DirtyX1 * FactorStep;

                        int IntFracX0 = (int)FracX0,
                            IntFracY0 = (int)FracY0,
                            IntFracX1 = (int)FracX1,
                            Sx = ISx + IntFracX0,
                            Sy = ISy + IntFracY0,
                            Ex = ISx + IntFracX1;

                        FracX0 -= IntFracX0;
                        FracY0 -= IntFracY0;
                        FracX1 -= IntFracX1;

                        Parallel.For(DirtyY1, DirtyY2, RenderParallelOptions ?? DefaultParallelOptions, j =>
                        {
                            long Offset = j * DisplayContext.Stride + DirtyX1 * sizeof(BGRA);
                            BGRA* Data = (BGRA*)(DisplayScan0 + Offset);

                            int Y = Sy + (int)(j * FactorStep + FracY0);

                            if (0 <= Y && Y < SourceH)
                            {
                                int X = Sx;
                                float FracX = FracX0;

                                if (X < 0 && (X < Ex || (X == Ex && FracX < FracX1)))
                                {
                                    do
                                    {
                                        *Data++ = EmptyPixel;

                                        FracX += FactorStep;
                                        while (FracX >= 1f)
                                        {
                                            X++;
                                            FracX -= 1f;
                                        }

                                        if (X >= Ex)
                                            return;

                                    } while (X < 0);
                                }

                                if (X < Ex || (X == Ex && FracX < FracX1))
                                {
                                    byte* pTemp = (byte*)Data;
                                    value.Operator.ScanLineNearestResizeTo(ref FracX, FactorStep, ref X, Ex, FracX1, Y, ref pTemp);
                                    Data = (BGRA*)pTemp;
                                }

                                while (X < Ex || (X == Ex && FracX < FracX1))
                                {
                                    *Data++ = EmptyPixel;

                                    FracX += FactorStep;
                                    while (FracX >= 1f)
                                    {
                                        X++;
                                        FracX -= 1f;
                                    }
                                }
                            }
                            else
                            {
                                // Clear
                                for (int i = DirtyX1; i < DirtyX2; i++)
                                    *Data++ = EmptyPixel;
                            }
                        });
                    };
                else
                    DrawHandler = (FactorStep, DirtyX1, DirtyY1, DirtyX2, DirtyY2) =>
                    {
                        byte* DisplayScan0 = (byte*)DisplayContext.Scan0;

                        int SourceW = value.Width,
                            SourceH = value.Height,
                            IntViewportX = (int)Viewport.X,
                            IntViewportY = (int)Viewport.Y,
                            ISx = IntViewportX - SourceLocation.X,
                            ISy = IntViewportY - SourceLocation.Y;
                        float FracX0 = (float)(Viewport.X - IntViewportX),
                              FracY0 = (float)(Viewport.Y - IntViewportY),
                              FracX1 = DirtyX2 * FactorStep + FracX0;

                        FracX0 += DirtyX1 * FactorStep;

                        int IntFracX0 = (int)FracX0,
                            IntFracY0 = (int)FracY0,
                            IntFracX1 = (int)FracX1,
                            Sx = ISx + IntFracX0,
                            Sy = ISy + IntFracY0,
                            Ex = ISx + IntFracX1;

                        FracX0 -= IntFracX0;
                        FracY0 -= IntFracY0;
                        FracX1 -= IntFracX1;

                        Parallel.For(DirtyY1, DirtyY2, RenderParallelOptions ?? DefaultParallelOptions, j =>
                        {
                            long Offset = j * DisplayContext.Stride + DirtyX1 * sizeof(BGRA);
                            BGRA* Data = (BGRA*)(DisplayScan0 + Offset);

                            int Y = Sy + (int)(j * FactorStep + FracY0);

                            if (0 <= Y && Y < SourceH)
                            {
                                int X = Sx;
                                float FracX = FracX0;

                                if (X < 0 && (X < Ex || (X == Ex && FracX < FracX1)))
                                {
                                    do
                                    {
                                        *Data++ = EmptyPixel;

                                        FracX += FactorStep;
                                        while (FracX >= 1f)
                                        {
                                            X++;
                                            FracX -= 1f;
                                        }

                                        if (X >= Ex)
                                            return;

                                    } while (X < 0);
                                }

                                if (X < Ex || (X == Ex && FracX < FracX1))
                                    value.Operator.ScanLineNearestResizeTo(ref FracX, FactorStep, ref X, Ex, FracX1, Y, ref Data);

                                while (X < Ex || (X == Ex && FracX < FracX1))
                                {
                                    *Data++ = EmptyPixel;

                                    FracX += FactorStep;
                                    while (FracX >= 1f)
                                    {
                                        X++;
                                        FracX -= 1f;
                                    }
                                }
                            }
                            else
                            {
                                // Clear
                                for (int i = DirtyX1; i < DirtyX2; i++)
                                    *Data++ = EmptyPixel;
                            }
                        });
                    };
            }
        }
        internal protected virtual Point<int> SourceLocation { set; get; }

        protected virtual Size<int> ViewBox { set; get; }

        protected virtual Rect Viewport { set; get; }

        protected virtual double Scale { set; get; }

        protected FloatBound LastImageBound;

        protected static readonly BGRA EmptyPixel = new BGRA();
        /// <summary>
        /// FactorStep,IntDirtyX1, IntDirtyY1, IntDirtyX2, IntDirtyY2
        /// </summary>
        protected Action<float, int, int, int, int> DrawHandler;
        protected virtual Int32Rect OnDraw()
        {
            if (SourceContext != null &&
                SourceContext.Width > 0 && SourceContext.Height > 0 &&
                Scale > 0)
            {
                // Calculate Source's Rect in ImageViewer.
                float SourceEx = SourceLocation.X + SourceContext.Width,
                      SourceEy = SourceLocation.Y + SourceContext.Height,
                      ISx = Math.Max((float)((SourceLocation.X - Viewport.X) * Scale), 0f),
                      ISy = Math.Max((float)((SourceLocation.Y - Viewport.Y) * Scale), 0f),
                      IEx = Math.Min((float)((SourceEx - Viewport.X) * Scale), DisplayContext.Width),
                      IEy = Math.Min((float)((SourceEy - Viewport.Y) * Scale), DisplayContext.Height);

                // Calculate DirtyRect (Compare with LastImageRect)
                float DirtyX1 = Math.Min(LastImageBound.Left, ISx),
                      DirtyY1 = Math.Min(LastImageBound.Top, ISy),
                      DirtyX2 = Math.Max(LastImageBound.Right, IEx),
                      DirtyY2 = Math.Max(LastImageBound.Bottom, IEy);

                int IntDirtyX1 = (int)Math.Ceiling(DirtyX1),
                    IntDirtyY1 = (int)Math.Floor(DirtyY1),
                    IntDirtyX2 = (int)Math.Ceiling(DirtyX2),
                    IntDirtyY2 = (int)Math.Floor(DirtyY2);

                LastImageBound = new FloatBound(DirtyX1, DirtyY1, DirtyX2, DirtyY2);

                DrawHandler((float)(1 / Scale), IntDirtyX1, IntDirtyY1, IntDirtyX2, IntDirtyY2);

                return new Int32Rect(IntDirtyX1, IntDirtyY1, IntDirtyX2 - IntDirtyX1, IntDirtyY2 - IntDirtyY1);
            }
            else
            {
                // Clear
                DisplayContext.Clear(EmptyPixel, null);
            }

            return new Int32Rect(0, 0, DisplayContext.Width, DisplayContext.Height);
        }

    }
}
