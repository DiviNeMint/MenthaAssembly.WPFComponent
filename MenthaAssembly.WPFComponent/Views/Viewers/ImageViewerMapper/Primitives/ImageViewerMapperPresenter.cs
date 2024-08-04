using MenthaAssembly.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MenthaAssembly.Views.Primitives
{
    internal sealed class ImageViewerMapperPresenter : Panel<ImageViewerMapper>
    {
        private readonly Rectangle ViewportElement;
        public ImageViewerMapperPresenter(ImageViewerMapper LogicalParent) : base(LogicalParent)
        {
            _ = SetBinding(BackgroundProperty, new Binding("Viewer.Background") { Source = LogicalParent });

            ViewportElement = new Rectangle();
            SetZIndex(ViewportElement, int.MaxValue);
            _ = ViewportElement.SetBinding(UseLayoutRoundingProperty, new Binding(nameof(UseLayoutRounding)) { Source = LogicalParent });
            _ = ViewportElement.SetBinding(SnapsToDevicePixelsProperty, new Binding(nameof(SnapsToDevicePixels)) { Source = LogicalParent });
            _ = ViewportElement.SetBinding(Shape.FillProperty, new Binding(nameof(LogicalParent.Fill)) { Source = LogicalParent });
            _ = ViewportElement.SetBinding(Shape.StrokeProperty, new Binding(nameof(LogicalParent.Stroke)) { Source = LogicalParent });
            _ = ViewportElement.SetBinding(Shape.StrokeThicknessProperty, new Binding(nameof(LogicalParent.StrokeThickness)) { Source = LogicalParent });
            _ = Children.Add(ViewportElement);
        }

        public void Attach(ImageViewer Viewer)
        {
            Viewer.ViewportChanged += OnViewportChanged;
            Viewer.ViewBoxChanged += OnViewBoxChanged;

            // Layers
            Viewer.Layers.CollectionChanged += OnLayerCollectionChanged;
            foreach (ImageViewerLayer Layer in Viewer.Layers)
            {
                Attach(Layer);
                MiniLayers.Add(Layer, new MiniLayer());
            }
        }
        private void Attach(ImageViewerLayer Layer)
        {
            Layer.IsVisibleChanged += OnLayerIsVisibleChanged;
            Layer.AlignmentChanged += OnLayerAlignmentChanged;
            Layer.Renderer.MarksChanged += OnLayerMarksChanged;
            Layer.Renderer.ThumbnailChanged += OnLayerThumbnailChanged;
        }

        public void Detach(ImageViewer Viewer)
        {
            Viewer.ViewportChanged -= OnViewportChanged;
            Viewer.ViewBoxChanged -= OnViewBoxChanged;

            // Layers
            Viewer.Layers.CollectionChanged -= OnLayerCollectionChanged;
            foreach (ImageViewerLayer Layer in Viewer.Layers)
                Detach(Layer);

            MiniLayers.Clear();
        }
        private void Detach(ImageViewerLayer Layer)
        {
            Layer.IsVisibleChanged -= OnLayerIsVisibleChanged;
            Layer.AlignmentChanged -= OnLayerAlignmentChanged;
            Layer.Renderer.MarksChanged -= OnLayerMarksChanged;
            Layer.Renderer.ThumbnailChanged -= OnLayerThumbnailChanged;
        }

        private void OnViewBoxChanged(object sender, ChangedEventArgs<Size<int>> e)
            => InvalidateViewBox();
        private void OnViewportChanged(object sender, RoutedPropertyChangedEventArgs<Rect> e)
            => InvalidateViewport();

        private readonly Dictionary<ImageViewerLayer, MiniLayer> MiniLayers = [];
        private void OnLayerCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (ImageViewerLayer Layer in e.NewItems.OfType<ImageViewerLayer>())
                        {
                            Attach(Layer);
                            MiniLayers.Add(Layer, new MiniLayer());
                        }

                        InvalidateCanvas();
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (ImageViewerLayer Layer in e.OldItems.OfType<ImageViewerLayer>())
                        {
                            Detach(Layer);
                            MiniLayers.Remove(Layer);
                        }

                        InvalidateVisual();
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (ImageViewerLayer Layer in e.OldItems.OfType<ImageViewerLayer>())
                        {
                            Detach(Layer);
                            MiniLayers.Remove(Layer);
                        }

                        foreach (ImageViewerLayer Layer in e.NewItems.OfType<ImageViewerLayer>())
                        {
                            Attach(Layer);
                            MiniLayers.Add(Layer, new MiniLayer());
                        }

                        InvalidateCanvas();
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (ImageViewerLayer Layer in MiniLayers.Keys)
                            Detach(Layer);

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

        private void OnLayerIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            => InvalidateCanvas();
        private void OnLayerAlignmentChanged(object sender, EventArgs e)
            => InvalidateCanvas();
        private void OnLayerMarksChanged(object sender, EventArgs e)
            => InvalidateMarks();
        private void OnLayerThumbnailChanged(object sender, EventArgs e)
            => InvalidateCanvas();

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

            // Skips ViewportElement by start index 0.
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
                ViewportElement.Arrange(Viewport.IsEmpty ? new Rect() : new Rect(Viewport.X * Scale, Viewport.Y * Scale, Viewport.Width * Scale, Viewport.Height * Scale));

                // Skips ViewportElement by start index 0.
                Rect Rect = new(FinalSize);
                int Count = Children.Count;
                for (int i = 1; i < Count; i++)
                    Children[i].Arrange(Rect);
            }
            else
            {
                ViewportElement.Arrange(new Rect());
            }

            return DesiredSize;
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
                Dispatcher.BeginInvoke(new Action(Refresh), DispatcherPriority.Render);
#else
                Dispatcher.BeginInvoke(Refresh, DispatcherPriority.Render);
#endif
        }

        private int ViewerHashCode = 0;
        private void Refresh()
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
                Viewer.Scale != Viewer.FitScale)
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
                Viewer.Scale != Viewer.FitScale)
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

            private int ImageHashCode = 0;
            public double Ix, Iy, Scale;
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
                WriteableBitmap Thumbnail = Layer.Renderer.Thumbnail;
                bool NewImage = false;
                int ImageHashCode = Thumbnail?.GetHashCode() ?? 0;
                if (this.ImageHashCode != ImageHashCode)
                {
                    NewImage = true;
                    this.Image = Thumbnail;
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

        }

    }
}