using System;
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

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(ImageViewerMapper), new PropertyMetadata(default));
        public object Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleTemplateProperty =
            DependencyProperty.Register("TitleTemplate", typeof(ControlTemplate), typeof(ImageViewerMapper), new PropertyMetadata(default));
        public ControlTemplate TitleTemplate
        {
            get => (ControlTemplate)GetValue(TitleTemplateProperty);
            set => SetValue(TitleTemplateProperty, value);
        }

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

        protected Control PART_Title { set; get; }
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

            double ScaleWidth = ActualWidth / TargetViewer.ActualWidth;
            double ScaleHeight = (ActualHeight - PART_Title?.ActualHeight ?? 0d) / TargetViewer.ActualHeight;
            double Scale = ScaleWidth <= 0 ? ScaleHeight : ScaleHeight <= 0 ? ScaleWidth : Math.Min(ScaleWidth, ScaleHeight);

            PART_Container.Width = TargetViewer.ActualWidth * Scale;
            PART_Container.Height = TargetViewer.ActualHeight * Scale;

            ScaleStep = ViewBox.Width / PART_Container.ActualWidth;
            OnViewportChanged(null, new ChangedEventArgs<Int32Rect>(Int32Rect.Empty, Viewport));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.GetTemplateChild("PART_Title") is Control PART_Title)
                this.PART_Title = PART_Title;
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


        //private void OnContainerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    if (TargetViewer is null)
        //        return;

        //    Point TempPosition = e.GetPosition(PART_Container);
        //    TargetViewer.Zoom(e.Delta > 0, new Int32Point(TempPosition.X * ScaleStep, TempPosition.Y * ScaleStep));
        //}

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
            UpdateImage();
        }

        protected virtual void OnViewBoxChanged(object sender, ChangedEventArgs<Int32Size> e)
        {
            if (PART_Container is null || TargetViewer is null || ActualWidth <= 0d || ActualHeight <= 0d)
                return;

            double ScaleWidth = ActualWidth / TargetViewer.ActualWidth;
            double ScaleHeight = (ActualHeight - PART_Title?.ActualHeight ?? 0d) / TargetViewer.ActualHeight;
            double Scale = ScaleWidth <= 0 ? ScaleHeight : ScaleHeight <= 0 ? ScaleWidth : Math.Min(ScaleWidth, ScaleHeight);

            PART_Container.Width = TargetViewer.ActualWidth * Scale;
            PART_Container.Height = TargetViewer.ActualHeight * Scale;

            ViewBox = e.NewValue;
            ScaleStep = ViewBox.Width / PART_Container.ActualWidth;
            UpdateImage();
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

        protected CancellationTokenSource DrawTokenSource { set; get; }
        protected async void UpdateImage()
        {
            if (SourceContext is null ||
                PART_Container is null ||
                PART_Container.ActualWidth is 0 ||
                PART_Container.ActualHeight is 0)
                return;

            if (DisplayContext is null ||
                DisplayContext.Width != (int)PART_Container.ActualWidth ||
                DisplayContext.Height != (int)PART_Container.ActualHeight)
                SetDisplayImage(this, new WriteableBitmap((int)PART_Container.ActualWidth, (int)PART_Container.ActualHeight, 96, 96, PixelFormats.Bgra32, null));

            DrawTokenSource?.Cancel();
            DrawTokenSource?.Dispose();
            DrawTokenSource = new CancellationTokenSource();

            BitmapContext Display = DisplayContext;
            if (Display.TryLock(100))
            {
                await Draw(DrawTokenSource.Token);

                Display.AddDirtyRect(new Int32Rect(0, 0, Display.Width, Display.Height));
                Display.Unlock();
            }
        }

        protected Task Draw(CancellationToken Token)
            => Task.Run(() =>
            {
                unsafe
                {
                    if (SourceContext != null && ScaleStep > 0)
                    {
                        Int32Point SourceLocation = new Int32Point((ViewBox.Width - SourceContext.Width) / 2,
                                                                   (ViewBox.Height - SourceContext.Height) / 2);
                        Int32Point SourceEndPoint = SourceLocation + new Int32Vector(SourceContext.Width, SourceContext.Height);

                        switch (SourceContext.Channel)
                        {
                            case 1:
                                {
                                    byte* DisplayContextScan0 = (byte*)DisplayContext.Scan0;
                                    byte* SourceContextScan0 = (byte*)SourceContext.Scan0;

                                    Parallel.For(0, DisplayContext.Height, (j) =>
                                    {
                                        byte* Data = DisplayContextScan0 + j * DisplayContext.Stride;

                                        double Y = j * ScaleStep;
                                        try
                                        {
                                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                            {
                                                double X = 0;
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
                                                    X += ScaleStep;
                                                }
                                            }
                                            else
                                            {
                                                Data += DisplayContext.PixelBytes - 1;
                                                for (int i = 0; i < DisplayContext.Stride; i += DisplayContext.PixelBytes)
                                                {
                                                    *Data = 0;
                                                    Data += DisplayContext.PixelBytes;
                                                }
                                                //    // Clear
                                                //    SetMemory((IntPtr)Data, 0, DisplayContext.Stride);
                                            }
                                        }
                                        catch
                                        {
                                            return;
                                        }
                                    });
                                }
                                break;
                            case 3:
                                {
                                    byte* DisplayContextScan0 = (byte*)DisplayContext.Scan0;
                                    byte* SourceContextScanR = (byte*)SourceContext.ScanR;
                                    byte* SourceContextScanG = (byte*)SourceContext.ScanG;
                                    byte* SourceContextScanB = (byte*)SourceContext.ScanB;

                                    Parallel.For(0, DisplayContext.Height, (j) =>
                                    {
                                        byte* Data = DisplayContextScan0 + j * DisplayContext.Stride;

                                        double Y = j * ScaleStep;
                                        try
                                        {
                                            if (SourceLocation.Y <= Y && Y < SourceEndPoint.Y)
                                            {
                                                double X = 0;
                                                for (int i = 0; i < DisplayContext.Stride; i += DisplayContext.PixelBytes)
                                                {
                                                    if (Token.IsCancellationRequested)
                                                        return;

                                                    if (SourceLocation.X <= X && X < SourceEndPoint.X)
                                                    {
                                                        int Index = ((int)Y - SourceLocation.Y) * SourceContext.Stride + (int)X - SourceLocation.X;
                                                        // B
                                                        *Data = *(SourceContextScanB + Index);
                                                        Data++;
                                                        // G
                                                        *Data = *(SourceContextScanG + Index);
                                                        Data++;
                                                        // R
                                                        *Data = *(SourceContextScanR + Index);
                                                        Data++;
                                                        // A
                                                        *Data = 255;
                                                        Data++;
                                                    }
                                                    else
                                                    {
                                                        // Clear
                                                        SetMemory((IntPtr)Data, 0, DisplayContext.PixelBytes);
                                                        Data += DisplayContext.PixelBytes;
                                                    }
                                                    X += ScaleStep;
                                                }
                                            }
                                            else
                                            {
                                                Data += DisplayContext.PixelBytes - 1;
                                                for (int i = 0; i < DisplayContext.Stride; i += DisplayContext.PixelBytes)
                                                {
                                                    *Data = 0;
                                                    Data += DisplayContext.PixelBytes;
                                                }
                                                //    // Clear
                                                //    SetMemory((IntPtr)Data, 0, DisplayContext.Stride);
                                            }
                                        }
                                        catch
                                        {
                                            return;
                                        }
                                    });
                                }
                                break;
                        }
                    }
                    else
                    {
                        // Clear
                        SetMemory(DisplayContext.Scan0, 0, DisplayContext.Stride * DisplayContext.Height);
                    }
                }
            });


    }
}
