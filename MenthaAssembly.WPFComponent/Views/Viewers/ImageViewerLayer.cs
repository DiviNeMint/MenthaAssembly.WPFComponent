using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Utils;
using MenthaAssembly.Views.Primitives;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(Marks))]
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
                              This.Marks = new ImageViewerLayerMarkCollection();
                              This.SourceContext = null;
                          }

                          if (e.NewValue is ImageViewerLayer AttachLayer)
                          {
                              AttachLayer.SourceChanged += This.OnAttachLayerSourceChanged;
                              This.SetBinding(ChannelProperty, new Binding(nameof(Channel)) { Source = AttachLayer });
                              This.SetBinding(VisibilityProperty, new Binding(nameof(Visibility)) { Source = AttachLayer });
                              This.Marks = AttachLayer.Marks;
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
                          This.OnChannelChanged(e.ToChangedEventArgs<ImageChannel>());
                  }));
        public ImageChannel Channel
        {
            get => (ImageChannel)GetValue(ChannelProperty);
            set => SetValue(ChannelProperty, value);
        }

        public static readonly DependencyProperty EnableLayerMarksProperty =
              DependencyProperty.Register("EnableLayerMarks", typeof(bool), typeof(ImageViewerLayer), new PropertyMetadata(true,
                  (d, e) =>
                  {
                      if (d is ImageViewerLayer This)
                          This.OnEnableLayerMarksChanged(e.ToChangedEventArgs<bool>());
                  }));
        public bool EnableLayerMarks
        {
            get => (bool)GetValue(EnableLayerMarksProperty);
            set => SetValue(EnableLayerMarksProperty, value);
        }

        public ImageViewerLayerMarkCollection Marks { get; private set; }

        internal bool IsGeneratedFromSystem = false;

        protected internal ImageViewerBase Viewer;
        static ImageViewerLayer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewerLayer), new FrameworkPropertyMetadata(typeof(ImageViewerLayer)));
        }

        public ImageViewerLayer()
        {
            Marks = new ImageViewerLayerMarkCollection();
            Marks.MarkChanged += OnMarkDataChanged;
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

        private void OnEnableLayerMarksChanged(ChangedEventArgs<bool> e)
            => UpdateCanvas();
        private void OnMarkDataChanged(object sender, EventArgs e)
            => UpdateCanvas();

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

        protected Bound<float> LastImageBound, LastMarksBound;
        private DelayActionToken UpdateCanvasToken;
        public virtual void UpdateCanvas()
        {
            UpdateCanvasToken?.Cancel();

            if (Viewer is null)
            {
                DisplayImage = null;
                return;
            }

            bool HasImage = _SourceContext != null && _SourceContext.Width > 0 && _SourceContext.Height > 0,
                 HasMarks = Marks.Count > 0;
            if (!(HasImage || HasMarks))
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

                // Reset LastImageBound
                LastImageBound.Left = float.MaxValue;
                LastImageBound.Top = float.MaxValue;
                LastImageBound.Right = float.MinValue;
                LastImageBound.Bottom = float.MinValue;

                // Reset LastMarksBound
                LastMarksBound.Left = float.MaxValue;
                LastMarksBound.Top = float.MaxValue;
                LastMarksBound.Right = float.MinValue;
                LastMarksBound.Bottom = float.MinValue;
            }

            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                BitmapContext Display = DisplayContext;
                if (Display.TryLock(1))
                {
                    try
                    {
                        Bound<int> DirtyBound = OnDrawImage();

                        if (EnableLayerMarks && HasMarks)
                        {
                            Bound<int> DirtyMarksBound = OnDrawMarks();
                            DirtyBound.Union(DirtyMarksBound);
                        }

                        Int32Rect DirtyRect = new Int32Rect(DirtyBound.Left, DirtyBound.Top, DirtyBound.Width, DirtyBound.Height);
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
        protected virtual Bound<int> OnDrawImage()
        {
            // Check Image
            if (_SourceContext != null &&
                _SourceContext.Width > 0 &&
                _SourceContext.Height > 0)
            {
                // Check Scale
                double Scale = Viewer.InternalScale;
                if (Scale > 0d)
                {
                    Rect Viewport = Viewer.InternalViewport;
                    int SourceW = _SourceContext.Width,
                        SourceH = _SourceContext.Height,
                        ContextX = Viewer.ContextX,
                        ContextY = Viewer.ContextY,
                        ContextW = Viewer.ContextWidth,
                        ContextH = Viewer.ContextHeight;

                    // Align
                    AlignContextLocation(SourceW, SourceH, ContextW, ContextH, ref ContextX, ref ContextY);

                    // Calculate Source's Rect in ImageViewer.
                    float SourceEx = ContextX + SourceW,
                          SourceEy = ContextY + SourceH,
                          ISx = (float)((ContextX - Viewport.X) * Scale),
                          ISy = (float)((ContextY - Viewport.Y) * Scale),
                          IEx = (float)((SourceEx - Viewport.X) * Scale),
                          IEy = (float)((SourceEy - Viewport.Y) * Scale);

                    // Calculate DirtyRect (Compare with LastImageRect)
                    float DirtyX1 = Math.Max(MathHelper.Min(LastImageBound.Left, LastMarksBound.Left, ISx), 0f),
                          DirtyY1 = Math.Max(MathHelper.Min(LastImageBound.Top, LastMarksBound.Top, ISy), 0f),
                          DirtyX2 = Math.Min(MathHelper.Max(LastImageBound.Right, LastMarksBound.Right, IEx), DisplayContext.Width),
                          DirtyY2 = Math.Min(MathHelper.Max(LastImageBound.Bottom, LastMarksBound.Bottom, IEy), DisplayContext.Height);

                    int IntDirtyX1 = (int)Math.Ceiling(DirtyX1),
                        IntDirtyY1 = (int)Math.Ceiling(DirtyY1),
                        IntDirtyX2 = (int)Math.Floor(DirtyX2),
                        IntDirtyY2 = (int)Math.Floor(DirtyY2);

                    // Set LastImageBound
                    LastImageBound.Left = DirtyX1;
                    LastImageBound.Top = DirtyY1;
                    LastImageBound.Right = DirtyX2;
                    LastImageBound.Bottom = DirtyY2;

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

                    _ = Parallel.For(IntDirtyY1, IntDirtyY2, Viewer.RenderParallelOptions ?? DefaultParallelOptions, j =>
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
                                IPixelAdapter<BGRA> Adapter = _SourceContext.Operator.GetAdapter<BGRA>(X, Y);

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

                    return new Bound<int>(IntDirtyX1, IntDirtyY1, IntDirtyX2, IntDirtyY2);
                }
            }

            // Excluded that had be cleared.
            if (LastImageBound.Left == float.MaxValue &&
                LastImageBound.Top == float.MaxValue &&
                LastImageBound.Right == float.MinValue &&
                LastImageBound.Bottom == float.MinValue)
                return Bound<int>.Empty;

            // Clear
            DisplayContext.Clear(EmptyPixel, null);

            // Reset LastImageBound
            LastImageBound.Left = float.MaxValue;
            LastImageBound.Top = float.MaxValue;
            LastImageBound.Right = float.MinValue;
            LastImageBound.Bottom = float.MinValue;

            return new Bound<int>(0, 0, DisplayContext.Width, DisplayContext.Height);
        }
        protected virtual Bound<int> OnDrawMarks()
        {
            // Check Scale
            double Scale = Viewer.InternalScale;
            if (Scale > 0d)
            {
                Rect Viewport = Viewer.InternalViewport;
                int ContextX = Viewer.ContextX,
                    ContextY = Viewer.ContextY,
                    ContextW = Viewer.ContextWidth,
                    ContextH = Viewer.ContextHeight;

                double ViewportSx = Viewport.Left,
                       ViewportSy = Viewport.Top,
                       ViewportEx = Viewport.Right,
                       ViewportEy = Viewport.Bottom;

                float DirtySx = LastMarksBound.Left,
                      DirtySy = LastMarksBound.Top,
                      DirtyEx = LastMarksBound.Right,
                      DirtyEy = LastMarksBound.Bottom;

                if (_SourceContext != null)
                {
                    int SourceW = _SourceContext.Width,
                        SourceH = _SourceContext.Height;

                    // Align
                    if (SourceW > 0 &&
                        SourceH > 0)
                        AlignContextLocation(SourceW, SourceH, ContextW, ContextH, ref ContextX, ref ContextY);
                }
                else if (DirtySx != int.MaxValue &&
                         DirtySy != int.MaxValue &&
                         DirtyEx != int.MinValue &&
                         DirtyEy != int.MinValue)
                {
                    // Clear Last Marks.
                    int LSx = (int)Math.Floor(DirtySx),
                        LSy = (int)Math.Floor(DirtySy),
                        LEx = (int)Math.Ceiling(DirtyEx),
                        LEy = (int)Math.Ceiling(DirtyEy);
                    Parallel.For(LSy, LEy, DefaultParallelOptions, j =>
                    {
                        IPixelAdapter<BGRA> Adapter = DisplayContext.Context.Operator.GetAdapter<BGRA>(LSx, j);
                        for (int i = LSx; i < LEx; i++, Adapter.MoveNext())
                            Adapter.Override(EmptyPixel);
                    });
                }

                int DisplayWidth = DisplayContext.Width,
                    DisplayHeight = DisplayContext.Height;
                long DisplayStride = DisplayContext.Stride;

                try
                {
                    byte* DisplayScan0 = (byte*)DisplayContext.Scan0;
                    foreach (ImageViewerLayerMark Mark in Marks.Where(i => i.Visible))
                    {
                        if (Mark.CreateVisualContext() is IImageContext Stamp)
                        {
                            double MW = Stamp.Width,
                                   MH = Stamp.Height,
                                   HMW = MW / 2d,
                                   HMH = MH / 2d;

                            bool Zoomable = Mark.Zoomable && Scale != 1d;
                            double MarkScale = Zoomable ? Scale.Clamp(Mark.ZoomMinScale, Mark.ZoomMaxScale) : 1d;
                            if (Viewer is ImageViewerMapper Mapper)
                            {
                                Zoomable = true;
                                if (!Mark.Zoomable)
                                    MarkScale = Scale / Mapper.TargetViewer.Scale.Clamp(Mark.ZoomMinScale, Mark.ZoomMaxScale);
                            }

                            if (Zoomable)
                            {
                                float FactorStep = (float)(1 / MarkScale);

                                foreach (Point Center in Mark.CenterLocations)
                                {
                                    // Calculate Global Location
                                    double MCx = ContextX + Center.X,
                                           MCy = ContextY + Center.Y,
                                           MSx = MCx - HMW,
                                           MSy = MCy - HMH,
                                           MEx = MSx + MW,
                                           MEy = MSy + MH;

                                    if (MEx < ViewportSx || ViewportEx < MSx ||
                                        MEy < ViewportSy || ViewportEy < MSy)
                                        continue;

                                    // Calculate DisplayContext Location
                                    double Cx = (MCx - ViewportSx) * Scale,
                                           Cy = (MCy - ViewportSy) * Scale,
                                           Sx = Cx - HMW * MarkScale,
                                           Sy = Cy - HMH * MarkScale,
                                           Ex = Cx + HMW * MarkScale,
                                           Ey = Cy + HMH * MarkScale;

                                    // Calculate Bound
                                    double BSx = Math.Max(Sx, 0),
                                           BSy = Math.Max(Sy, 0),
                                           BEx = Math.Min(Ex, DisplayWidth),
                                           BEy = Math.Min(Ey, DisplayHeight);

                                    // Calculate Nearest Resize Datas
                                    float FracX0 = (float)((BSx - Sx) * FactorStep),
                                          FracX1 = (float)(FracX0 + (BEx - BSx) * FactorStep);

                                    int IntDirtySx = (int)Math.Ceiling(BSx),
                                        IntDirtySy = (int)Math.Ceiling(BSy),
                                        IntDirtyEx = (int)Math.Ceiling(BEx),
                                        IntDirtyEy = (int)Math.Ceiling(BEy),
                                        StampSx = (int)FracX0,
                                        StampEx = (int)FracX1;

                                    FracX0 -= StampSx;
                                    FracX1 -= StampEx;

                                    // Draw
                                    _ = Parallel.For(IntDirtySy, IntDirtyEy, j =>
                                    {
                                        long Offset = j * DisplayStride + IntDirtySx * sizeof(BGRA);
                                        BGRA* pData = (BGRA*)(DisplayScan0 + Offset);

                                        float FracX = FracX0;
                                        int X = StampSx,
                                            Y = (int)((j - Sy) * FactorStep);

                                        IPixelAdapter<BGRA> Adapter = Stamp.Operator.GetAdapter<BGRA>(X, Y);
                                        while (X < MW && (X < StampEx || (X == StampEx && FracX < FracX1)))
                                        {
                                            Adapter.OverlayTo(pData++);

                                            FracX += FactorStep;
                                            while (FracX >= 1f)
                                            {
                                                FracX -= 1f;
                                                Adapter.MoveNext();
                                                X++;
                                            }
                                        }
                                    });

                                    // Update Dirty
                                    DirtySx = Math.Min(IntDirtySx, DirtySx);
                                    DirtySy = Math.Min(IntDirtySy, DirtySy);
                                    DirtyEx = Math.Max(IntDirtyEx, DirtyEx);
                                    DirtyEy = Math.Max(IntDirtyEy, DirtyEy);
                                }
                            }
                            else
                            {
                                foreach (Point Center in Mark.CenterLocations)
                                {
                                    // Calculate Global Location
                                    double MCx = ContextX + Center.X,
                                           MCy = ContextY + Center.Y,
                                           MSx = MCx - HMW,
                                           MSy = MCy - HMH,
                                           MEx = MSx + MW,
                                           MEy = MSy + MH;

                                    if (MEx < ViewportSx || ViewportEx < MSx ||
                                        MEy < ViewportSy || ViewportEy < MSy)
                                        continue;

                                    // Calculate DisplayContext Location
                                    double Cx = (MCx - ViewportSx) * Scale,
                                           Cy = (MCy - ViewportSy) * Scale;
                                    int Sx = (int)Math.Round(Cx - HMW),
                                        Sy = (int)Math.Round(Cy - HMH),
                                        Ex = (int)(Sx + MW),
                                        Ey = (int)(Sy + MH);

                                    // Calculate Bound
                                    int IntDirtySx = Math.Max(Sx, 0),
                                        IntDirtySy = Math.Max(Sy, 0),
                                        IntDirtyEx = Math.Min(Ex, DisplayWidth),
                                        IntDirtyEy = Math.Min(Ey, DisplayHeight),
                                        StampSx = IntDirtySx - Sx,
                                        StampWidth = IntDirtyEx - IntDirtySx;

                                    // Draw
                                    _ = Parallel.For(IntDirtySy, IntDirtyEy, j =>
                                    {
                                        long Offset = j * DisplayStride + IntDirtySx * sizeof(BGRA);
                                        BGRA* pData = (BGRA*)(DisplayScan0 + Offset);

                                        Stamp.Operator.ScanLine<BGRA>(StampSx, j - Sy, StampWidth, Adapter => Adapter.OverlayTo(pData++));
                                    });

                                    // Update Dirty
                                    DirtySx = Math.Min(IntDirtySx, DirtySx);
                                    DirtySy = Math.Min(IntDirtySy, DirtySy);
                                    DirtyEx = Math.Max(IntDirtyEx, DirtyEx);
                                    DirtyEy = Math.Max(IntDirtyEy, DirtyEy);
                                }
                            }
                        }
                    }

                    return DirtySx == int.MaxValue &&
                           DirtySy == int.MaxValue &&
                           DirtyEx == int.MinValue &&
                           DirtyEy == int.MinValue ?
                           Bound<int>.Empty :
                           new Bound<int>((int)DirtySx, (int)DirtySy, (int)DirtyEx, (int)DirtyEy);
                }
                finally
                {
                    LastMarksBound.Left = DirtySx;
                    LastMarksBound.Top = DirtySy;
                    LastMarksBound.Right = DirtyEx;
                    LastMarksBound.Bottom = DirtyEy;
                }
            }

            // Reset LastImageBound
            LastMarksBound.Left = float.MaxValue;
            LastMarksBound.Top = float.MaxValue;
            LastMarksBound.Right = float.MinValue;
            LastMarksBound.Bottom = float.MinValue;

            return Bound<int>.Empty;
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
