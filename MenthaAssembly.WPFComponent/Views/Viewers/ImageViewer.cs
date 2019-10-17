using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly.Views
{
    public class ImageViewer : Control
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
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern void SetMemory(IntPtr dst, int Color, int Length);

        public event EventHandler<ChangedEventArgs<ImageContext>> SourceChanged;

        public event EventHandler<ChangedEventArgs<Int32Size>> ViewBoxChanged;

        public event EventHandler<ChangedEventArgs<Int32Rect>> ViewportChanged;

        public static readonly DependencyProperty SourceProperty =
              DependencyProperty.Register("Source", typeof(BitmapSource), typeof(ImageViewer), new PropertyMetadata(default,
                  (d, e) =>
                  {
                      if (d is ImageViewer This &&
                          e.NewValue is BitmapSource Image)
                          This.SourceContext = Image.GetImageContext();
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
        public static void SetDisplayImage(DependencyObject obj, WriteableBitmap value)
            => obj.SetValue(DisplayImageProperty, value);
        public static WriteableBitmap GetDisplayImage(DependencyObject obj)
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

        public static readonly DependencyProperty ZoomScaleProperty =
              DependencyProperty.Register("ZoomScale", typeof(double), typeof(ImageViewer), new PropertyMetadata(2d));
        public double ZoomScale
        {
            get => (double)GetValue(ZoomScaleProperty);
            set => SetValue(ZoomScaleProperty, value);
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

        private ImageContext _SourceContext;
        protected ImageContext SourceContext
        {
            get => _SourceContext;
            set
            {
                ChangedEventArgs<ImageContext> e = new ChangedEventArgs<ImageContext>(_SourceContext, value);
                _SourceContext = value;
                OnSourceChanged(e);
            }
        }


        static ImageViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewer), new FrameworkPropertyMetadata(typeof(ImageViewer)));
        }

        public void Zoom(bool ZoomIn)
        {
            if (ZoomIn)
            {
                if (Factor < 1)
                    Factor = Math.Min(1, Factor * ZoomScale);
            }
            else
            {
                if (Factor > MinFactor)
                    Factor = Math.Max(MinFactor, Factor / ZoomScale);
            }
        }
        public void Zoom(bool ZoomIn, Int32Point CenterPoint)
        {
            if (ZoomIn)
            {
                if (Factor < 1)
                {
                    IsZoomWithCenterPoint = true;
                    this.CenterPoint = CenterPoint;
                    Factor = Math.Min(1, Factor * ZoomScale);
                    IsZoomWithCenterPoint = false;
                }
            }
            else
            {
                if (Factor > MinFactor)
                {
                    IsZoomWithCenterPoint = true;
                    this.CenterPoint = CenterPoint;
                    Factor = Math.Max(MinFactor, Factor / ZoomScale);
                    IsZoomWithCenterPoint = false;
                }
            }
        }

        public void SetSourceContext(ImageContext Source)
            => SourceContext = Source;
        public void SetSourceContext(int Width, int Height, IntPtr Scan0, int Stride, int PixelBytes)
            => SourceContext = new ImageContext(Width, Height, Scan0, Stride, PixelBytes);
        public void SetSourceContext(int Width, int Height, IntPtr ScanR, IntPtr ScanG, IntPtr ScanB, int Stride)
            => SourceContext = new ImageContext(Width, Height, ScanR, ScanG, ScanB, Stride);

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

            double Factor = CalculateFactor();
            if (Factor.Equals(this.Factor))
            {
                OnFactorChanged();
                return;
            }
            this.Factor = Factor;
        }

        protected bool IsMinFactor { set; get; } = true;
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

            UpdateImage();
        }

        protected Int32Size CalculateViewBox()
        {
            if (SourceContext is null || ActualHeight <= 0 || ActualWidth <= 0)
                return Int32Size.Empty;

            double Ratio = 1;
            double Scale = Math.Max(SourceContext.Width / ActualWidth, SourceContext.Height / ActualHeight);

            while (Ratio < Scale)
                Ratio *= ZoomScale;

            MinFactor = 1d / Ratio;
            return new Int32Size(ActualWidth * Ratio, ActualHeight * Ratio);
        }

        protected double CalculateFactor()
        {
            if (IsMinFactor || Factor.Equals(-1d))
                return MinFactor;

            return Math.Max(MinFactor, Factor);
        }

        protected bool IsZoomWithCenterPoint { set; get; }
        protected Int32Point CenterPoint { set; get; }
        protected Int32Rect CalculateViewport()
        {
            Int32Size ViewportHalfSize = ViewBox * (MinFactor / Factor / 2);
            Int32Point C0 = IsZoomWithCenterPoint ? CenterPoint : new Int32Point(Viewport.X + Viewport.Width / 2, Viewport.Y + Viewport.Height / 2);

            Int32Rect Result = new Int32Rect(C0.X - ViewportHalfSize.Width,
                                             C0.Y - ViewportHalfSize.Height,
                                             ViewportHalfSize.Width * 2,
                                             ViewportHalfSize.Height * 2);
            Result.X = Math.Min(Math.Max(0, Result.X), ViewBox.Width - Result.Width);
            Result.Y = Math.Min(Math.Max(0, Result.Y), ViewBox.Height - Result.Height);

            return Result;
        }

        protected CancellationTokenSource DrawTokenSource { set; get; }
        protected async void UpdateImage()
        {
            if (ActualWidth is 0 || ActualHeight is 0)
                return;

            if (DisplayContext is null ||
                DisplayContext.Width != (int)ActualWidth ||
                DisplayContext.Height != (int)ActualHeight)
                SetDisplayImage(this, new WriteableBitmap((int)ActualWidth, (int)ActualHeight, 96, 96, PixelFormats.Bgra32, null));

            DrawTokenSource?.Cancel();
            DrawTokenSource?.Dispose();
            DrawTokenSource = new CancellationTokenSource();

            BitmapContext Display = DisplayContext;
            if (Display.TryLock(100))
            {
                await Draw(ViewBox, Factor, Viewport, DrawTokenSource.Token);

                Display.AddDirtyRect(new Int32Rect(0, 0, Display.Width, Display.Height));
                Display.Unlock();
            }
        }

        protected Task Draw(Int32Size ViewBox, double Factor, Int32Rect Viewport, CancellationToken Token)
            => Task.Run(() =>
            {
                unsafe
                {
                    if (SourceContext != null && Factor > 0)
                    {
                        Int32Point SourceLocation = new Int32Point((ViewBox.Width - SourceContext.Width) / 2,
                                                                   (ViewBox.Height - SourceContext.Height) / 2);
                        Int32Point SourceEndPoint = SourceLocation + new Int32Vector(SourceContext.Width, SourceContext.Height);

                        byte* DisplayContextScan0 = (byte*)DisplayContext.Scan0;
                        byte* SourceContextScan0 = (byte*)SourceContext.Scan0;
                        double FactorStep = 1 / Factor;

                        Parallel.For(0, DisplayContext.Height, (j) =>
                        {
                            byte* Data = DisplayContextScan0 + j * DisplayContext.Stride;

                            double Y = Viewport.Y + j * FactorStep;
                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                            {
                                double X = Viewport.X;
                                for (int i = 0; i < DisplayContext.Stride; i += DisplayContext.PixelBytes)
                                {
                                    if (Token.IsCancellationRequested)
                                        return;

                                    if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                    {
                                        byte* SourceContextBuffer = SourceContextScan0 +
                                                                    ((int)Y - SourceLocation.Y) * SourceContext.Stride +
                                                                    ((int)X - SourceLocation.X) * SourceContext.PixelBytes;
                                        //Draw SourceContext
                                        for (int k = 0; k < DisplayContext.PixelBytes; k++)
                                        {
                                            *Data = *SourceContextBuffer;
                                            Data++;
                                            SourceContextBuffer++;
                                        }
                                    }
                                    else
                                    {
                                        // Clear
                                        SetMemory((IntPtr)Data, 0, DisplayContext.PixelBytes);
                                        Data += DisplayContext.PixelBytes;
                                    }
                                    X += FactorStep;
                                }
                            }
                            else
                            {
                                Data += DisplayContext.PixelBytes - 1;
                                try
                                {
                                    for (int i = 0; i < DisplayContext.Stride; i += DisplayContext.PixelBytes)
                                    {
                                        *Data = 0;
                                        Data += DisplayContext.PixelBytes;
                                    }
                                }
                                catch
                                {
                                    return;
                                }

                                //try
                                //{
                                //    // Clear
                                //    SetMemory((IntPtr)Data, 0, DisplayContext.Stride);
                                //}
                                //catch
                                //{
                                //    return;
                                //}
                            }
                        });
                    }
                    else
                    {
                        // Clear
                        SetMemory(DisplayContext.Scan0, 0, DisplayContext.Stride * DisplayContext.Height);
                    }
                }
            });

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Point Position = e.GetPosition(this);
            double FactorStep = 1 / Factor;
            Zoom(e.Delta > 0, new Int32Point(Viewport.X + Position.X * FactorStep, Viewport.Y + Position.Y * FactorStep));
        }

        protected bool IsLeftMouseDown { set; get; } = false;
        protected Point MousePosition { set; get; }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.CaptureMouse();
                IsLeftMouseDown = true;
                MousePosition = e.GetPosition(this);
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                Point Position = e.GetPosition(this);
                Int32Rect TempViewport = Viewport - new Int32Vector(new Vector(Position.X - MousePosition.X, Position.Y - MousePosition.Y) / Factor);
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
            }
        }

    }
}
