using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Utils;
using MenthaAssembly.Views.Primitives;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly.Views
{
    public unsafe class ImageViewerLayer : FrameworkElement
    {
        private delegate void DrawAction(IPixelAdapter<BGRA> Adapter, BGRA* pDisplay);
        private static readonly ParallelOptions DefaultParallelOptions = new();

        public event EventHandler<ChangedEventArgs<IImageContext>> SourceChanged;

        public event EventHandler<ChangedEventArgs<ImageChannel>> ChannelChanged;

        internal static readonly DependencyProperty AttachedLayerProperty =
              DependencyProperty.Register("AttachedLayer", typeof(ImageViewerLayer), typeof(ImageViewerLayer), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is ImageViewerLayer This)
                      {
                          if (e.OldValue is ImageViewerLayer DetachLayer)
                          {
                              DetachLayer.SourceChanged -= This.OnAttachLayerSourceChanged;
                              This.ClearValue(ChannelProperty);
                              This.ClearValue(VisibilityProperty);
                              This.SourceContext = null;
                          }

                          if (e.NewValue is ImageViewerLayer AttachLayer)
                          {
                              AttachLayer.SourceChanged += This.OnAttachLayerSourceChanged;
                              This.SetBinding(ChannelProperty, new Binding(nameof(Channel)) { Source = AttachLayer });
                              This.SetBinding(VisibilityProperty, new Binding(nameof(Visibility)) { Source = AttachLayer });
                              This.SourceContext = AttachLayer.SourceContext;
                          }
                      }
                  }));
        internal ImageViewerLayer AttachedLayer
        {
            get => (ImageViewerLayer)GetValue(AttachedLayerProperty);
            set => SetValue(AttachedLayerProperty, value);
        }

        internal static readonly DependencyProperty DisplayImageProperty =
              DependencyProperty.Register("DisplayImage", typeof(WriteableBitmap), typeof(ImageViewerLayer), new FrameworkPropertyMetadata(null,
                  FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) =>
                {
                    if (d is ImageViewerLayer This)
                        This.DisplayContext = e.NewValue is WriteableBitmap Bitmap ? new BitmapContext(Bitmap) : null;
                }));
        internal WriteableBitmap DisplayImage
        {
            get => (WriteableBitmap)GetValue(DisplayImageProperty);
            set => SetValue(DisplayImageProperty, value);
        }

        protected internal virtual BitmapContext DisplayContext { set; get; }

        public static readonly DependencyProperty SourceProperty =
              Image.SourceProperty.AddOwner(typeof(ImageViewerLayer), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is ImageViewerLayer This)
                          This.SourceContext = (e.NewValue as ImageSource)?.ToImageContext();
                  }));
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private IImageContext _SourceContext;
        public IImageContext SourceContext
        {
            get => _SourceContext;
            set
            {
                if (_SourceContext is null &&
                    value is null)
                    return;

                ChangedEventArgs<IImageContext> e = new ChangedEventArgs<IImageContext>(_SourceContext, value);
                _SourceContext = value;
                OnSourceChanged(e);
            }
        }

        public static readonly DependencyProperty ChannelProperty =
              DependencyProperty.Register("Channel", typeof(ImageChannel), typeof(ImageViewerLayer), new PropertyMetadata(ImageChannel.All,
                  (d, e) =>
                  {
                      if (d is ImageViewerLayer This)
                          This.OnChannelChanged(new ChangedEventArgs<ImageChannel>(e.OldValue, e.NewValue));
                  }));
        public ImageChannel Channel
        {
            get => (ImageChannel)GetValue(ChannelProperty);
            set => SetValue(ChannelProperty, value);
        }

        internal bool IsGeneratedFromSystem = false;

        protected internal ImageViewerBase Viewer;
        static ImageViewerLayer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewerLayer), new FrameworkPropertyMetadata(typeof(ImageViewerLayer)));
        }

        private void OnAttachLayerSourceChanged(object sender, ChangedEventArgs<IImageContext> e)
            => SourceContext = e.NewValue;

        protected virtual void OnSourceChanged(ChangedEventArgs<IImageContext> e)
            => SourceChanged?.Invoke(this, e);

        protected virtual void OnChannelChanged(ChangedEventArgs<ImageChannel> e)
        {
            if (e != null)
                ChannelChanged?.Invoke(this, e);

            ImageChannel Value = e?.NewValue ?? Channel;
            DrawHandler = Value switch
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

            UpdateCanvas();
        }

        protected int DisplayAreaWidth = 0,
                      DisplayAreaHeight = 0;
        protected override Size MeasureOverride(Size AvailableSize)
        {
            DisplayAreaWidth = (int)AvailableSize.Width;
            DisplayAreaHeight = (int)AvailableSize.Height;
            return AvailableSize;
        }

        protected override void OnRender(DrawingContext Context)
        {
            WriteableBitmap Image = DisplayImage;
            if (Image == null)
                return;

            //computed from the ArrangeOverride return size 
            Context.DrawImage(Image, new Rect(RenderSize));
        }

        protected Bound<float> LastImageBound;
        private DelayActionToken UpdateCanvasToken;
        public virtual void UpdateCanvas()
        {
            UpdateCanvasToken?.Cancel();

            if (Viewer is null ||
                _SourceContext is null ||
                _SourceContext.Width is 0 ||
                _SourceContext.Height is 0)
            {
                DisplayImage = null;
                return;
            }

            if (!IsMeasureValid ||
                DisplayAreaWidth is 0 ||
                DisplayAreaHeight is 0)
            {
                DisplayImage = null;
                UpdateCanvasToken = DispatcherHelper.DelayAction(10d, () => UpdateCanvas());
                return;
            }

            if (DisplayContext is null ||
                DisplayContext.Width != DisplayAreaWidth ||
                DisplayContext.Height != DisplayAreaHeight)
            {
                DisplayImage = new WriteableBitmap(DisplayAreaWidth, DisplayAreaHeight, 96, 96, PixelFormats.Bgra32, null);
                LastImageBound = new Bound<float>(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                BitmapContext Display = DisplayContext;
                if (Display.TryLock(1))
                {
                    try
                    {
                        Int32Rect DirtyRect = OnDraw();
                        Display.AddDirtyRect(DirtyRect);
                    }
                    catch { }
                    finally
                    {
                        Display.Unlock();
                        Display = null;
                    }
                }
            }));
        }

        protected static readonly BGRA EmptyPixel = new();
        private DrawAction DrawHandler = (Adapter, pDisplay) => Adapter.OverrideTo(pDisplay);

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
        protected virtual Int32Rect OnDraw()
        {
            double Scale = Viewer.InternalScale;
            if (Scale > 0d)
            {
                Rect Viewport = Viewer.InternalViewport;
                int SourceW = SourceContext.Width,
                    SourceH = SourceContext.Height,
                    ContextX = Viewer.ContextX,
                    ContextY = Viewer.ContextY,
                    ContextW = Viewer.ContextWidth,
                    ContextH = Viewer.ContextHeight;

                // Align
                AlignContextLocation(SourceW, SourceH, ContextW, ContextH, ref ContextX, ref ContextY);

                // Calculate Source's Rect in ImageViewer.
                float SourceEx = ContextX + SourceW,
                      SourceEy = ContextY + SourceH,
                      ISx = Math.Max((float)((ContextX - Viewport.X) * Scale), 0f),
                      ISy = Math.Max((float)((ContextY - Viewport.Y) * Scale), 0f),
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

                int IntViewportX = (int)Viewport.X,
                    IntViewportY = (int)Viewport.Y,
                    IntISx = IntViewportX - ContextX,
                    IntISy = IntViewportY - ContextY;
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

                Parallel.For(IntDirtyY1, IntDirtyY2, Viewer.RenderParallelOptions ?? DefaultParallelOptions, j =>
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

            // Clear
            DisplayContext.Clear(EmptyPixel, null);
            return new Int32Rect(0, 0, DisplayContext.Width, DisplayContext.Height);
        }

        private void AlignContextLocation(int SourceW, int SourceH, int ContextW, int ContextH, ref int ContextX, ref int ContextY)
        {
            // Align
            if (HorizontalAlignment == HorizontalAlignment.Center)
                ContextX += (ContextW - SourceW) >> 1;
            else if (HorizontalAlignment == HorizontalAlignment.Right)
                ContextX += ContextW - SourceW;

            if (VerticalAlignment == VerticalAlignment.Center)
                ContextY += (ContextH - SourceH) >> 1;
            else if (VerticalAlignment == VerticalAlignment.Bottom)
                ContextY += ContextH - SourceH;
        }

        /// <summary>
        /// Get Pixel of current mouse position in ImageViewerLayer.
        /// </summary>
        public IPixel GetPixel()
        {
            Point MousePosition = Mouse.GetPosition(this);
            return GetPixel(MousePosition.X, MousePosition.Y);
        }
        /// <summary>
        /// Get Pixel of Point(X, Y) at ImageViewerLayer.
        /// </summary>
        public IPixel GetPixel(double X, double Y)
        {
            if (X < 0 || ActualWidth < X ||
                Y < 0 || ActualHeight < Y)
                throw new ArgumentOutOfRangeException();

            if (LastImageBound.Contains((float)X, (float)Y))
            {
                double Scale = Viewer.InternalScale;
                Rect Viewport = Viewer.InternalViewport;
                int SourceW = SourceContext.Width,
                    SourceH = SourceContext.Height,
                    ContextX = Viewer.ContextX,
                    ContextY = Viewer.ContextY,
                    ContextW = Viewer.ContextWidth,
                    ContextH = Viewer.ContextHeight;

                // Align
                AlignContextLocation(SourceW, SourceH, ContextW, ContextH, ref ContextX, ref ContextY);

                return GetPixel(Math.Min((int)Math.Round(Viewport.X + X / Scale - ContextX), SourceW - 1),
                                Math.Min((int)Math.Round(Viewport.Y + Y / Scale - ContextY), SourceH - 1));
            }

            return new BGRA();
        }
        /// <summary>
        /// Get Pixel of Point(X, Y) at SourceImage.
        /// </summary>
        public IPixel GetPixel(int X, int Y)
            => SourceContext[X, Y];

        /// <summary>
        /// Get Pixel's Point of current mouse position in ImageViewerLayer.
        /// </summary>
        public Point GetPixelPoint()
        {
            Point MousePosition = Mouse.GetPosition(this);
            return GetPixelPoint(MousePosition.X, MousePosition.Y);
        }
        /// <summary>
        /// Get Pixel's Point of Point(X, Y) at ImageViewerLayer.
        /// </summary>
        public Point GetPixelPoint(double X, double Y)
        {
            if (SourceContext is null)
                throw new ArgumentNullException("Source is null.");

            double Scale = Viewer.InternalScale;
            Rect Viewport = Viewer.InternalViewport;
            int SourceW = SourceContext.Width,
                SourceH = SourceContext.Height,
                ContextX = Viewer.ContextX,
                ContextY = Viewer.ContextY,
                ContextW = Viewer.ContextWidth,
                ContextH = Viewer.ContextHeight;

            // Align
            AlignContextLocation(SourceW, SourceH, ContextW, ContextH, ref ContextX, ref ContextY);

            return new Point(Viewport.X + X / Scale - ContextX,
                             Viewport.Y + Y / Scale - ContextY);
        }

        /// <summary>
        /// Get Point of ImageViewerLayer at PixelPoint(X, Y).
        /// </summary>
        public Point GetLayerPoint(Point PixelPoint)
            => GetLayerPoint(PixelPoint.X, PixelPoint.Y);
        /// <summary>
        /// Get Point of ImageViewerLayer at PixelPoint(X, Y).
        /// </summary>
        public Point GetLayerPoint(double X, double Y)
        {
            if (SourceContext is null)
                throw new ArgumentNullException("Source is null.");

            double Scale = Viewer.InternalScale;
            Rect Viewport = Viewer.InternalViewport;
            int SourceW = SourceContext.Width,
                SourceH = SourceContext.Height,
                ContextX = Viewer.ContextX,
                ContextY = Viewer.ContextY,
                ContextW = Viewer.ContextWidth,
                ContextH = Viewer.ContextHeight;

            // Align
            AlignContextLocation(SourceW, SourceH, ContextW, ContextH, ref ContextX, ref ContextY);

            return new Point((X + ContextX - Viewport.X) * Scale,
                             (Y + ContextY - Viewport.Y) * Scale);
        }

    }
}
