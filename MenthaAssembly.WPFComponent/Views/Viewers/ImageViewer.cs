using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MenthaAssembly.Media.Imaging;

namespace MenthaAssembly.Views
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
    public class ImageViewer : Control
    {
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern void SetMemory(IntPtr dst, int Color, int Length);

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageViewer));
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public event EventHandler<ChangedEventArgs<ImageContext>> SourceChanged;

        public event EventHandler<ChangedEventArgs<Int32Size>> ViewBoxChanged;

        public event EventHandler<ChangedEventArgs<Int32Rect>> ViewportChanged;

        public static readonly DependencyProperty SourceProperty =
              DependencyProperty.Register("Source", typeof(BitmapSource), typeof(ImageViewer), new PropertyMetadata(default,
                  (d, e) =>
                  {
                      if (d is ImageViewer This &&
                          e.NewValue is BitmapSource Image)
                          This.SourceContext = Image.ToImageContext();
                  }));
        public BitmapSource Source
        {
            get => (BitmapSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty DisplayImageProperty =
            DependencyProperty.RegisterAttached("DisplayImage", typeof(WriteableBitmap), typeof(ImageViewer), new PropertyMetadata(default,
                (d, e) =>
                {
                    if (d is ImageViewer This &&
                        e.NewValue is WriteableBitmap Bitmap)
                        This.DisplayContext = new BitmapContext(Bitmap);
                }));
        protected static void SetDisplayImage(DependencyObject obj, WriteableBitmap value)
            => obj.SetValue(DisplayImageProperty, value);
        protected static WriteableBitmap GetDisplayImage(DependencyObject obj)
            => (WriteableBitmap)obj.GetValue(DisplayImageProperty);

        public static readonly DependencyProperty FactorProperty =
              DependencyProperty.Register("Factor", typeof(double), typeof(ImageViewer), new PropertyMetadata(-1d,
                  (d, e) =>
                  {
                      if (d is ImageViewer This)
                          This.OnFactorChanged();
                  }));
        public double Factor
        {
            get => (double)GetValue(FactorProperty);
            set => SetValue(FactorProperty, value);
        }

        public static readonly DependencyProperty ZoomRatioProperty =
              DependencyProperty.Register("ZoomRatio", typeof(double), typeof(ImageViewer), new PropertyMetadata(2d));
        public double ZoomRatio
        {
            get => (double)GetValue(ZoomRatioProperty);
            set => SetValue(ZoomRatioProperty, value);
        }

        protected double MinFactor { set; get; }

        public static readonly DependencyProperty ViewportProperty =
              DependencyProperty.Register("Viewport", typeof(Int32Rect), typeof(ImageViewer), new PropertyMetadata(Int32Rect.Empty,
                  (d, e) =>
                  {
                      if (d is ImageViewer This)
                          This.OnViewportChanged(new ChangedEventArgs<Int32Rect>((Int32Rect)e.OldValue, (Int32Rect)e.NewValue));
                  }));
        public Int32Rect Viewport
        {
            get => (Int32Rect)GetValue(ViewportProperty);
            set => SetValue(ViewportProperty, value);
        }

        private Int32Size _ViewBox;
        protected Int32Size ViewBox
        {
            get => _ViewBox;
            set
            {
                ChangedEventArgs<Int32Size> e = new ChangedEventArgs<Int32Size>(_ViewBox, value);
                _ViewBox = value;
                OnViewBoxChanged(e);
            }
        }

        protected BitmapContext DisplayContext { set; get; }

        protected Int32Point SourceLocation { set; get; }
        private ImageContext _SourceContext;
        protected ImageContext SourceContext
        {
            get => _SourceContext;
            set
            {
                ChangedEventArgs<ImageContext> e = new ChangedEventArgs<ImageContext>(_SourceContext, value);
                _SourceContext = value;
                this.Dispatcher.Invoke(() => OnSourceChanged(e));
            }
        }
        protected Image PART_Image;

        static ImageViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewer), new FrameworkPropertyMetadata(typeof(ImageViewer)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //if (this.GetTemplateChild("PART_Image") is Image PART_Image)
            //{
            //    this.PART_Image = PART_Image;
            //    PART_Image.Source = DisplayImage;
            //}
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (ActualHeight > 0 && ActualWidth > 0)
                ViewBox = CalculateViewBox();
        }

        protected virtual void OnSourceChanged(ChangedEventArgs<ImageContext> e)
        {
            if (e != null)
                SourceChanged?.Invoke(this, e);

            Int32Size ViewBox = CalculateViewBox();
            if (Int32Size.Empty.Equals(ViewBox))
                return;

            if (ViewBox.Equals(this.ViewBox))
            {
                OnViewBoxChanged(null);
                return;
            }
            this.ViewBox = ViewBox;
        }

        protected virtual void OnViewBoxChanged(ChangedEventArgs<Int32Size> e)
        {
            if (e != null)
                ViewBoxChanged?.Invoke(this, e);

            SourceLocation = (SourceContext is null || SourceContext.Width > ViewBox.Width) ?
                             new Int32Point() :
                             new Int32Point((ViewBox.Width - SourceContext.Width) / 2,
                                            (ViewBox.Height - SourceContext.Height) / 2);

            double Factor = CalculateFactor();
            if (Factor.Equals(this.Factor))
            {
                OnFactorChanged();
                return;
            }
            this.Factor = Factor;
        }

        protected bool IsMinFactor = true;
        protected virtual void OnFactorChanged()
        {
            IsMinFactor = MinFactor.Equals(Factor);
            Int32Rect Viewport = CalculateViewport();
            if (Viewport.Equals(this.Viewport))
            {
                OnViewportChanged(null);
                return;
            }
            this.Viewport = Viewport;
        }

        protected virtual void OnViewportChanged(ChangedEventArgs<Int32Rect> e)
        {
            if (e != null)
                ViewportChanged?.Invoke(this, e);

            OnRenderImage();
        }

        protected Int32Size CalculateViewBox()
        {
            Size DisplayArea = GetDisplayArea();
            if (SourceContext is null || DisplayArea.Height <= 0 || DisplayArea.Width <= 0)
                return Int32Size.Empty;

            double Ratio = 1;
            double Scale = Math.Max(SourceContext.Width / DisplayArea.Width, SourceContext.Height / DisplayArea.Height);

            while (Ratio < Scale)
                Ratio *= ZoomRatio;

            MinFactor = 1d / Ratio;

            return new Int32Size(DisplayArea.Width * Ratio, DisplayArea.Height * Ratio);
        }

        protected double CalculateFactor()
        {
            if (IsMinFactor || Factor.Equals(-1d))
                return MinFactor;

            return Math.Max(MinFactor, Factor);
        }

        private bool IsZoomWithMouse;
        private Point Zoom_MousePosition;
        private Vector Zoom_MouseMoveDelta;
        protected Int32Rect CalculateViewport()
        {
            Int32Size ViewportHalfSize = ViewBox * (MinFactor / Factor / 2);
            Int32Point C0 = IsZoomWithMouse ? new Int32Point(Zoom_MousePosition.X - Zoom_MouseMoveDelta.X / Factor, Zoom_MousePosition.Y - Zoom_MouseMoveDelta.Y / Factor) :
                                              new Int32Point(Viewport.X + Viewport.Width / 2, Viewport.Y + Viewport.Height / 2);

            Int32Rect Result = new Int32Rect(C0.X - ViewportHalfSize.Width,
                                             C0.Y - ViewportHalfSize.Height,
                                             ViewportHalfSize.Width * 2,
                                             ViewportHalfSize.Height * 2);
            Result.X = Math.Min(Math.Max(0, Result.X), ViewBox.Width - Result.Width);
            Result.Y = Math.Min(Math.Max(0, Result.Y), ViewBox.Height - Result.Height);

            return Result;
        }

        private CancellationTokenSource OnDrawTokenSource;
        protected void OnRenderImage()
        {
            Size DisplayArea = GetDisplayArea();
            if (DisplayArea.Width is 0 || ActualHeight is 0)
                return;

            if (DisplayContext is null ||
                DisplayContext.Width != (int)DisplayArea.Width ||
                DisplayContext.Height != (int)DisplayArea.Height)
            {
                SetDisplayImage(this, new WriteableBitmap((int)DisplayArea.Width, (int)DisplayArea.Height, 96, 96, PixelFormats.Bgra32, null));
                LastImageRect = new Int32Rect(0, 0, DisplayContext.Width, DisplayContext.Height);
            }

            OnDrawTokenSource?.Cancel();
            OnDrawTokenSource?.Dispose();
            OnDrawTokenSource = new CancellationTokenSource();

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                BitmapContext Display = DisplayContext;
                if (Display.TryLock(1))
                {
                    try
                    {
                        Int32Rect DirtyRect = OnDraw(Factor, Viewport, OnDrawTokenSource.Token);
                        Display.AddDirtyRect(DirtyRect);
                    }
                    catch { }
                    finally
                    {
                        Display.Unlock();
                    }
                }
            }));
        }

        private Int32Rect LastImageRect;
        protected Int32Rect OnDraw(double Factor, Int32Rect Viewport, CancellationToken Token)
        {
            unsafe
            {
                if (SourceContext != null && Factor > 0)
                {
                    Int32Point SourceEndPoint = new Int32Point(SourceLocation.X + SourceContext.Width,
                                                               SourceLocation.Y + SourceContext.Height);
                    // Calculate Source's Rect in ImageViewer.
                    int ImageX1 = Math.Max((int)((SourceLocation.X - Viewport.X) * Factor), 0),
                        ImageY1 = Math.Max((int)((SourceLocation.Y - Viewport.Y) * Factor), 0),
                        ImageX2 = Math.Min((int)((SourceEndPoint.X - Viewport.X) * Factor) + 1, DisplayContext.Width),
                        ImageY2 = Math.Min((int)((SourceEndPoint.Y - Viewport.Y) * Factor) + 1, DisplayContext.Height);

                    // Calculate DirtyRect (Compare with LastImageRect)
                    int DirtyRectX1 = Math.Min(LastImageRect.X, ImageX1),
                        DirtyRectX2 = Math.Max(LastImageRect.X + LastImageRect.Width, ImageX2),
                        DirtyRectY1 = Math.Min(LastImageRect.Y, ImageY1),
                        DirtyRectY2 = Math.Max(LastImageRect.Y + LastImageRect.Height, ImageY2);

                    LastImageRect = new Int32Rect(ImageX1, ImageY1, ImageX2 - ImageX1, ImageY2 - ImageY1);


                    int* DisplayContextScan0 = (int*)DisplayContext.Scan0;
                    int DisplayStrideWidth = DisplayContext.Stride / ((DisplayContext.BitsPerPixel + 7) >> 3);
                    double FactorStep = 1 / Factor;

                    switch (SourceContext.Channels)
                    {
                        case 1:
                            switch (SourceContext.BitsPerPixel)
                            {
                                case 1:
                                    {
                                        byte* SourceContextScan0 = (byte*)SourceContext.Scan0;
                                        Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                                        {
                                            int* Data = DisplayContextScan0 + j * DisplayStrideWidth + DirtyRectX1;

                                            double Y = Viewport.Y + j * FactorStep;
                                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                            {
                                                double X = Viewport.X + DirtyRectX1 * FactorStep;
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                {
                                                    if (Token.IsCancellationRequested)
                                                        break;

                                                    if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                                    {
                                                        int SourceX = (int)X - SourceLocation.X;
                                                        int Offset = SourceX >> 3;
                                                        int Shift = 7 - (SourceX - (Offset << 3));

                                                        byte SourceContextBuffer = *(SourceContextScan0 +
                                                                                    ((int)Y - SourceLocation.Y) * SourceContext.Stride +
                                                                                    Offset);
                                                        int PaletteIndex = (SourceContextBuffer >> Shift) & 0x01;
                                                        if (SourceContext.Palette is null)
                                                        {
                                                            int Value = PaletteIndex * 255;
                                                            *Data++ = SourceContext.Palette?[PaletteIndex] ??
                                                                      0xFF << 24 |      // A
                                                                      Value << 16 |     // R
                                                                      Value << 8 |      // G
                                                                      Value;            // B
                                                        }
                                                        else
                                                            *Data++ = SourceContext.Palette[PaletteIndex];

                                                    }
                                                    else
                                                    {
                                                        // Clear
                                                        *Data++ = 0;
                                                    }
                                                    X += FactorStep;
                                                }
                                            }
                                            else
                                            {
                                                // Clear
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                    *Data++ = 0;
                                            }
                                        });
                                    }
                                    break;
                                case 4:
                                    {
                                        byte* SourceContextScan0 = (byte*)SourceContext.Scan0;
                                        Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                                        {
                                            int* Data = DisplayContextScan0 + j * DisplayStrideWidth + DirtyRectX1;

                                            double Y = Viewport.Y + j * FactorStep;
                                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                            {
                                                double X = Viewport.X + DirtyRectX1 * FactorStep;
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                {
                                                    if (Token.IsCancellationRequested)
                                                        break;

                                                    if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                                    {
                                                        int SourceX = (int)X - SourceLocation.X;
                                                        int Offset = SourceX >> 1;
                                                        int Shift = 1 - (SourceX - (Offset << 1));

                                                        byte SourceContextBuffer = *(SourceContextScan0 +
                                                                                    ((int)Y - SourceLocation.Y) * SourceContext.Stride +
                                                                                    Offset);
                                                        int PaletteIndex = (SourceContextBuffer >> (Shift << 2)) & 0x0F;
                                                        if (SourceContext.Palette is null)
                                                        {
                                                            int Value = PaletteIndex * 17;
                                                            *Data++ = 0xFF << 24 |      // A
                                                                      Value << 16 |     // R
                                                                      Value << 8 |      // G
                                                                      Value;            // B
                                                        }
                                                        else
                                                            *Data++ = SourceContext.Palette[PaletteIndex];
                                                    }
                                                    else
                                                    {
                                                        // Clear
                                                        *Data++ = 0;
                                                    }
                                                    X += FactorStep;
                                                }
                                            }
                                            else
                                            {
                                                // Clear
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                    *Data++ = 0;
                                            }
                                        });
                                    }
                                    break;
                                case 8:
                                    {
                                        byte* SourceContextScan0 = (byte*)SourceContext.Scan0;
                                        Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                                        {
                                            int* Data = DisplayContextScan0 + j * DisplayStrideWidth + DirtyRectX1;

                                            double Y = Viewport.Y + j * FactorStep;
                                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                            {
                                                double X = Viewport.X + DirtyRectX1 * FactorStep;
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                {
                                                    if (Token.IsCancellationRequested)
                                                        break;

                                                    if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                                    {
                                                        byte SourceContextBuffer = *(SourceContextScan0 +
                                                                                    ((int)Y - SourceLocation.Y) * SourceContext.Stride +
                                                                                    ((int)X - SourceLocation.X));

                                                        *Data++ = SourceContext.Palette?[SourceContextBuffer] ??
                                                                  0xFF << 24 |                 // A
                                                                  SourceContextBuffer << 16 |  // R
                                                                  SourceContextBuffer << 8 |   // G
                                                                  SourceContextBuffer;         // B
                                                    }
                                                    else
                                                    {
                                                        // Clear
                                                        *Data++ = 0;
                                                    }
                                                    X += FactorStep;
                                                }
                                            }
                                            else
                                            {
                                                // Clear
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                    *Data++ = 0;
                                            }
                                        });
                                    }
                                    break;
                                case 24:
                                    {
                                        byte* SourceContextScan0 = (byte*)SourceContext.Scan0;
                                        Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                                        {
                                            int* Data = DisplayContextScan0 + j * DisplayStrideWidth + DirtyRectX1;

                                            double Y = Viewport.Y + j * FactorStep;
                                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                            {
                                                double X = Viewport.X + DirtyRectX1 * FactorStep;
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                {
                                                    if (Token.IsCancellationRequested)
                                                        break;

                                                    if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                                    {
                                                        byte* SourceContextBuffer = SourceContextScan0 +
                                                                                    ((int)Y - SourceLocation.Y) * SourceContext.Stride +
                                                                                    ((int)X - SourceLocation.X) * 3;
                                                        *Data++ = 0xFF << 24 |                  // A
                                                                  *SourceContextBuffer++ |      // B
                                                                  *SourceContextBuffer++ << 8 | // G
                                                                  *SourceContextBuffer << 16;   // R
                                                    }
                                                    else
                                                    {
                                                        // Clear
                                                        *Data++ = 0;
                                                    }
                                                    X += FactorStep;
                                                }
                                            }
                                            else
                                            {
                                                // Clear
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                    *Data++ = 0;
                                            }
                                        });
                                    }
                                    break;
                                case 32:
                                    {
                                        int* SourceContextScan0 = (int*)SourceContext.Scan0;
                                        int SourceStrideWidth = SourceContext.Stride / 4;
                                        Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                                        {
                                            int* Data = DisplayContextScan0 + j * DisplayStrideWidth + DirtyRectX1;

                                            double Y = Viewport.Y + j * FactorStep;
                                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                            {
                                                double X = Viewport.X + DirtyRectX1 * FactorStep;
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                {
                                                    if (Token.IsCancellationRequested)
                                                        break;

                                                    if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                                    {
                                                        int* SourceContextBuffer = SourceContextScan0 +
                                                                                   ((int)Y - SourceLocation.Y) * SourceStrideWidth +
                                                                                   ((int)X - SourceLocation.X);
                                                        *Data++ = *SourceContextBuffer;
                                                    }
                                                    else
                                                    {
                                                        // Clear
                                                        *Data++ = 0;
                                                    }
                                                    X += FactorStep;
                                                }
                                            }
                                            else
                                            {
                                                // Clear
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                    *Data++ = 0;
                                            }
                                        });
                                    }
                                    break;
                            }
                            break;
                        case 3:
                            {
                                byte* SourceContextScanR = (byte*)SourceContext.ScanR,
                                      SourceContextScanG = (byte*)SourceContext.ScanG,
                                      SourceContextScanB = (byte*)SourceContext.ScanB;

                                Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                                {
                                    int* Data = DisplayContextScan0 + j * DisplayStrideWidth + DirtyRectX1;

                                    double Y = Viewport.Y + j * FactorStep;
                                    if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                    {
                                        double X = Viewport.X + DirtyRectX1 * FactorStep;
                                        for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        {
                                            if (Token.IsCancellationRequested)
                                                break;

                                            if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                            {
                                                int Step = ((int)Y - SourceLocation.Y) * SourceContext.Stride + (int)X - SourceLocation.X;
                                                *Data++ = 0xFF << 24 |                          // A
                                                          *(SourceContextScanR + Step) << 16 |  // R
                                                          *(SourceContextScanG + Step) << 8 |   // G
                                                          *(SourceContextScanB + Step);         // B
                                            }
                                            else
                                            {
                                                // Clear
                                                *Data++ = 0;
                                            }
                                            X += FactorStep;
                                        }
                                    }
                                    else
                                    {
                                        // Clear
                                        for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                            *Data++ = 0;
                                    }
                                });
                            }
                            break;
                        case 4:
                            {
                                byte* SourceContextScanA = (byte*)SourceContext.ScanA,
                                      SourceContextScanR = (byte*)SourceContext.ScanR,
                                      SourceContextScanG = (byte*)SourceContext.ScanG,
                                      SourceContextScanB = (byte*)SourceContext.ScanB;

                                Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                                {
                                    int* Data = DisplayContextScan0 + j * DisplayStrideWidth + DirtyRectX1;

                                    double Y = Viewport.Y + j * FactorStep;
                                    if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                    {
                                        double X = Viewport.X + DirtyRectX1 * FactorStep;
                                        for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        {
                                            if (Token.IsCancellationRequested)
                                                break;

                                            if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                            {
                                                int Step = ((int)Y - SourceLocation.Y) * SourceContext.Stride + (int)X - SourceLocation.X;
                                                *Data++ = *(SourceContextScanA + Step) << 24 |  // A
                                                          *(SourceContextScanR + Step) << 16 |  // R
                                                          *(SourceContextScanG + Step) << 8 |   // G
                                                          *(SourceContextScanB + Step);         // B
                                            }
                                            else
                                            {
                                                // Clear
                                                *Data++ = 0;
                                            }
                                            X += FactorStep;
                                        }
                                    }
                                    else
                                    {
                                        // Clear
                                        for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                            *Data++ = 0;
                                    }
                                });
                            }
                            break;
                    }
                    return new Int32Rect(DirtyRectX1,
                                           DirtyRectY1,
                                           Math.Max(DirtyRectX2 - DirtyRectX1, 0),
                                           Math.Max(DirtyRectY2 - DirtyRectY1, 0));
                }
                else
                {
                    // Clear
                    SetMemory(DisplayContext.Scan0, 0, DisplayContext.Stride * DisplayContext.Height);
                }
            }
            return new Int32Rect(0, 0, DisplayContext.Width, DisplayContext.Height);
        }

        private bool IsLeftMouseDown;
        private Point MousePosition;
        private Vector MouseMoveDelta;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.CaptureMouse();
                IsLeftMouseDown = true;
                MousePosition = e.GetPosition(this);
                MouseMoveDelta = new Vector();
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                Point Position = e.GetPosition(this);

                Vector MoveDelta = new Vector(Position.X - MousePosition.X, Position.Y - MousePosition.Y);
                MouseMoveDelta += MoveDelta;

                Int32Rect TempViewport = Viewport - new Int32Vector(MoveDelta / Factor);
                TempViewport.X = Math.Min(Math.Max(0, TempViewport.X), ViewBox.Width - TempViewport.Width);
                TempViewport.Y = Math.Min(Math.Max(0, TempViewport.Y), ViewBox.Height - TempViewport.Height);
                Viewport = TempViewport;

                MousePosition = Position;
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                this.ReleaseMouseCapture();
                IsLeftMouseDown = false;
                if (MouseMoveDelta.LengthSquared <= 25)
                    OnClick(new RoutedEventArgs(ClickEvent, this));
            }
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Point Position = e.GetPosition(this);
            Size DisplayArea = GetDisplayArea();
            Zoom(e.Delta > 0,
                 new Point(Viewport.X + Position.X / Factor, Viewport.Y + Position.Y / Factor),
                 new Vector(Position.X - DisplayArea.Width / 2, Position.Y - DisplayArea.Height / 2));
        }

        protected virtual void OnClick(RoutedEventArgs e)
            => RaiseEvent(e);


        public BitmapContext GetDisplayContext()
            => DisplayContext;
        public Size GetDisplayArea()
            => new Size(this.ActualWidth - (this.BorderThickness.Left + this.BorderThickness.Right),
                        this.ActualHeight - (this.BorderThickness.Top + this.BorderThickness.Bottom));

        public ImageContext GetSourceContext()
            => SourceContext;
        public void SetSourceContext(string ImagePath)
            => SourceContext = new BitmapImage(new Uri(ImagePath)).ToImageContext();
        public void SetSourceContext(Uri ImageUri)
            => SourceContext = new BitmapImage(ImageUri).ToImageContext();
        public void SetSourceContext(ImageContext Source)
            => SourceContext = Source;
        public void SetSourceContext(int Width, int Height, IntPtr Scan0, int Stride, int PixelBytes)
            => SourceContext = new ImageContext(Width, Height, Scan0, Stride, PixelBytes);
        public void SetSourceContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB)
            => SourceContext = new ImageContext(Width, Height, ScanR, ScanG, ScanB);
        public void SetSourceContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride)
            => SourceContext = new ImageContext(Width, Height, ScanR, ScanG, ScanB, Stride);
        public void SetSourceContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB)
            => SourceContext = new ImageContext(Width, Height, ScanA, ScanR, ScanG, ScanB);
        public void SetSourceContext(int Width, int Height, IntPtr ScanA, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride)
            => SourceContext = new ImageContext(Width, Height, ScanA, ScanR, ScanG, ScanB, Stride);

        public void Zoom(bool ZoomIn)
        {
            if (ZoomIn)
            {
                if (Factor < 1)
                    Factor = Math.Min(1, Factor * ZoomRatio);
            }
            else
            {
                if (Factor > MinFactor)
                    Factor = Math.Max(MinFactor, Factor / ZoomRatio);
            }
        }
        public void Zoom(bool ZoomIn, Point Zoom_MousePosition, Vector Zoom_MouseMoveDelta)
        {
            if (ZoomIn)
            {
                if (Factor < 1)
                {
                    IsZoomWithMouse = true;
                    this.Zoom_MousePosition = Zoom_MousePosition;
                    this.Zoom_MouseMoveDelta = Zoom_MouseMoveDelta;
                    Factor = Math.Min(1, Factor * ZoomRatio);
                    IsZoomWithMouse = false;
                }
            }
            else
            {
                if (Factor > MinFactor)
                {
                    IsZoomWithMouse = true;
                    this.Zoom_MousePosition = Zoom_MousePosition;
                    this.Zoom_MouseMoveDelta = Zoom_MouseMoveDelta;
                    Factor = Math.Max(MinFactor, Factor / ZoomRatio);
                    IsZoomWithMouse = false;
                }
            }
        }

        /// <summary>
        /// Move Viewport To Point(X, Y) at SourceImage.
        /// </summary>
        public void MoveTo(Int32Point Position)
            => MoveTo(Position.X, Position.Y);
        /// <summary>
        /// Move Viewport To Point(X, Y) at SourceImage.
        /// </summary>
        public void MoveTo(int X, int Y)
        {
            Int32Rect TempViewport = new Int32Rect(SourceLocation.X + X - Viewport.Width / 2,
                                                   SourceLocation.Y + Y - Viewport.Height / 2,
                                                   Viewport.Width,
                                                   Viewport.Height);

            TempViewport.X = Math.Min(Math.Max(0, TempViewport.X), ViewBox.Width - TempViewport.Width);
            TempViewport.Y = Math.Min(Math.Max(0, TempViewport.Y), ViewBox.Height - TempViewport.Height);
            Viewport = TempViewport;
        }

        /// <summary>
        /// Get PixelInfo of current mouse position in ImageViewer.
        /// </summary>
        public PixelInfo GetPixel()
        {
            Point MousePosition = Mouse.GetPosition(this);
            return GetPixel(MousePosition.X, MousePosition.Y);
        }
        /// <summary>
        /// Get PixelInfo of Point(X, Y) at ImageViewer.
        /// </summary>
        public PixelInfo GetPixel(double X, double Y)
        {
            if (X < 0 || ActualWidth < X ||
                Y < 0 || ActualHeight < Y)
                throw new ArgumentOutOfRangeException();

            double AreaX = X - BorderThickness.Left;
            double AreaY = Y - BorderThickness.Top;

            if (LastImageRect.X <= AreaX && AreaX <= LastImageRect.X + LastImageRect.Width &&
                LastImageRect.Y <= AreaY && AreaY <= LastImageRect.Y + LastImageRect.Height)
                return GetPixel(Math.Min(Viewport.X + (int)(AreaX / Factor) - SourceLocation.X, SourceContext.Width - 1),
                                Math.Min(Viewport.Y + (int)(AreaY / Factor) - SourceLocation.Y, SourceContext.Height - 1));

            return PixelInfo.Empty;
        }
        /// <summary>
        /// Get PixelInfo of Point(X, Y) at SourceImage.
        /// </summary>
        public PixelInfo GetPixel(int X, int Y)
        {
            if (X < 0 || SourceContext.Width < X ||
                Y < 0 || SourceContext.Height < Y)
                throw new ArgumentOutOfRangeException();

            unsafe
            {
                switch (SourceContext.Channels)
                {
                    case 1:
                        switch (SourceContext.BitsPerPixel)
                        {
                            case 8:
                                byte Gray = *((byte*)SourceContext.Scan0 + Y * SourceContext.Stride + X);
                                return new PixelInfo(X, Y, 255, Gray, Gray, Gray);
                            case 24:
                                byte* SourceContextScan0 = (byte*)SourceContext.Scan0 + Y * SourceContext.Stride + X * 3;
                                return new PixelInfo(X, Y, 255, *SourceContextScan0++, *SourceContextScan0++, *SourceContextScan0);
                            case 32:
                                return new PixelInfo(X, Y, *((int*)SourceContext.Scan0 + Y * SourceContext.Stride / 4 + X));
                        }
                        break;
                    case 3:
                        {
                            int Offset = Y * SourceContext.Stride + X;
                            byte* SourceContextScanR = (byte*)SourceContext.ScanR + Offset,
                                  SourceContextScanG = (byte*)SourceContext.ScanG + Offset,
                                  SourceContextScanB = (byte*)SourceContext.ScanB + Offset;
                            return new PixelInfo(X, Y, 255, *SourceContextScanR, *SourceContextScanG, *SourceContextScanB);
                        }
                    case 4:
                        {
                            int Offset = Y * SourceContext.Stride + X;
                            byte* SourceContextScanA = (byte*)SourceContext.ScanA + Offset,
                                  SourceContextScanR = (byte*)SourceContext.ScanR + Offset,
                                  SourceContextScanG = (byte*)SourceContext.ScanG + Offset,
                                  SourceContextScanB = (byte*)SourceContext.ScanB + Offset;
                            return new PixelInfo(X, Y, *SourceContextScanA, *SourceContextScanR, *SourceContextScanG, *SourceContextScanB);
                        }
                }
            }
            return PixelInfo.Empty;
        }

        /// <summary>
        /// Get Pixel's Point of current mouse position in ImageViewer.
        /// </summary>
        public Point GetPixelPoint()
        {
            Point MousePosition = Mouse.GetPosition(this);
            return GetPixelPoint(MousePosition.X, MousePosition.Y);
        }
        /// <summary>
        /// Get Pixel's Point of Point(X, Y) at ImageViewer.
        /// </summary>
        public Point GetPixelPoint(double X, double Y)
        {
            if (SourceContext is null)
                throw new ArgumentNullException("Source is null.");

            double AreaX = X - BorderThickness.Left;
            double AreaY = Y - BorderThickness.Top;

            return new Point(Viewport.X + (int)(AreaX / Factor) - SourceLocation.X,
                             Viewport.Y + (int)(AreaY / Factor) - SourceLocation.Y);
        }

        public void Save(string FilePath)
        {
            using FileStream FileStream = new FileStream(FilePath, FileMode.Create);
            FileInfo FileInfo = new FileInfo(FilePath);
            BitmapEncoder Encoder;
            switch (FileInfo.Extension)
            {
                case ".tif":
                    Encoder = new TiffBitmapEncoder();
                    break;
                case ".jpeg":
                    Encoder = new JpegBitmapEncoder();
                    break;
                case ".png":
                    Encoder = new PngBitmapEncoder();
                    break;
                case ".bmp":
                default:
                    Encoder = new BmpBitmapEncoder();
                    break;
            }
            Encoder.Frames.Add(BitmapFrame.Create(GetDisplayImage(this)));
            Encoder.Save(FileStream);
        }

    }
}
