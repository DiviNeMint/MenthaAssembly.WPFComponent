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

                    if (NewViewer || IsRefresh)
                        InvalidateVisual();
                }

                else if (ViewerHashCode != 0)
                {
                    ViewerHashCode = 0;
                    InvalidateVisual();
                }
            }
            finally
            {
                IsCanvasValid = true;
            }
        }

        protected override void OnRender(DrawingContext Context)
        {
            base.OnRender(Context);

            foreach (MiniLayer Layer in MiniLayers.Values.Where(i => i.IsValid))
                Context.DrawImage(Layer.Image, Layer.Region);
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
                    bool IsVisibleChanged = IsVisible != Layer.IsVisible;
                    if (IsVisibleChanged)
                    {
                        IsVisible = Layer.IsVisible;
                        if (!IsVisible)
                            return true;
                    }

                    if (Layer.Source is ImageSource Image)
                        return PrepareImageSource(Viewer, Layer, Image, Scale) || IsVisibleChanged;

                    else if (Layer.SourceContext is IImageContext ImageContext)
                        return PrepareImageContext(Viewer, Layer, ImageContext, Scale) || IsVisibleChanged;

                    bool IsRefresh = this.Image != null;
                    this.Image = null;
                    return IsVisibleChanged || IsRefresh;
                }
                finally
                {
                    IsPreparing = false;
                }
            }

            private int ImageHashCode = 0;
            private double Ix, Iy, Scale;
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
            private bool PrepareImageContext(ImageViewer Viewer, ImageViewerLayer Layer, IImageContext Image, double Scale)
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
                    Region.Width = Math.Round(Iw * Scale);
                    Region.Height = Math.Round(Ih * Scale);
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

                Iw = Region.Width;
                Ih = Region.Height;

                if (NewImage ||
                    Image is null ||
                    this.Image.Width < Iw ||
                    this.Image.Height < Ih)
                    this.Image = CreateImage(Image, (int)Iw, (int)Ih);

                return NewImage || NewScale || NewLocation;
            }

            private unsafe WriteableBitmap CreateImage(IImageContext ImageContext, int Iw, int Ih)
            {
                WriteableBitmap Bitmap = new(Iw, Ih, 96d, 96d, PixelFormats.Bgra32, null);
                Bitmap.Lock();
                try
                {
                    NearestResizePixelAdapter<BGRA> Adapter0 = new(ImageContext, Iw, Ih);
                    byte* pDest0 = (byte*)Bitmap.BackBuffer;
                    long Stride = Bitmap.BackBufferStride;

                    _ = Parallel.For(0, Ih, j =>
                    {
                        PixelAdapter<BGRA> Adapter = Adapter0.Clone();
                        Adapter.InternalMove(0, j);

                        BGRA* pDest = (BGRA*)(pDest0 + j * Stride);
                        for (int i = 0; i < Iw; i++, Adapter.InternalMoveNext())
                            Adapter.OverlayTo(pDest++);
                    });
                }
                finally
                {
                    Bitmap.AddDirtyRect(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight));
                    Bitmap.Unlock();
                }

                return Bitmap;
            }

        }

    }
}