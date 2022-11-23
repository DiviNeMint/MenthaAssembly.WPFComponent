using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Views.Primitives;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(Elements))]
    public class ImageViewerLayer : FrameworkElement
    {
        internal event EventHandler<ImageViewerLayer> StatusChanged;

        public static readonly RoutedEvent SourceChangedEvent =
            EventManager.RegisterRoutedEvent("SourceChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<ImageSource>), typeof(ImageViewerLayer));
        public event RoutedPropertyChangedEventHandler<ImageSource> SourceChanged
        {
            add => AddHandler(SourceChangedEvent, value);
            remove => RemoveHandler(SourceChangedEvent, value);
        }

        public static readonly RoutedEvent SourceContextChangedEvent =
            EventManager.RegisterRoutedEvent("SourceContextChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<IImageContext>), typeof(ImageViewerLayer));
        public event RoutedPropertyChangedEventHandler<IImageContext> SourceContextChanged
        {
            add => AddHandler(SourceContextChangedEvent, value);
            remove => RemoveHandler(SourceContextChangedEvent, value);
        }

        public static readonly DependencyProperty SourceProperty =
            Image.SourceProperty.AddOwner(typeof(ImageViewerLayer), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is ImageViewerLayer This && !This.IsContextChanging)
                        This.OnSourceChanged(e.ToRoutedPropertyChangedEventArgs<ImageSource>(SourceChangedEvent));
                }));
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceContextProperty =
            DependencyProperty.Register("SourceContext", typeof(IImageContext), typeof(ImageViewerLayer), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is ImageViewerLayer This && !This.IsContextChanging)
                        This.OnSourceContextChanged(e.ToRoutedPropertyChangedEventArgs<IImageContext>(SourceContextChangedEvent));
                }));
        public IImageContext SourceContext
        {
            get => (IImageContext)GetValue(SourceContextProperty);
            set => SetValue(SourceContextProperty, value);
        }

        public ImageViewerLayerMarkCollection Marks { get; }

        public ImageViewerLayerElementCollection Elements { get; }

        internal readonly ImageViewerLayerRenderer Renderer;
        private readonly ImageViewerLayerPresenter TemplatePresenter;
        internal readonly bool IsGeneratedFromCollection;
        internal ImageViewerLayer(bool GeneratedFromCollection)
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

            IsGeneratedFromCollection = GeneratedFromCollection;
            Renderer = new ImageViewerLayerRenderer(this);
            TemplatePresenter = new ImageViewerLayerPresenter(this);
            AddVisualChild(TemplatePresenter);

            Marks = new ImageViewerLayerMarkCollection(this);
            Elements = new ImageViewerLayerElementCollection(this, TemplatePresenter.Children);

            Type ThisType = GetType();
            DependencyPropertyDescriptor.FromProperty(VerticalAlignmentProperty, ThisType).AddValueChanged(this, OnAlignmentChanged);
            DependencyPropertyDescriptor.FromProperty(HorizontalAlignmentProperty, ThisType).AddValueChanged(this, OnAlignmentChanged);

            IsVisibleChanged += OnVisibleChanged;
        }
        public ImageViewerLayer() : this(false)
        {
        }

        protected override int VisualChildrenCount
            => 1;
        protected override Visual GetVisualChild(int Index)
            => TemplatePresenter;

        private bool IsContextChanging = false;
        private void OnSourceChanged(RoutedPropertyChangedEventArgs<ImageSource> e)
        {
            try
            {
                IsContextChanging = true;

                if (SourceContext is IImageContext Context)
                {
                    ClearValue(SourceContextProperty);
                    InvalidateContextSize(Context.Width, Context.Height);
                }

                else if (e.OldValue is ImageSource OldImage)
                    InvalidateContextSize(OldImage.Width, OldImage.Height);

                else if (e.NewValue is ImageSource Image)
                    InvalidateContextSize(Image.Width, Image.Height);

                RaiseEvent(e);
            }
            finally
            {
                IsContextChanging = false;
            }
        }
        private void OnSourceContextChanged(RoutedPropertyChangedEventArgs<IImageContext> e)
        {
            try
            {
                IsContextChanging = true;
                if (Source is ImageSource Image)
                {
                    ClearValue(SourceProperty);
                    InvalidateContextSize(Image.Width, Image.Height);
                }

                else if (e.OldValue is IImageContext OldContext)
                    InvalidateContextSize(OldContext.Width, OldContext.Height);

                else if (e.NewValue is IImageContext Context)
                    InvalidateContextSize(Context.Width, Context.Height);

                RaiseEvent(e);
            }
            finally
            {
                IsContextChanging = false;
            }
        }

        protected virtual void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Source is ImageSource Image)
                InvalidateContextSize(Image.Width, Image.Height);

            if (Source is IImageContext Context)
                InvalidateContextSize(Context.Width, Context.Height);

            else
                InvalidateCanvas();

            OnStatusChanged();
        }
        protected virtual void OnAlignmentChanged(object sender, EventArgs e)
        {
            if (Parent is ImageViewer)
                InvalidateCanvas();

            OnStatusChanged();
        }
        protected virtual void OnStatusChanged()
            => StatusChanged?.Invoke(Parent, this);
        protected override void OnRenderSizeChanged(SizeChangedInfo SizeInfo)
        {
            if (Parent is not ImageViewer &&
                (Source != null || SourceContext != null))
                InvalidateCanvas();

            base.OnRenderSizeChanged(SizeInfo);
        }

        protected override Size MeasureOverride(Size AvailableSize)
        {
            TemplatePresenter.Measure(AvailableSize);
            return AvailableSize;
        }
        protected override Size ArrangeOverride(Size FinalSize)
        {
            TemplatePresenter.Arrange(new Rect(FinalSize));
            return base.ArrangeOverride(FinalSize);
        }

        protected virtual void InvalidateContextSize(double Width, double Height)
        {
            if (Parent is ImageViewer Viewer)
            {
                double Cw = Viewer.ContextWidth,
                       Ch = Viewer.ContextHeight;
                if (IsVisible)
                {
                    if (Width < Cw && Height < Ch)
                        InvalidateCanvas();
                    else
                        Viewer.Manager.Add(ImageViewerAction.ContextSize);
                }
                else if (Cw == Width || Ch == Height)
                {
                    Viewer.Manager.Add(ImageViewerAction.ContextSize);
                }
            }

            else if (IsVisible)
            {
                InvalidateCanvas();
            }
        }

        /// <summary>
        /// Invalidate the canvas.
        /// After invalidation, the canvas will update its rendering, which will happen asynchronously.
        /// </summary>
        public virtual void InvalidateCanvas()
        {
            Renderer.Invalidate();
            TemplatePresenter.InvalidateArrange();
        }

        /// <summary>
        /// Invalidate the marks.
        /// After invalidation, the marks will update its rendering, which will happen asynchronously.
        /// </summary>
        public virtual void InvalidateMarks()
            => Renderer.InvalidateMarks();

        protected override void OnRender(DrawingContext Context)
            => Renderer.Render(Context);

        /// <summary>
        /// Gets the image coordination of the mouse position in this layer.
        /// </summary>
        public Point GetImageCoordination()
        {
            Point Position = Mouse.GetPosition(this);
            return GetImageCoordination(Position.X, Position.Y);
        }
        /// <summary>
        /// Gets the image coordination of the specified position in this layer.
        /// </summary>
        /// <param name="Lx">The x-coordinate of the specified position in this layer.</param>
        /// <param name="Ly">The y-coordinate of the specified position in this layer.</param>
        public Point GetImageCoordination(double Lx, double Ly)
        {
            Renderer.GetImageCoordination(Lx, Ly, out double Ix, out double Iy);
            return new Point(Ix, Iy);
        }

        /// <summary>
        /// Gets the global image coordination of the mouse position in this layer.
        /// </summary>
        public Point GetGlobalImageCoordination()
        {
            Point Position = Mouse.GetPosition(this);
            return GetGlobalImageCoordination(Position.X, Position.Y);
        }
        /// <summary>
        /// Gets the global image coordination of the specified position in this layer.
        /// </summary>
        /// <param name="Lx">The x-coordinate of the specified position in this layer.</param>
        /// <param name="Ly">The y-coordinate of the specified position in this layer.</param>
        public Point GetGlobalImageCoordination(double Lx, double Ly)
        {
            Renderer.GetGlobalImageCoordination(Lx, Ly, out double Ix, out double Iy);
            return new Point(Ix, Iy);
        }

        /// <summary>
        /// Gets the position in this layer of the specified image coordination.
        /// </summary>
        /// <param name="Ix">The x-coordinate of the specified image coordination.</param>
        /// <param name="Iy">The y-coordinate of the specified image coordination.</param>
        public Point GetLayerPosition(double Ix, double Iy)
        {
            Renderer.GetLayerPosition(Ix, Iy, out double Lx, out double Ly);
            return new Point(Lx, Ly);
        }

    }
}