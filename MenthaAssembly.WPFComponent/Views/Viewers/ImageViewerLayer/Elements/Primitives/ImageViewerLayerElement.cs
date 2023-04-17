using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace MenthaAssembly.Views.Primitives
{
    public abstract class ImageViewerLayerElement : ImageViewerLayerObject
    {
        public static readonly RoutedEvent DragDeltaEvent = Thumb.DragDeltaEvent.AddOwner(typeof(ImageViewerLayerElement));
        public event DragDeltaEventHandler DragDelta
        {
            add => AddHandler(DragDeltaEvent, value);
            remove => RemoveHandler(DragDeltaEvent, value);
        }

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageViewerLayerElement));
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public static new readonly RoutedEvent SizeChangedEvent =
            EventManager.RegisterRoutedEvent("SizeChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Size>), typeof(ImageViewerLayerElement));
        public new event RoutedPropertyChangedEventHandler<Point> SizeChanged
        {
            add => AddHandler(SizeChangedEvent, value);
            remove => RemoveHandler(SizeChangedEvent, value);
        }

        public static new readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(ImageViewerLayerElement),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange,
                    (d, e) =>
                    {
                        if (d is ImageViewerLayerElement This)
                        {
                            double H = This.Height;
                            Size Old = e.OldValue is double OldValue && !double.IsNaN(OldValue) && !double.IsNaN(H) ? new(OldValue, H) : new(double.NaN, double.NaN),
                                 New = e.NewValue is double NewValue && !double.IsNaN(NewValue) && !double.IsNaN(H) ? new(NewValue, H) : new(double.NaN, double.NaN);

                            This.OnSizeChanged(new RoutedPropertyChangedEventArgs<Size>(Old, New, SizeChangedEvent));
                        }
                    }));
        public new double Width
        {
            get => (double)GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        public static new readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(ImageViewerLayerElement),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange,
                    (d, e) =>
                    {
                        if (d is ImageViewerLayerElement This)
                        {
                            double W = This.Width;
                            Size Old = e.OldValue is double OldValue && !double.IsNaN(OldValue) && !double.IsNaN(W) ? new(W, OldValue) : new(double.NaN, double.NaN),
                                 New = e.NewValue is double NewValue && !double.IsNaN(NewValue) && !double.IsNaN(W) ? new(W, NewValue) : new(double.NaN, double.NaN);

                            This.OnSizeChanged(new RoutedPropertyChangedEventArgs<Size>(Old, New, SizeChangedEvent));
                        }
                    }));
        public new double Height
        {
            get => (double)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        public static readonly RoutedEvent LocationChangedEvent =
            EventManager.RegisterRoutedEvent("LocationChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Point>), typeof(ImageViewerLayerElement));
        public event RoutedPropertyChangedEventHandler<Point> LocationChanged
        {
            add => AddHandler(LocationChangedEvent, value);
            remove => RemoveHandler(LocationChangedEvent, value);
        }

        public static readonly DependencyProperty LocationProperty =
            DependencyProperty.Register("Location", typeof(Point), typeof(ImageViewerLayerElement),
                new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsParentArrange,
                    (d, e) =>
                    {
                        if (d is ImageViewerLayerElement This)
                            This.OnLocationChanged(e.ToRoutedPropertyChangedEventArgs<Point>(LocationChangedEvent));
                    }));
        public Point Location
        {
            get => (Point)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        public static readonly DependencyProperty DraggableProperty =
              DependencyProperty.Register("Draggable", typeof(bool), typeof(ImageViewerLayerElement), new PropertyMetadata(true));
        public bool Draggable
        {
            get => (bool)GetValue(DraggableProperty);
            set => SetValue(DraggableProperty, value);
        }

        public static readonly DependencyProperty ZoomableProperty =
              DependencyProperty.Register("Zoomable", typeof(bool), typeof(ImageViewerLayerElement),
                  new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentArrange,
                      (d, e) =>
                      {
                          if (d is ImageViewerLayerElement This)
                              This.OnZoomableChanged(e.ToChangedEventArgs<bool>());
                      }));
        public bool Zoomable
        {
            get => (bool)GetValue(ZoomableProperty);
            set => SetValue(ZoomableProperty, value);
        }

        public Size ZoomedDesiredSize { get; protected set; }

        private readonly Binding WidthBinding, HeightBinding;
        protected ImageViewerLayerElement()
        {
            WidthBinding = new Binding(nameof(Width)) { Source = this };
            HeightBinding = new Binding(nameof(Height)) { Source = this };
            _ = SetBinding(FrameworkElement.WidthProperty, WidthBinding);
            _ = SetBinding(FrameworkElement.HeightProperty, HeightBinding);
        }

        protected override Size MeasureOverride(Size AvailableSize)
        {
            double Cw = Width,
                   Ch = Height;

            if (double.IsNaN(Cw) || double.IsInfinity(Cw) || double.IsNaN(Ch) || double.IsInfinity(Ch))
            {
                Cw = 0d;
                Ch = 0d;
            }
            else if (Zoomable)
            {
                double Scale = GetScale();
                if (!double.IsNaN(Scale))
                {
                    Cw *= Scale;
                    Ch *= Scale;
                }
            }

            ZoomedDesiredSize = new Size(Cw, Ch);
            return ZoomedDesiredSize;
        }

        private void OnZoomableChanged(ChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                BindingOperations.ClearBinding(this, FrameworkElement.WidthProperty);
                BindingOperations.ClearBinding(this, FrameworkElement.HeightProperty);
            }
            else
            {
                _ = SetBinding(FrameworkElement.WidthProperty, WidthBinding);
                _ = SetBinding(FrameworkElement.HeightProperty, HeightBinding);
            }
        }

        private bool IsLeftMouseDown;
        private Point MousePosition;
        private double MouseMoveDistance;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _ = CaptureMouse();
                IsLeftMouseDown = true;
                MousePosition = e.GetPosition(null);
                MouseMoveDistance = 0d;
            }

            e.Handled = true;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                Point Position = e.GetPosition(null);
                double Dx = Position.X - MousePosition.X,
                       Dy = Position.Y - MousePosition.Y;
                try
                {
                    if (Draggable)
                    {
                        DragDeltaEventArgs DragDeltaArg = new DragDeltaEventArgs(Dx, Dy);
                        OnDragDelta(DragDeltaArg);
                        if (DragDeltaArg.Handled)
                            return;

                        double Scale = GetScale();
                        Point LastLocation = Location;
                        Location = new Point(LastLocation.X + Dx / Scale, LastLocation.Y + Dy / Scale);
                    }

                    if (MouseMoveDistance <= 25d)
                        MouseMoveDistance += Dx * Dx + Dy * Dy;
                }
                finally
                {
                    MousePosition = Position;
                    e.Handled = true;
                }
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                ReleaseMouseCapture();
                IsLeftMouseDown = false;

                if (MouseMoveDistance <= 25d)
                    OnClick(new RoutedEventArgs(ClickEvent, this));
            }
        }

        protected virtual void OnDragDelta(DragDeltaEventArgs e)
            => RaiseEvent(e);

        protected virtual void OnClick(RoutedEventArgs e)
            => RaiseEvent(e);

        protected virtual void OnLocationChanged(RoutedPropertyChangedEventArgs<Point> e)
            => RaiseEvent(e);

        protected virtual void OnSizeChanged(RoutedPropertyChangedEventArgs<Size> e)
            => RaiseEvent(e);

    }
}