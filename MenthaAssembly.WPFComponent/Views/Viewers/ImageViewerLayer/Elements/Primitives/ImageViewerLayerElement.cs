using System.Windows;
using System.Windows.Input;

namespace MenthaAssembly.Views.Primitives
{
    public abstract class ImageViewerLayerElement : ImageViewerLayerObject
    {
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageViewerLayerElement));
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
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

                if (Draggable)
                {
                    double Scale = GetScale();
                    Point LastLocation = Location;
                    Location = new Point(LastLocation.X + Dx / Scale, LastLocation.Y + Dy / Scale);
                }

                if (MouseMoveDistance <= 25d)
                    MouseMoveDistance += Dx * Dx + Dy * Dy;

                MousePosition = Position;
                e.Handled = true;
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

        protected virtual void OnClick(RoutedEventArgs e)
            => RaiseEvent(e);

        protected virtual void OnLocationChanged(RoutedPropertyChangedEventArgs<Point> e)
            => RaiseEvent(e);

    }
}