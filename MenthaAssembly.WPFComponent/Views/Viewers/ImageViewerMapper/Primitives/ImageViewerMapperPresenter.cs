using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MenthaAssembly.Views.Primitives
{
    internal sealed class ImageViewerMapperPresenter : Panel<ImageViewerMapper>, IImageViewerAttachment
    {
        private readonly Rectangle ViewportRect;
        public ImageViewerMapperPresenter(ImageViewerMapper LogicalParent) : base(LogicalParent)
        {
            _ = SetBinding(BackgroundProperty, new Binding("Viewer.Background") { Source = LogicalParent });

            ViewportRect = new Rectangle();
            SetZIndex(ViewportRect, int.MaxValue);
            _ = ViewportRect.SetBinding(Shape.StrokeProperty, new Binding(nameof(LogicalParent.ViewportStroke)) { Source = LogicalParent });
            _ = ViewportRect.SetBinding(Shape.FillProperty, new Binding(nameof(LogicalParent.ViewportFill)) { Source = LogicalParent });
            _ = Children.Add(ViewportRect);
        }

        private double Scale = double.NaN;
        protected override Size MeasureOverride(Size AvailableSize)
        {
            double Lw = 0d,
                   Lh = 0d;
            if (LogicalParent.Viewer is ImageViewer Viewer)
            {
                Size<int> ViewBox = Viewer.ViewBox;
                if (!ViewBox.IsEmpty)
                {
                    bool NewScale;
                    double ScaleX = AvailableSize.Width / ViewBox.Width,
                           ScaleY = AvailableSize.Height / ViewBox.Height;
                    if (ScaleX < ScaleY)
                    {
                        NewScale = Scale != ScaleX;
                        Scale = ScaleX;
                        Lw = AvailableSize.Width;
                        Lh = ViewBox.Height * ScaleX;
                    }
                    else
                    {
                        NewScale = Scale != ScaleY;
                        Scale = ScaleY;
                        Lw = ViewBox.Width * ScaleY;
                        Lh = ViewBox.Height;
                    }

                    if (NewScale)
                        InvalidateCanvas();
                }
            }

            AvailableSize = new Size(Lw, Lh);

            // Skips ViewportRect by start index 0.
            int Count = Children.Count;
            for (int i = 1; i < Count; i++)
                Children[i].Measure(AvailableSize);

            return AvailableSize;
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            if (!double.IsNaN(Scale) &&
                LogicalParent.Viewer is ImageViewer Viewer)
            {
                Rect Viewport = Viewer.Viewport;
                ViewportRect.Arrange(Viewport.IsEmpty ? new Rect() : new Rect(Viewport.X * Scale, Viewport.Y * Scale, Viewport.Width * Scale, Viewport.Height * Scale));

                // Skips ViewportRect by start index 0.
                Rect Rect = new(FinalSize);
                int Count = Children.Count;
                for (int i = 1; i < Count; i++)
                    Children[i].Arrange(Rect);
            }

            return DesiredSize;
        }

        private readonly Dictionary<ImageViewerLayer, MiniLayer> MiniLayers = new();
        public void OnLayerCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (ImageViewerLayer Layer in e.NewItems.OfType<ImageViewerLayer>())
                            MiniLayers.Add(Layer, new MiniLayer());

                        InvalidateCanvas();
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (ImageViewerLayer Layer in e.OldItems.OfType<ImageViewerLayer>())
                            MiniLayers.Remove(Layer);

                        InvalidateVisual();
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (ImageViewerLayer Layer in e.OldItems.OfType<ImageViewerLayer>())
                            MiniLayers.Remove(Layer);

                        foreach (ImageViewerLayer Layer in e.NewItems.OfType<ImageViewerLayer>())
                            MiniLayers.Add(Layer, new MiniLayer());

                        InvalidateCanvas();
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        MiniLayers.Clear();
                        InvalidateVisual();
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                default:
                    InvalidateVisual();
                    break;
            }
        }

        public void InvalidateViewBox()
        {
            InvalidateMeasure();
            InvalidateArrange();
        }

        public void InvalidateViewport()
            => InvalidateArrange();

        private bool IsMarksValid = true;
        public void InvalidateMarks()
        {
            if (IsMarksValid)
            {
                IsMarksValid = false;
                InvalidateCanvas();
            }
        }

        private bool IsCanvasValid = true;
        public void InvalidateCanvas()
        {
            if (!double.IsNaN(Scale) && IsCanvasValid)
#if NET462
                Dispatcher.BeginInvoke(new Action(UpdateCanvas), DispatcherPriority.Render);
#else
                Dispatcher.BeginInvoke(UpdateCanvas, DispatcherPriority.Render);
#endif
        }

        private int ViewerHashCode = 0;
        private void UpdateCanvas()
        {
            IsCanvasValid = false;

            try
            {
                if (LogicalParent.Viewer is ImageViewer Viewer)
                {
                    bool NewViewer = false;
                    int ViewerHashCode = Viewer.GetHashCode();
                    if (this.ViewerHashCode != ViewerHashCode)
                    {
                        NewViewer = true;
                        this.ViewerHashCode = ViewerHashCode;
                        MiniLayers.Clear();
                    }

                    bool IsRefresh = false;
                    foreach (ImageViewerLayer Layer in Viewer.Layers)
                    {
                        if (!MiniLayers.TryGetValue(Layer, out MiniLayer Cache))
                        {
                            Cache = new MiniLayer();
                            MiniLayers.Add(Layer, Cache);
                        }

                        if (Cache.Prepare(Viewer, Layer, Scale))
                            IsRefresh = true;
                    }

                    if (NewViewer || IsRefresh || !IsMarksValid)
                        InvalidateVisual();
                }

                else if (ViewerHashCode != 0)
                {
                    ViewerHashCode = 0;
                    MiniLayers.Clear();
                    InvalidateVisual();
                }
            }
            finally
            {
                IsMarksValid = true;
                IsCanvasValid = true;
            }
        }

        protected override void OnRender(DrawingContext Context)
        {
            base.OnRender(Context);

            foreach (MiniLayer Layer in MiniLayers.Values.Where(i => i.IsValid))
            {
                Context.DrawImage(Layer.Image, Layer.Region);

                if (Layer.Marks.FirstOrDefault() != null)
                {
                    double Ix = Layer.Ix * Scale,
                           Iy = Layer.Iy * Scale;
                    foreach (ImageViewerLayerMark Mark in Layer.Marks)
                    {
                        ImageSource Visual = Mark.GetVisual();
                        double Iw = Visual.Width * Scale,
                               Ih = Visual.Height * Scale,
                               Dx = Ix - Iw / 2d,
                               Dy = Iy - Ih / 2d;

                        Rect Dirty = new(double.NaN, double.NaN, Iw, Ih);
                        Brush Brush = Mark.GetBrush();
                        foreach (Point Location in Mark.Locations)
                        {
                            Dirty.X = Location.X * Scale + Dx;
                            Dirty.Y = Location.Y * Scale + Dy;
                            Context.DrawRectangle(Brush, null, Dirty);
                        }
                    }
                }
            }

        }

        private bool IsLeftMouseDown = false;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.LeftButton == MouseButtonState.Pressed &&
                LogicalParent.Viewer is ImageViewer Viewer &&
                Viewer.Scale != Viewer.MinScale)
            {
                CaptureMouse();
                IsLeftMouseDown = true;

                Point Position = e.GetPosition(this);
                Viewer.MoveTo(Position.X / Scale - Viewer.ContextX, Position.Y / Scale - Viewer.ContextY);

                e.Handled = true;
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsLeftMouseDown &&
                LogicalParent.Viewer is ImageViewer Viewer &&
                Viewer.Scale != Viewer.MinScale)
            {
                Point Position = e.GetPosition(this);
                Viewer.MoveTo(Position.X / Scale - Viewer.ContextX, Position.Y / Scale - Viewer.ContextY);

                e.Handled = true;
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (IsLeftMouseDown)
            {
                ReleaseMouseCapture();
                IsLeftMouseDown = false;
            }
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (LogicalParent.Viewer is ImageViewer Viewer)
            {
                Viewer.Zoom(e.Delta > 0, Viewer.ActualWidth / 2d, Viewer.ActualHeight / 2d);
                e.Handled = true;
            }
        }

        private class MiniLayer
        {
            public ImageSource Image;

            public Rect Region;

            public bool IsVisible;

            public bool IsValid
                => Image != null;

            public IEnumerable<ImageViewerLayerMark> Marks;

            public MiniLayer()
            {
                Ix = Iy = Scale = double.NaN;
            }

            private bool IsPreparing;
            public bool Prepare(ImageViewer Viewer, ImageViewerLayer Layer, double Scale)
            {
                if (IsPreparing)
                    return false;

                try
                {
                    IsPreparing = true;

                    bool NewMarks = false;
                    if (Marks != Layer.Marks)
                    {
                        NewMarks = true;
                        Marks = Layer.Marks;
                    }

                    bool IsVisibleChanged = IsVisible != Layer.IsVisible;
                    if (IsVisibleChanged)
                    {
                        IsVisible = Layer.IsVisible;
                        if (!IsVisible)
                            return true;
                    }

                    bool IsRefresh;
                    if (Layer.Source is ImageSource Image)
                        IsRefresh = PrepareImageSource(Viewer, Layer, Image, Scale);

                    else if (Layer.SourceContext is IImageContext ImageContext)
                        IsRefresh = PrepareImageContext(Viewer, Layer, ImageContext, Scale);

                    else
                    {
                        IsRefresh = this.Image != null;
                        this.Image = null;
                        ImageHashCode = 0;
                    }

                    return IsRefresh || NewMarks || IsVisibleChanged;
                }
                finally
                {
                    IsPreparing = false;
                }
            }

            private int ImageHashCode = 0;
            public double Ix, Iy, Scale;
            private bool PrepareImageSource(ImageViewer Viewer, ImageViewerLayer Layer, ImageSource Image, double Scale)
            {
                bool NewImage = false;
                int ImageHashCode = Image.GetHashCode();
                if (this.ImageHashCode != ImageHashCode)
                {
                    NewImage = true;
                    this.Image = Image;
                    this.ImageHashCode = ImageHashCode;
                }

                bool NewScale = false;
                double Iw = Image.Width,
                       Ih = Image.Height;
                if (this.Scale != Scale)
                {
                    NewScale = true;
                    this.Scale = Scale;
                    Region.Width = Iw * Scale;
                    Region.Height = Ih * Scale;
                }

                bool NewLocation = false;
                double Ix = Viewer.ContextX,
                       Iy = Viewer.ContextY;
                ImageViewerLayerRenderer.AlignContextLocation(Viewer, Layer, Iw, Ih, ref Ix, ref Iy);
                if (this.Ix != Ix || this.Iy != Iy)
                {
                    NewLocation = true;
                    this.Ix = Ix;
                    this.Iy = Iy;

                    Region.X = Ix * Scale;
                    Region.Y = Iy * Scale;
                }

                return NewImage || NewScale || NewLocation;
            }
            private unsafe bool PrepareImageContext(ImageViewer Viewer, ImageViewerLayer Layer, IImageContext Image, double Scale)
            {
                bool NewImage = false;
                int ImageHashCode = Image.GetHashCode();
                if (this.ImageHashCode != ImageHashCode)
                {
                    NewImage = true;
                    this.ImageHashCode = ImageHashCode;
                }

                bool NewScale = false;
                double Iw = Image.Width,
                       Ih = Image.Height;
                if (this.Scale != Scale)
                {
                    NewScale = true;
                    this.Scale = Scale;
                }

                bool NewLocation = false;
                double Ix = Viewer.ContextX,
                       Iy = Viewer.ContextY;
                ImageViewerLayerRenderer.AlignContextLocation(Viewer, Layer, Iw, Ih, ref Ix, ref Iy);
                if (NewScale || this.Ix != Ix || this.Iy != Iy)
                {
                    NewLocation = true;
                    this.Ix = Ix;
                    this.Iy = Iy;

                    Region.X = Ix * Scale;
                    Region.Y = Iy * Scale;
                }

                bool NewSize = false;
                Iw = Math.Round(Iw * Scale);
                Ih = Math.Round(Ih * Scale);
                if (Iw != Region.Width || Ih != Region.Height)
                {
                    NewSize = true;
                    Region.Width = Iw;
                    Region.Height = Ih;
                }

                if (this.Image is null || NewImage || NewSize)
                {
                    int IntIw = (int)Iw,
                        IntIh = (int)Ih;
                    if (GetCanvas(IntIw, IntIh) is WriteableBitmap Canvas &&
                        TryLockCanvas(Canvas))
                    {
                        try
                        {
                            NearestResizePixelAdapter<BGRA> Adapter0 = new(Image, IntIw, IntIh);
                            byte* pDest0 = (byte*)Canvas.BackBuffer;
                            long Stride = Canvas.BackBufferStride;

                            _ = Parallel.For(0, IntIh, j =>
                            {
                                PixelAdapter<BGRA> Adapter = Adapter0.Clone();
                                Adapter.DangerousMove(0, j);

                                BGRA* pDest = (BGRA*)(pDest0 + j * Stride);
                                for (int i = 0; i < IntIw; i++, Adapter.DangerousMoveNextX())
                                    Adapter.OverlayTo(pDest++);
                            });
                        }
                        finally
                        {
                            Canvas.AddDirtyRect(new Int32Rect(0, 0, IntIw, IntIh));
                            Canvas.Unlock();
                        }

                        this.Image = Canvas;
                        return true;
                    }
                }

                return NewLocation;
            }

            private WriteableBitmap LastCanvas;
            private WriteableBitmap GetCanvas(int Iw, int Ih)
            {
                if (Iw == 0 || Ih == 0)
                    return null;

                if (LastCanvas is WriteableBitmap Canvas &&
                    Canvas.PixelWidth == Iw &&
                    Canvas.PixelHeight == Ih)
                    return Canvas;

                LastCanvas = new WriteableBitmap(Iw, Ih, 96d, 96d, PixelFormats.Bgra32, null);
                return LastCanvas;
            }
            private bool TryLockCanvas(WriteableBitmap Canvas)
            {
                for (int t = 0; t < 3; t++)
                {
                    try
                    {
                        Canvas.Lock();
                        return true;
                    }
                    catch
                    {
                    }
                }

                return false;
            }

        }

    }
}