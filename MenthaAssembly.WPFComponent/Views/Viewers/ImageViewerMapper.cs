using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public class ImageViewerMapper : Control
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

        public static readonly DependencyProperty TargetViewerProperty =
              DependencyProperty.Register("TargetViewer", typeof(ImageViewer), typeof(ImageViewerMapper), new PropertyMetadata(default,
                  (d, e) =>
                  {
                      if (d is ImageViewerMapper This)
                      {
                          if (e.OldValue is ImageViewer OldViewer)
                          {
                              OldViewer.SourceChanged -= This.OnSourceChanged;
                              OldViewer.ViewBoxChanged -= This.OnViewBoxChanged;
                              OldViewer.ViewportChanged -= This.OnViewportChanged;
                          }

                          if (e.NewValue is ImageViewer NewViewer)
                          {
                              This.PART_Container?.SetBinding(BackgroundProperty, new Binding
                              {
                                  Path = new PropertyPath(BackgroundProperty),
                                  Source = NewViewer
                              });
                              NewViewer.SourceChanged += This.OnSourceChanged;
                              NewViewer.ViewBoxChanged += This.OnViewBoxChanged;
                              NewViewer.ViewportChanged += This.OnViewportChanged;

                              This.OnRenderSizeChanged(new SizeChangedInfo(This, This.RenderSize, true, true));
                          }
                      }
                  }));
        public ImageViewer TargetViewer
        {
            get => (ImageViewer)GetValue(TargetViewerProperty);
            set => SetValue(TargetViewerProperty, value);
        }

        public static readonly DependencyProperty DisplayImageProperty =
            DependencyProperty.RegisterAttached("DisplayImage", typeof(WriteableBitmap), typeof(ImageViewerMapper), new PropertyMetadata(default,
                (d, e) =>
                {
                    if (d is ImageViewerMapper This &&
                        e.NewValue is WriteableBitmap Bitmap)
                        This.DisplayContext = new BitmapContext(Bitmap);
                }));
        public static void SetDisplayImage(DependencyObject obj, WriteableBitmap value)
            => obj.SetValue(DisplayImageProperty, value);
        public static WriteableBitmap GetDisplayImage(DependencyObject obj)
            => (WriteableBitmap)obj.GetValue(DisplayImageProperty);

        public static readonly DependencyProperty RectStrokeProperty =
              DependencyProperty.Register("RectStroke", typeof(SolidColorBrush), typeof(ImageViewerMapper), new PropertyMetadata(default));
        public SolidColorBrush RectStroke
        {
            get => (SolidColorBrush)GetValue(RectStrokeProperty);
            set => SetValue(RectStrokeProperty, value);
        }

        public static readonly DependencyProperty RectFillProperty =
              DependencyProperty.Register("RectFill", typeof(SolidColorBrush), typeof(ImageViewerMapper), new PropertyMetadata(default));
        public SolidColorBrush RectFill
        {
            get => (SolidColorBrush)GetValue(RectFillProperty);
            set => SetValue(RectFillProperty, value);
        }

        protected Panel PART_Container { set; get; }
        protected Rectangle PART_Rect { set; get; }

        protected BitmapContext DisplayContext { set; get; }

        protected ImageContext SourceContext { set; get; }

        protected Int32Size ViewBox { set; get; }

        protected double ScaleStep { set; get; }

        protected Int32Rect Viewport { set; get; }

        static ImageViewerMapper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewerMapper), new FrameworkPropertyMetadata(typeof(ImageViewerMapper)));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.NewSize.IsEmpty || !(sizeInfo.WidthChanged || sizeInfo.HeightChanged) ||
                PART_Container is null || TargetViewer is null ||
                ActualWidth <= 0d || ActualHeight <= 0d)
                return;

            Size TargetDisplayArea = TargetViewer.GetDisplayArea();
            double ScaleWidth = ActualWidth / TargetDisplayArea.Width;
            double ScaleHeight = ActualHeight / TargetDisplayArea.Height;
            double Scale = ScaleWidth <= 0 ? ScaleHeight : ScaleHeight <= 0 ? ScaleWidth : Math.Min(ScaleWidth, ScaleHeight);

            PART_Container.Width = TargetDisplayArea.Width * Scale;
            PART_Container.Height = TargetDisplayArea.Height * Scale;

            ScaleStep = ViewBox.Width / PART_Container.ActualWidth;
            OnViewportChanged(null, new ChangedEventArgs<Int32Rect>(Int32Rect.Empty, Viewport));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.GetTemplateChild("PART_Rect") is Rectangle PART_Rect)
                this.PART_Rect = PART_Rect;
            if (this.GetTemplateChild("PART_Container") is Panel PART_Container)
            {
                this.PART_Container = PART_Container;
                this.PART_Container.SetBinding(BackgroundProperty, new Binding
                {
                    Path = new PropertyPath(BackgroundProperty),
                    Source = TargetViewer
                });
                PART_Container.PreviewMouseWheel += (s, e) => TargetViewer?.Zoom(e.Delta > 0);
                PART_Container.PreviewMouseDown += OnContainerPreviewMouseDown;
                PART_Container.PreviewMouseMove += OnContainerPreviewMouseMove;
                PART_Container.PreviewMouseUp += OnContainerPreviewMouseUp;
            }
        }

        protected bool IsLeftMouseDown { set; get; } = false;
        protected Point Position { set; get; }
        private void OnContainerPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                PART_Container.CaptureMouse();
                IsLeftMouseDown = true;
                Position = e.GetPosition(PART_Container);
                TargetViewer.Viewport = CalculateViewportLocation(new Int32Point(
                    Math.Min(Math.Max(0, Position.X), PART_Container.ActualWidth) * ScaleStep,
                    Math.Min(Math.Max(0, Position.Y), PART_Container.ActualHeight) * ScaleStep));
            }
        }
        private void OnContainerPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                Point TempPosition = e.GetPosition(PART_Container);
                if (Position.Equals(TempPosition))
                    return;

                Position = TempPosition;
                TargetViewer.Viewport = CalculateViewportLocation(new Int32Point(
                    Math.Min(Math.Max(0, Position.X), PART_Container.ActualWidth) * ScaleStep,
                    Math.Min(Math.Max(0, Position.Y), PART_Container.ActualHeight) * ScaleStep));
            }
        }
        private void OnContainerPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                PART_Container.ReleaseMouseCapture();
                IsLeftMouseDown = false;
            }
        }

        protected Int32Rect CalculateViewportLocation(Int32Point Position)
        {
            Int32Rect TempViewport = new Int32Rect(Position.X - TargetViewer.Viewport.Width / 2,
                                                   Position.Y - TargetViewer.Viewport.Height / 2,
                                                   TargetViewer.Viewport.Width,
                                                   TargetViewer.Viewport.Height);

            TempViewport.X = Math.Min(Math.Max(0, TempViewport.X), ViewBox.Width - TempViewport.Width);
            TempViewport.Y = Math.Min(Math.Max(0, TempViewport.Y), ViewBox.Height - TempViewport.Height);

            return TempViewport;
        }

        private void OnSourceChanged(object sender, ChangedEventArgs<ImageContext> e)
        {
            SourceContext = e.NewValue;
            OnRenderImage();
        }

        protected virtual void OnViewBoxChanged(object sender, ChangedEventArgs<Int32Size> e)
        {
            if (PART_Container is null || TargetViewer is null || ActualWidth <= 0d || ActualHeight <= 0d)
                return;

            Size TargetDisplayArea = TargetViewer.GetDisplayArea();

            double ScaleWidth = ActualWidth / TargetDisplayArea.Width;
            double ScaleHeight = ActualHeight / TargetDisplayArea.Height;
            double Scale = ScaleWidth <= 0 ? ScaleHeight : ScaleHeight <= 0 ? ScaleWidth : Math.Min(ScaleWidth, ScaleHeight);

            PART_Container.Width = TargetDisplayArea.Width * Scale;
            PART_Container.Height = TargetDisplayArea.Height * Scale;

            ViewBox = e.NewValue;
            ScaleStep = ViewBox.Width / PART_Container.Width;
            OnRenderImage();
        }

        protected virtual void OnViewportChanged(object sender, ChangedEventArgs<Int32Rect> e)
        {
            if (double.IsNaN(ScaleStep) || ScaleStep <= 0)
                return;

            Viewport = e.NewValue;
            PART_Rect.Margin = new Thickness(Viewport.X / ScaleStep, Viewport.Y / ScaleStep, 0, 0);
            PART_Rect.Width = Viewport.Width / ScaleStep;
            PART_Rect.Height = Viewport.Height / ScaleStep;
        }

        protected CancellationTokenSource OnDrawTokenSource { set; get; }
        protected void OnRenderImage()
        {
            if (SourceContext is null ||
                PART_Container is null ||
                PART_Container.ActualWidth is 0 ||
                PART_Container.ActualHeight is 0)
                return;

            if (DisplayContext is null ||
                DisplayContext.Width != (int)PART_Container.Width ||
                DisplayContext.Height != (int)PART_Container.Height)
            {
                SetDisplayImage(this, new WriteableBitmap((int)PART_Container.Width, (int)PART_Container.Height, 96, 96, PixelFormats.Bgra32, null));
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
                        Int32Rect DirtyRect = OnDraw(OnDrawTokenSource.Token);
                        Display.AddDirtyRect(DirtyRect);
                        Debug.WriteLine(DirtyRect);
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
        protected Int32Rect OnDraw(CancellationToken Token)
        {
            unsafe
            {
                if (SourceContext != null && ScaleStep > 0)
                {
                    // Calculate Original Source's Rect
                    Int32Point SourceLocation = new Int32Point((ViewBox.Width - SourceContext.Width) / 2,
                                                               (ViewBox.Height - SourceContext.Height) / 2),
                               SourceEndPoint = new Int32Point(SourceLocation.X + SourceContext.Width,
                                                               SourceLocation.Y + SourceContext.Height);
                    // Calculate Source's Rect in ImageViewer.
                    int ImageX1 = Math.Max((int)(SourceLocation.X / ScaleStep), 0),
                        ImageY1 = Math.Max((int)(SourceLocation.Y / ScaleStep), 0),
                        ImageX2 = Math.Min((int)(SourceEndPoint.X / ScaleStep) + 1, DisplayContext.Width),
                        ImageY2 = Math.Min((int)(SourceEndPoint.Y / ScaleStep) + 1, DisplayContext.Height);

                    // Calculate DirtyRect (Compare with LastImageRect)
                    int DirtyRectX1 = Math.Min(LastImageRect.X, ImageX1),
                        DirtyRectX2 = Math.Max(LastImageRect.X + LastImageRect.Width, ImageX2),
                        DirtyRectY1 = Math.Min(LastImageRect.Y, ImageY1),
                        DirtyRectY2 = Math.Max(LastImageRect.Y + LastImageRect.Height, ImageY2);

                    LastImageRect = new Int32Rect(ImageX1, ImageY1, ImageX2 - ImageX1, ImageY2 - ImageY1);


                    int* DisplayContextScan0 = (int*)DisplayContext.Scan0;
                    int DisplayStrideWidth = DisplayContext.Stride / DisplayContext.PixelBytes;

                    switch (SourceContext.Channel)
                    {
                        case 1:
                            switch (SourceContext.PixelBytes)
                            {
                                case 1:
                                    {
                                        byte* SourceContextScan0 = (byte*)SourceContext.Scan0;

                                        Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                                        {
                                            int* Data = DisplayContextScan0 + j * DisplayStrideWidth + DirtyRectX1;

                                            double Y = j * ScaleStep;
                                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                            {
                                                double X = DirtyRectX1 * ScaleStep;
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                {
                                                    if (Token.IsCancellationRequested)
                                                        return;

                                                    if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                                    {
                                                        byte* SourceContextBuffer = SourceContextScan0 +
                                                                                    ((int)Y - SourceLocation.Y) * SourceContext.Stride +
                                                                                    ((int)X - SourceLocation.X);
                                                        *Data++ = 255 << 24 |                   // A
                                                                  *SourceContextBuffer << 16 |  // R
                                                                  *SourceContextBuffer << 8 |   // G
                                                                  *SourceContextBuffer;         // B
                                                    }
                                                    else
                                                    {
                                                        // Clear
                                                        *Data++ = 0;
                                                    }
                                                    X += ScaleStep;
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
                                case 3:
                                    {
                                        byte* SourceContextScan0 = (byte*)SourceContext.Scan0;

                                        Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                                        {
                                            int* Data = DisplayContextScan0 + j * DisplayStrideWidth + DirtyRectX1;

                                            double Y = j * ScaleStep;
                                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                            {
                                                double X = DirtyRectX1 * ScaleStep;
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                {
                                                    if (Token.IsCancellationRequested)
                                                        return;

                                                    if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                                    {
                                                        byte* SourceContextBuffer = SourceContextScan0 +
                                                                                    ((int)Y - SourceLocation.Y) * SourceContext.Stride +
                                                                                    ((int)X - SourceLocation.X) * SourceContext.PixelBytes;
                                                        *Data++ = 255 << 24 |                       // A
                                                                  *SourceContextBuffer++ << 16 |    // R
                                                                  *SourceContextBuffer++ << 8 |     // G
                                                                  *SourceContextBuffer;             // B
                                                    }
                                                    else
                                                    {
                                                        // Clear
                                                        *Data++ = 0;
                                                    }
                                                    X += ScaleStep;
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
                                        int* SourceContextScan0 = (int*)SourceContext.Scan0;
                                        int SourceStrideWidth = SourceContext.Stride / SourceContext.PixelBytes;

                                        Parallel.For(DirtyRectY1, DirtyRectY2, (j) =>
                                        {
                                            int* Data = DisplayContextScan0 + j * DisplayStrideWidth + DirtyRectX1;

                                            double Y = j * ScaleStep;
                                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                            {
                                                double X = DirtyRectX1 * ScaleStep;
                                                for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                                {
                                                    if (Token.IsCancellationRequested)
                                                        return;

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
                                                    X += ScaleStep;
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

                                    double Y = j * ScaleStep;
                                    if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                    {
                                        double X = DirtyRectX1 * ScaleStep;
                                        for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        {
                                            if (Token.IsCancellationRequested)
                                                return;

                                            if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                            {
                                                int Index = ((int)Y - SourceLocation.Y) * SourceContext.Stride + (int)X - SourceLocation.X;
                                                *Data++ = 255 << 24 |                           // A
                                                          *(SourceContextScanR + Index) << 16 | // R
                                                          *(SourceContextScanG + Index) << 8 |  // G
                                                          *(SourceContextScanB + Index);        // B
                                            }
                                            else
                                            {
                                                // Clear
                                                *Data++ = 0;
                                            }
                                            X += ScaleStep;
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

                                    double Y = j * ScaleStep;
                                    if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                    {
                                        double X = DirtyRectX1 * ScaleStep;
                                        for (int i = DirtyRectX1; i < DirtyRectX2; i++)
                                        {
                                            if (Token.IsCancellationRequested)
                                                return;

                                            if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                            {
                                                int Index = ((int)Y - SourceLocation.Y) * SourceContext.Stride + (int)X - SourceLocation.X;
                                                *Data++ = *(SourceContextScanA + Index) << 24 | // A
                                                          *(SourceContextScanR + Index) << 16 | // R
                                                          *(SourceContextScanG + Index) << 8 |  // G
                                                          *(SourceContextScanB + Index);        // B
                                            }
                                            else
                                            {
                                                // Clear
                                                *Data++ = 0;
                                            }
                                            X += ScaleStep;
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
    }
}
