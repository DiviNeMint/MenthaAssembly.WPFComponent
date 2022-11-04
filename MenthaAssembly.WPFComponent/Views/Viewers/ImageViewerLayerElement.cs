using MenthaAssembly.Views.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(Content))]
    public class ImageViewerLayerElement : ContentPresenter
    {
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageViewerLayerElement));
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public static readonly RoutedEvent LocationChangedEvent =
            EventManager.RegisterRoutedEvent("LocationChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Point<int>>), typeof(ImageViewerLayerElement));
        public event RoutedPropertyChangedEventHandler<Point<int>> LocationChanged
        {
            add => AddHandler(LocationChangedEvent, value);
            remove => RemoveHandler(LocationChangedEvent, value);
        }

        public static readonly DependencyProperty LocationProperty =
              DependencyProperty.Register("Location", typeof(Point<int>), typeof(ImageViewerLayerElement), new FrameworkPropertyMetadata(Point<int>.Origin,
                  FrameworkPropertyMetadataOptions.AffectsParentArrange,
                  (d, e) =>
                  {
                      if (d is ImageViewerLayerElement This)
                          This.OnLocationChanged(e.ToChangedEventArgs<Point<int>>());
                  }));
        public Point<int> Location
        {
            get => (Point<int>)GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        public static readonly DependencyProperty IsDraggableProperty =
              DependencyProperty.Register("IsDraggable", typeof(bool), typeof(ImageViewerLayerElement), new PropertyMetadata(true));
        public bool IsDraggable
        {
            get => (bool)GetValue(IsDraggableProperty);
            set => SetValue(IsDraggableProperty, value);
        }

        private ImageViewerLayer Layer;
        protected override void OnVisualParentChanged(DependencyObject OldParent)
        {
            base.OnVisualParentChanged(OldParent);
            Layer = Parent as ImageViewerLayer;
        }

        private bool IsLeftMouseDown;
        private Point MousePosition;
        private Vector MouseMoveDelta;
        private double DragDx, DragDy;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                CaptureMouse();
                IsLeftMouseDown = true;
                MousePosition = e.GetPosition(this);
                MouseMoveDelta = new Vector();
            }

            e.Handled = true;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                Point Position = e.GetPosition(this);
                Vector Delta = new Vector(Position.X - MousePosition.X, Position.Y - MousePosition.Y);

                if (IsDraggable &&
                    Layer?.Viewer is ImageViewerBase Viewer)
                {
                    double Scale = Viewer.InternalScale,
                           DDx = DragDx + Delta.X / Scale,
                           DDy = DragDy + Delta.Y / Scale,
                           IDx = Math.Round(DDx),
                           IDy = Math.Round(DDy);

                    DragDx = IDx - DDx;
                    DragDy = IDy - DDy;

                    if (IDx != 0d || IDy != 0d)
                        Location = Point<int>.Offset(Location, (int)IDx, (int)IDy);
                }

                MouseMoveDelta += Delta;
            }

            e.Handled = true;
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

            e.Handled = true;
        }

        protected virtual void OnClick(RoutedEventArgs e)
            => RaiseEvent(e);

        protected virtual void OnLocationChanged(ChangedEventArgs<Point<int>> e)
        {
            RoutedPropertyChangedEventArgs<Point<int>> Arg = new(e.OldValue, e.NewValue, LocationChangedEvent) { Source = this };
            RaiseEvent(Arg);
        }

    }
}