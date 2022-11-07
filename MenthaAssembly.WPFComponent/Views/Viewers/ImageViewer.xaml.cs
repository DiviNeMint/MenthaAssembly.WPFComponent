using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Views.Primitives;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(Layers))]
    public sealed class ImageViewer : ImageViewerBase
    {
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageViewer));
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public event EventHandler<ChangedEventArgs<IEnumerable>> ItemsSourceChanged;

        public event EventHandler<ChangedEventArgs<Size<int>>> ViewBoxChanged;

        public event EventHandler<ChangedEventArgs<Rect>> ViewportChanged;

        public static readonly DependencyProperty ItemsSourceProperty =
              ItemsControl.ItemsSourceProperty.AddOwner(typeof(ImageViewer), new FrameworkPropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is ImageViewer This)
                          This.OnItemsSourceChanged(e.ToChangedEventArgs<IEnumerable>());
                  }));
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ImageViewerLayerCollection Layers { get; }

        public static readonly DependencyProperty ScaleProperty =
              DependencyProperty.Register("Scale", typeof(double), typeof(ImageViewer), new PropertyMetadata(-1d,
                  (d, e) =>
                  {
                      if (d is ImageViewer This)
                          This.OnScaleChanged(new ChangedEventArgs<double>(e.OldValue, e.NewValue));
                  },
                  (d, v) => v is double Value ? Value <= 0 ? -1d : Value : DependencyProperty.UnsetValue));
        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty MaxScaleProperty =
                DependencyProperty.Register("MaxScale", typeof(double), typeof(ImageViewer), new PropertyMetadata(1d,
                    (d, e) =>
                    {
                        if (d is ImageViewer This &&
                            e.NewValue is double NewValue &&
                            This.Scale > NewValue)
                            This.Scale = NewValue;
                    }));
        public double MaxScale
        {
            get => (double)GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, value);
        }

        public static readonly DependencyProperty ScaleRatioProperty =
              DependencyProperty.Register("ScaleRatio", typeof(double), typeof(ImageViewer), new PropertyMetadata(2d));
        public double ScaleRatio
        {
            get => (double)GetValue(ScaleRatioProperty);
            set => SetValue(ScaleRatioProperty, value);
        }

        private double MinScale;

        public static readonly DependencyProperty ViewportProperty =
              DependencyProperty.Register("Viewport", typeof(Rect), typeof(ImageViewer), new PropertyMetadata(Rect.Empty,
                  (d, e) =>
                  {
                      if (d is ImageViewer This)
                          This.OnViewportChanged(new ChangedEventArgs<Rect>(e.OldValue, e.NewValue));
                  }));
        public Rect Viewport
        {
            get => (Rect)GetValue(ViewportProperty);
            set => SetValue(ViewportProperty, value);
        }

        private Size<int> _ViewBox;
        internal override Size<int> ViewBox
        {
            get => _ViewBox;
            set
            {
                ChangedEventArgs<Size<int>> e = new ChangedEventArgs<Size<int>>(_ViewBox, value);
                _ViewBox = value;
                OnViewBoxChanged(e);
            }
        }

        static ImageViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewer), new FrameworkPropertyMetadata(typeof(ImageViewer)));
        }

        public ImageViewer()
        {
            Layers = new ImageViewerLayerCollection(this);
        }

        internal Panel PART_Container;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Container") is Panel PART_Container)
            {
                this.PART_Container = PART_Container;

                foreach (ImageViewerLayer Layer in Layers)
                    PART_Container.Children.Add(Layer);
            }

            Layers.CollectionChanged += OnLayersCollectionChanged;
            Layers.LayerSourceChanged += OnLayersSourceChanged;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo Info)
        {
            base.OnRenderSizeChanged(Info);

            if (!IsLoaded)
            {
                Size PrevSize = Info.PreviousSize;
                if (PrevSize.Width == 0 && PrevSize.Height == 0)
                    UpdateViewBoxAndContextInfo();
            }
            else if (DisplayAreaWidth > 0 && DisplayAreaHeight > 0)
            {
                IsResizeViewer = true;
                Resize_ViewportCenterInImage = new Point(Viewport.X + Viewport.Width / 2 - ContextX,
                                                         Viewport.Y + Viewport.Height / 2 - ContextY);

                // Update ViewBox
                Size<int> ViewBox = CalculateViewBox();
                if (ViewBox.Equals(this.ViewBox))
                    OnViewBoxChanged(null);
                else
                    this.ViewBox = ViewBox;

                IsResizeViewer = false;
            }
        }

        private void OnLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        bool StepUpdate = true;
                        foreach (ImageViewerLayer Layer in e.NewItems.OfType<ImageViewerLayer>())
                        {
                            PART_Container.Children.Add(Layer);

                            if (StepUpdate)
                            {
                                IImageContext Image = Layer.SourceContext;
                                if (Image.Width < ContextWidth && Image.Height < ContextHeight)
                                    Layer.UpdateCanvas();
                                else
                                    StepUpdate = false;
                            }
                        }

                        if (!StepUpdate)
                            UpdateViewBoxAndContextInfo();

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        bool StepUpdate = true;
                        foreach (ImageViewerLayer Layer in e.OldItems.OfType<ImageViewerLayer>())
                        {
                            PART_Container.Children.Remove(Layer);

                            if (StepUpdate)
                            {
                                IImageContext Image = Layer.SourceContext;
                                if (Image.Width == ContextWidth || Image.Height == ContextHeight)
                                    StepUpdate = false;
                            }
                        }

                        if (!StepUpdate)
                            UpdateViewBoxAndContextInfo();

                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        bool StepUpdate = true;
                        foreach (ImageViewerLayer Layer in e.OldItems.OfType<ImageViewerLayer>())
                        {
                            PART_Container.Children.Remove(Layer);

                            if (StepUpdate)
                            {
                                IImageContext Image = Layer.SourceContext;
                                if (Image.Width == ContextWidth || Image.Height == ContextHeight)
                                    StepUpdate = false;
                            }
                        }

                        foreach (ImageViewerLayer Layer in e.NewItems.OfType<ImageViewerLayer>())
                        {
                            PART_Container.Children.Add(Layer);

                            if (StepUpdate)
                            {
                                IImageContext Image = Layer.SourceContext;
                                if (Image.Width < ContextWidth && Image.Height < ContextHeight)
                                    Layer.UpdateCanvas();
                                else
                                    StepUpdate = false;
                            }
                        }

                        if (!StepUpdate)
                            UpdateViewBoxAndContextInfo();

                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        PART_Container.Children.Clear();
                        UpdateViewBoxAndContextInfo();
                        break;
                    }
            }
        }
        private void OnLayersSourceChanged(object sender, ChangedEventArgs<IImageContext> e)
        {
            if (ContextWidth == 0 || ContextHeight == 0)
            {
                UpdateViewBoxAndContextInfo();
                return;
            }

            IImageContext Image = e.NewValue;
            if (Image.Width >= ContextWidth || Image.Height >= ContextHeight)
            {
                UpdateViewBoxAndContextInfo();
                return;
            }

            Image = e.OldValue;
            if (Image.Width >= ContextWidth || Image.Height >= ContextHeight)
            {
                UpdateViewBoxAndContextInfo();
                return;
            }

            if (sender is ImageViewerLayer Layer)
                Layer.UpdateCanvas();
        }

        private void OnItemsSourceChanged(ChangedEventArgs<IEnumerable> e)
        {
            Layers.SetItemsSource(e.NewValue);
            ItemsSourceChanged?.Invoke(this, e);
            UpdateViewBoxAndContextInfo();
        }

        private void OnViewBoxChanged(ChangedEventArgs<Size<int>> e)
        {
            if (e != null)
            {
                UpdateContextLocation();
                ViewBoxChanged?.Invoke(this, e);
            }

            // Update Scale
            double NewScale = CalculateScale();
            if (NewScale.Equals(Scale))
                OnScaleChanged(null);
            else
                Scale = NewScale;
        }

        internal bool IsMinScale = true;
        private void OnScaleChanged(ChangedEventArgs<double> e)
        {
            if (e != null)
            {
                InternalScale = e.NewValue;
                IsMinScale = MinScale.Equals(Scale);
            }

            // Update Viewport
            Rect Viewport = CalculateViewport();
            if (Viewport.Equals(this.Viewport))
                OnViewportChanged(null);
            else
                this.Viewport = Viewport;
        }

        private void OnViewportChanged(ChangedEventArgs<Rect> e)
        {
            if (e != null)
            {
                InternalViewport = e.NewValue;
                ViewportChanged?.Invoke(this, e);
            }

            UpdateLayerCanvas();
        }

        private Size<int> CalculateViewBox()
        {
            if (DisplayAreaHeight == 0 || DisplayAreaWidth == 0 ||
                ContextWidth == 0 || ContextHeight == 0)
                return Size<int>.Empty;

            double Ratio = 1,
                   Scale = Math.Max(ContextWidth / DisplayAreaWidth, ContextHeight / DisplayAreaHeight);

            while (Ratio < Scale)
                Ratio *= ScaleRatio;

            MinScale = 1d / Ratio;

            return new Size<int>((int)(DisplayAreaWidth * Ratio), (int)(DisplayAreaHeight * Ratio));
        }

        private double CalculateScale()
        {
            if (ContextWidth == 0 || ContextHeight == 0)
                return -1d;

            if (IsMinScale || Scale.Equals(-1d))
                return MinScale;

            return Math.Max(MinScale, Scale);
        }

        private bool IsZoomWithMouse;
        private Point Zoom_MousePosition;
        private Vector Zoom_MouseMoveDelta;
        private bool IsResizeViewer;
        private Point Resize_ViewportCenterInImage;
        private Rect CalculateViewport()
        {
            if (ContextWidth == 0 || ContextHeight == 0)
                return Rect.Empty;

            int ViewBoxW = ViewBox.Width,
                ViewBoxH = ViewBox.Height;

            Rect Viewport = this.Viewport;
            if (Viewport.IsEmpty)
                return new Rect(0, 0, ViewBoxW, ViewBoxH);

            double UnderFactor = 1 / Scale,
                   HalfFactor = MinScale * UnderFactor * 0.5,
                   HalfViewportW = ViewBoxW * HalfFactor,
                   HalfViewportH = ViewBoxH * HalfFactor;

            Point C0 = IsZoomWithMouse ? new Point(Zoom_MousePosition.X - Zoom_MouseMoveDelta.X * UnderFactor, Zoom_MousePosition.Y - Zoom_MouseMoveDelta.Y * UnderFactor) :
                       IsResizeViewer ? new Point(Resize_ViewportCenterInImage.X + ContextX, Resize_ViewportCenterInImage.Y + ContextY) :
                                        new Point(Viewport.X + Viewport.Width * 0.5, Viewport.Y + Viewport.Height * 0.5);

            Rect Result = new Rect(C0.X - HalfViewportW,
                                   C0.Y - HalfViewportH,
                                   HalfViewportW * 2,
                                   HalfViewportH * 2);
            Result.X = MathHelper.Clamp(Result.X, 0d, ViewBoxW - Result.Width);
            Result.Y = MathHelper.Clamp(Result.Y, 0d, ViewBoxH - Result.Height);

            return Result;
        }

        private void UpdateViewBoxAndContextInfo()
        {
            UpdateContextSize(Layers);

            Size<int> ViewBox = CalculateViewBox();
            if (ViewBox.Equals(this.ViewBox))
                OnViewBoxChanged(null);
            else
                this.ViewBox = ViewBox;
        }

        private void UpdateLayerCanvas()
        {
            foreach (ImageViewerLayer Layer in Layers)
                Layer.UpdateCanvas();
        }

        private bool IsLeftMouseDown;
        private Point MousePosition;
        private Vector MouseMoveDelta;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                CaptureMouse();
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

                Vector TempVector = new Vector(Position.X - MousePosition.X, Position.Y - MousePosition.Y);
                MouseMoveDelta += TempVector;

                if (IsMinScale)
                    return;

                Viewport = new Rect(MathHelper.Clamp(Viewport.X - TempVector.X / Scale, 0, ViewBox.Width - Viewport.Width),
                                    MathHelper.Clamp(Viewport.Y - TempVector.Y / Scale, 0, ViewBox.Height - Viewport.Height),
                                    Viewport.Width,
                                    Viewport.Height);

                MousePosition = Position;
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                ReleaseMouseCapture();
                IsLeftMouseDown = false;

                if (MouseMoveDelta.LengthSquared <= 25)
                    OnClick(new RoutedEventArgs(ClickEvent, this));
            }
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Point Position = e.GetPosition(this);
            Zoom(e.Delta > 0,
                 new Point(Viewport.X + Position.X / Scale, Viewport.Y + Position.Y / Scale),
                 new Vector(Position.X - DisplayAreaWidth / 2, Position.Y - DisplayAreaHeight / 2));
        }

        private void OnClick(RoutedEventArgs e)
            => RaiseEvent(e);

        public void Zoom(bool ZoomIn)
        {
            if (ZoomIn)
            {
                if (0 < Scale && Scale < MaxScale)
                    Scale = Math.Min(Scale * ScaleRatio, MaxScale);
            }
            else
            {
                if (0 < Scale && Scale > MinScale)
                    Scale = Math.Max(MinScale, Scale / ScaleRatio);
            }
        }
        private void Zoom(bool ZoomIn, Point Zoom_MousePosition, Vector Zoom_MouseMoveDelta)
        {
            if (ZoomIn)
            {
                if (0 < Scale && Scale < MaxScale)
                {
                    try
                    {
                        IsZoomWithMouse = true;
                        this.Zoom_MousePosition = Zoom_MousePosition;
                        this.Zoom_MouseMoveDelta = Zoom_MouseMoveDelta;
                        Scale = Math.Min(Scale * ScaleRatio, MaxScale);
                    }
                    finally
                    {
                        IsZoomWithMouse = false;
                    }
                }
            }
            else
            {
                if (0 < Scale && MinScale < Scale)
                {
                    try
                    {
                        IsZoomWithMouse = true;
                        this.Zoom_MousePosition = Zoom_MousePosition;
                        this.Zoom_MouseMoveDelta = Zoom_MouseMoveDelta;
                        Scale = Math.Max(MinScale, Scale / ScaleRatio);
                    }
                    finally
                    {
                        IsZoomWithMouse = false;
                    }
                }
            }
        }

        /// <summary>
        /// Move Viewport To Point(X, Y) at SourceImage.
        /// </summary>
        public void MoveTo(Point Position)
            => MoveTo(Position.X, Position.Y);
        /// <summary>
        /// Move Viewport To Point(X, Y) at SourceImage.
        /// </summary>
        public void MoveTo(double X, double Y)
        {
            Rect TempViewport = new Rect(ContextX + X - Viewport.Width / 2,
                                         ContextY + Y - Viewport.Height / 2,
                                         Viewport.Width,
                                         Viewport.Height);

            TempViewport.X = MathHelper.Clamp(TempViewport.X, 0, ViewBox.Width - TempViewport.Width);
            TempViewport.Y = MathHelper.Clamp(TempViewport.Y, 0, ViewBox.Height - TempViewport.Height);
            Viewport = TempViewport;
        }

    }
}