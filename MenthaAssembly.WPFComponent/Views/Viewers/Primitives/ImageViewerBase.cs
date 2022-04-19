using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Utils;
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
    public abstract unsafe class ImageViewerBase : Control
    {
        private delegate void DrawAction(IPixelAdapter<BGRA> Adapter, BGRA* pDisplay);
        private static readonly ParallelOptions DefaultParallelOptions = new();

        public ParallelOptions RenderParallelOptions { set; get; }

        public static readonly DependencyProperty DisplayImageProperty =
            DependencyProperty.RegisterAttached("DisplayImage", typeof(WriteableBitmap), typeof(ImageViewerBase), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is ImageViewerBase This &&
                        e.NewValue is WriteableBitmap Bitmap)
                        This.DisplayContext = new BitmapContext(Bitmap);
                }));
        protected static void SetDisplayImage(ImageViewerBase obj, WriteableBitmap value)
            => obj.SetValue(DisplayImageProperty, value);
        protected internal static WriteableBitmap GetDisplayImage(ImageViewerBase obj)
            => (WriteableBitmap)obj.GetValue(DisplayImageProperty);

        public static readonly DependencyProperty ItemsLayerImageProperty =
            DependencyProperty.RegisterAttached("ItemsLayerImage", typeof(WriteableBitmap), typeof(ImageViewerBase), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is ImageViewerBase This &&
                        e.NewValue is WriteableBitmap Bitmap)
                        This.ItemsLayerContext = new BitmapContext(Bitmap);
                }));
        protected static void SetItemsLayerImage(ImageViewerBase obj, WriteableBitmap value)
            => obj.SetValue(ItemsLayerImageProperty, value);
        protected internal static WriteableBitmap GetItemsLayerImage(ImageViewerBase obj)
            => (WriteableBitmap)obj.GetValue(ItemsLayerImageProperty);

        protected internal virtual BitmapContext DisplayContext { set; get; }

        protected internal virtual BitmapContext ItemsLayerContext { set; get; }

        public virtual IImageContext SourceContext { set; get; }
        protected internal virtual Point<int> SourceLocation { set; get; }

        protected virtual Size<int> ViewBox { set; get; }

        protected virtual Rect Viewport { set; get; }

        protected virtual double Scale { set; get; }

        private ImageChannel _Channel;
        protected virtual ImageChannel Channel
        {
            get => _Channel;
            set
            {
                _Channel = value;
                DrawHandler = value switch
                {
                    ImageChannel.R => (Adapter, pDisplay) =>
                    {
                        byte R = Adapter.R;
                        pDisplay->A = byte.MaxValue;
                        pDisplay->R = R;
                        pDisplay->G = R;
                        pDisplay->B = R;
                    }
                    ,
                    ImageChannel.G => (Adapter, pDisplay) =>
                    {
                        byte G = Adapter.G;
                        pDisplay->A = byte.MaxValue;
                        pDisplay->R = G;
                        pDisplay->G = G;
                        pDisplay->B = G;
                    }
                    ,
                    ImageChannel.B => (Adapter, pDisplay) =>
                    {
                        byte B = Adapter.B;
                        pDisplay->A = byte.MaxValue;
                        pDisplay->R = B;
                        pDisplay->G = B;
                        pDisplay->B = B;
                    }
                    ,
                    _ => (Adapter, pDisplay) => Adapter.OverrideTo(pDisplay),
                };
            }
        }

        protected Bound<float> LastImageBound;

        protected static readonly BGRA EmptyPixel = new();
        private DrawAction DrawHandler = (Adapter, pDisplay) => Adapter.OverrideTo(pDisplay);
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

                LastImageBound = new Bound<float>(DirtyX1, DirtyY1, DirtyX2, DirtyY2);

                #region Draw
                float FactorStep = (float)(1 / Scale);
                byte* DisplayScan0 = (byte*)DisplayContext.Scan0;

                int SourceW = SourceContext.Width,
                    SourceH = SourceContext.Height,
                    IntViewportX = (int)Viewport.X,
                    IntViewportY = (int)Viewport.Y,
                    IntISx = IntViewportX - SourceLocation.X,
                    IntISy = IntViewportY - SourceLocation.Y;
                float FracX0 = (float)(Viewport.X - IntViewportX),
                      FracY0 = (float)(Viewport.Y - IntViewportY),
                      FracX1 = IntDirtyX2 * FactorStep + FracX0;

                FracX0 += IntDirtyX1 * FactorStep;

                int IntFracX0 = (int)FracX0,
                    IntFracY0 = (int)FracY0,
                    IntFracX1 = (int)FracX1,
                    Sx = IntISx + IntFracX0,
                    Sy = IntISy + IntFracY0,
                    Ex = IntISx + IntFracX1;

                FracX0 -= IntFracX0;
                FracY0 -= IntFracY0;
                FracX1 -= IntFracX1;

                Parallel.For(IntDirtyY1, IntDirtyY2, RenderParallelOptions ?? DefaultParallelOptions, j =>
                {
                    long Offset = j * DisplayContext.Stride + IntDirtyX1 * sizeof(BGRA);
                    BGRA* pData = (BGRA*)(DisplayScan0 + Offset);

                    int Y = Sy + (int)(j * FactorStep + FracY0);

                    if (0 <= Y && Y < SourceH)
                    {
                        int X = Sx;
                        float FracX = FracX0;

                        if (X < 0 && (X < Ex || (X == Ex && FracX < FracX1)))
                        {
                            do
                            {
                                *pData++ = EmptyPixel;

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
                            IPixelAdapter<BGRA> Adapter = SourceContext.Operator.GetAdapter<BGRA>(X, Y);

                            while (X < SourceW && (X < Ex || (X == Ex && FracX < FracX1)))
                            {
                                DrawHandler(Adapter, pData++);

                                FracX += FactorStep;
                                while (FracX >= 1f)
                                {
                                    FracX -= 1f;
                                    Adapter.MoveNext();
                                    X++;
                                }
                            }
                        }

                        while (X < Ex || (X == Ex && FracX < FracX1))
                        {
                            *pData++ = EmptyPixel;

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
                        for (int i = IntDirtyX1; i < IntDirtyX2; i++)
                            *pData++ = EmptyPixel;
                    }
                });

                #endregion

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
