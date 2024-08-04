using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Views.Primitives;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(Layers))]
    public class ImageViewer : FrameworkElement
    {
        internal event EventHandler<ChangedEventArgs<Size<int>>> ViewBoxChanged;

        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageViewer));
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public static readonly RoutedEvent RightClickEvent =
            EventManager.RegisterRoutedEvent(nameof(RightClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageViewer));
        public event RoutedEventHandler RightClick
        {
            add => AddHandler(RightClickEvent, value);
            remove => RemoveHandler(RightClickEvent, value);
        }

        public static readonly RoutedEvent ItemsSourceChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(ItemsSourceChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<IEnumerable>), typeof(ImageViewer));
        public event RoutedPropertyChangedEventHandler<IEnumerable> ItemsSourceChanged
        {
            add => AddHandler(ItemsSourceChangedEvent, value);
            remove => RemoveHandler(ItemsSourceChangedEvent, value);
        }

        public static readonly RoutedEvent ScaleChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(ScaleChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(ImageViewer));
        public event RoutedPropertyChangedEventHandler<double> ScaleChanged
        {
            add => AddHandler(ScaleChangedEvent, value);
            remove => RemoveHandler(ScaleChangedEvent, value);
        }

        public static readonly RoutedEvent ViewportChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(ViewportChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Rect>), typeof(ImageViewer));
        public event RoutedPropertyChangedEventHandler<Rect> ViewportChanged
        {
            add => AddHandler(ViewportChangedEvent, value);
            remove => RemoveHandler(ViewportChangedEvent, value);
        }

        public static readonly DependencyProperty BackgroundProperty =
            Border.BackgroundProperty.AddOwner(typeof(ImageViewer), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E))));
        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty BorderBrushProperty =
            Border.BorderBrushProperty.AddOwner(typeof(ImageViewer), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x3F, 0x3F, 0x46))));
        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static readonly DependencyProperty BorderThicknessProperty =
            Border.BorderThicknessProperty.AddOwner(typeof(ImageViewer), new PropertyMetadata(new Thickness(1)));
        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            ItemsControl.ItemsSourceProperty.AddOwner(typeof(ImageViewer), new FrameworkPropertyMetadata(null,
                (d, e) =>
                {
                    if (d is ImageViewer This)
                        This.OnItemsSourceChanged(e.ToRoutedPropertyChangedEventArgs<IEnumerable>(ItemsSourceChangedEvent));
                }));
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ImageViewerLayerCollection Layers { get; }

        internal static readonly DependencyPropertyKey ViewBoxPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ViewBox), typeof(Size<int>), typeof(ImageViewer), new PropertyMetadata(Size<int>.Empty,
                (d, e) =>
                {
                    if (d is ImageViewer This)
                        This.OnViewBoxChanged(e.ToChangedEventArgs<Size<int>>());
                }));
        public Size<int> ViewBox
            => (Size<int>)GetValue(ViewBoxPropertyKey.DependencyProperty);

        public static readonly DependencyProperty FitMarginProperty =
            DependencyProperty.Register(nameof(FitMargin), typeof(Thickness), typeof(ImageViewer), new PropertyMetadata(new Thickness(10),
                    (d, e) =>
                    {
                        if (d is ImageViewer This)
                            This.OnFitMarginChanged(e.ToChangedEventArgs<Thickness>());
                    }));
        public Thickness FitMargin
        {
            get => (Thickness)GetValue(FitMarginProperty);
            set => SetValue(FitMarginProperty, value);
        }

        public static readonly DependencyProperty ViewportProperty =
            DependencyProperty.Register(nameof(Viewport), typeof(Rect), typeof(ImageViewer), new PropertyMetadata(Rect.Empty,
                (d, e) =>
                {
                    if (d is ImageViewer This)
                        This.OnViewportChanged(e.ToRoutedPropertyChangedEventArgs<Rect>(ViewportChangedEvent));
                }));
        public Rect Viewport
        {
            get => (Rect)GetValue(ViewportProperty);
            set => SetValue(ViewportProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register(nameof(Scale), typeof(double), typeof(ImageViewer), new PropertyMetadata(double.NaN,
                (d, e) =>
                {
                    if (d is ImageViewer This)
                        This.OnScaleChanged(e.ToRoutedPropertyChangedEventArgs<double>(ScaleChangedEvent));
                }));
        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, double.IsNaN(value) ? value : value.Clamp(FitScale, MaxScale));
        }

        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.Register(nameof(MaxScale), typeof(double), typeof(ImageViewer), new PropertyMetadata(1d,
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
            DependencyProperty.Register(nameof(ScaleRatio), typeof(double), typeof(ImageViewer), new PropertyMetadata(2d));
        public double ScaleRatio
        {
            get => (double)GetValue(ScaleRatioProperty);
            set => SetValue(ScaleRatioProperty, value);
        }

        internal readonly ImageViewerManager Manager;
        private readonly Border TemplateBorder;
        private readonly ImageViewerPresenter TemplatePresenter;
        public ImageViewer()
        {
            Manager = new ImageViewerManager(this);

            TemplatePresenter = new ImageViewerPresenter(this);
            Layers = new ImageViewerLayerCollection(this, TemplatePresenter.Children);

            TemplateBorder = new Border
            {
                ClipToBounds = true,
                Child = TemplatePresenter
            };
            _ = TemplateBorder.SetBinding(BackgroundProperty, new Binding(nameof(Background)) { Source = this });
            _ = TemplateBorder.SetBinding(BorderBrushProperty, new Binding(nameof(BorderBrush)) { Source = this });
            _ = TemplateBorder.SetBinding(BorderThicknessProperty, new Binding(nameof(BorderThickness)) { Source = this });
            _ = TemplateBorder.SetBinding(SnapsToDevicePixelsProperty, new Binding(nameof(SnapsToDevicePixels)) { Source = this });
            _ = TemplateBorder.SetBinding(UseLayoutRoundingProperty, new Binding(nameof(UseLayoutRounding)) { Source = this });
            AddVisualChild(TemplateBorder);
        }

        protected virtual void OnItemsSourceChanged(RoutedPropertyChangedEventArgs<IEnumerable> e)
        {
            Layers.SetItemsSource(e.NewValue);
            RaiseEvent(e);
        }

        protected virtual void OnFitMarginChanged(ChangedEventArgs<Thickness> e)
            => Manager.Add(ImageViewerAction.ComputeViewBox);

        protected virtual void OnViewBoxChanged(ChangedEventArgs<Size<int>> e)
        {
            Manager.Add(ImageViewerAction.ContextLocation);
            ViewBoxChanged?.Invoke(this, e);
        }

        protected virtual void OnViewportChanged(RoutedPropertyChangedEventArgs<Rect> e)
        {
            Manager.Add(ImageViewerAction.RenderCanvas);
            RaiseEvent(e);
        }

        protected virtual void OnScaleChanged(RoutedPropertyChangedEventArgs<double> e)
        {
            Manager.Add(ImageViewerAction.ComputeViewport);
            RaiseEvent(e);
        }

        protected internal int ContextWidth = 0,
                               ContextHeight = 0;
        protected internal virtual void RefreshContextSize()
        {
            ContextWidth = 0;
            ContextHeight = 0;
            foreach (ImageViewerLayer Layer in Layers)
            {
                if (!Layer.IsVisible)
                    continue;

                if (Layer.Source is ImageSource Image)
                {
                    ContextWidth = Math.Max(ContextWidth, (int)Image.Width);
                    ContextHeight = Math.Max(ContextHeight, (int)Image.Height);
                }
                else if (Layer.SourceContext is IImageContext Context)
                {
                    ContextWidth = Math.Max(ContextWidth, Context.Width);
                    ContextHeight = Math.Max(ContextHeight, Context.Height);
                }
            }
        }

        protected internal int ContextX = 0,
                               ContextY = 0;
        protected internal virtual void RefreshContextLocation()
        {
            int Lx = ContextX,
                Ly = ContextY;

            Size<int> ViewBox = this.ViewBox;
            ContextX = Math.Max((ViewBox.Width - ContextWidth) >> 1, 0);
            ContextY = Math.Max((ViewBox.Height - ContextHeight) >> 1, 0);

            // Updates the center point of viewport
            if (!double.IsNaN(ViewportCx) && !double.IsNaN(ViewportCy))
            {
                ViewportCx += ContextX - Lx;
                ViewportCy += ContextY - Ly;
            }
        }

        protected internal double FitScale = double.NaN;
        protected internal virtual Size<int> ComputeViewBox(out double FitScale)
        {
            FitScale = double.NaN;
            if (ContextWidth == 0 || ContextHeight == 0)
                return Size<int>.Empty;

            double Lw = TemplatePresenter.ActualWidth,
                   Lh = TemplatePresenter.ActualHeight;
            if (Lw == 0 || Lh == 0)
                return Size<int>.Empty;

            Thickness FitMargin = this.FitMargin;
            FitScale = Math.Min((Lw - FitMargin.Left - FitMargin.Right) / ContextWidth, (Lh - FitMargin.Top - FitMargin.Bottom) / ContextHeight);
            return new Size<int>((int)(Lw / FitScale), (int)(Lh / FitScale));
        }

        protected internal virtual double ComputeScale()
        {
            if (ContextWidth == 0 || ContextHeight == 0)
                return double.NaN;

            double Scale = this.Scale;
            return double.IsNaN(Scale) || Scale == FitScale ? FitScale :
                                                              Scale.Clamp(FitScale, MaxScale);
        }

        internal double ViewportCx = double.NaN,
                        ViewportCy = double.NaN;
        protected internal virtual Rect ComputeViewport()
        {
            // Checks Context
            if (ContextWidth == 0 || ContextHeight == 0)
            {
                ViewportCx = double.NaN;
                ViewportCy = double.NaN;
                return Rect.Empty;
            }

            // Checks ViewBox
            Size<int> ViewBox = this.ViewBox;
            int ViewBoxW = ViewBox.Width,
                ViewBoxH = ViewBox.Height;
            if (ViewBoxW == 0 || ViewBoxH == 0)
            {
                ViewportCx = double.NaN;
                ViewportCy = double.NaN;
                return Rect.Empty;
            }

            // Checks the center point of viewport
            if (double.IsNaN(ViewportCx) || double.IsNaN(ViewportCy))
            {
                ViewportCx = ViewBoxW / 2d;
                ViewportCy = ViewBoxH / 2d;
                return new Rect(0d, 0d, ViewBoxW, ViewBoxH);
            }

            // Checks Last Viewport
            Rect Last = Viewport;
            if (Last.IsEmpty)
            {
                ViewportCx = ViewBoxW / 2d;
                ViewportCy = ViewBoxH / 2d;
                return new Rect(0d, 0d, ViewBoxW, ViewBoxH);
            }

            double Factor = FitScale / Scale,
                   W = ViewBoxW * Factor,
                   H = ViewBoxH * Factor,
                   X = ViewportCx - W / 2d,
                   Y = ViewportCy - H / 2d;

            // Clamps X
            if (X < 0d)
            {
                ViewportCx -= X;
                X = 0d;
            }
            else
            {
                double Dx = ViewBoxW - X - W;
                if (Dx < 0)
                {
                    ViewportCx += Dx;
                    X += Dx;
                }
            }

            // Clamps Y
            if (Y < 0d)
            {
                ViewportCy -= Y;
                Y = 0d;
            }
            else
            {
                double Dy = ViewBoxH - Y - H;
                if (Dy < 0)
                {
                    ViewportCy += Dy;
                    Y += Dy;
                }
            }

            return new Rect(X, Y, W, H);
        }

        protected override int VisualChildrenCount
            => 1;
        protected override Visual GetVisualChild(int Index)
            => TemplateBorder;

        protected override Size MeasureOverride(Size AvailableSize)
        {
            TemplateBorder.Measure(AvailableSize);
            return base.MeasureOverride(AvailableSize);
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            Rect Rect = new(FinalSize);
            TemplateBorder.Arrange(Rect);
            return base.ArrangeOverride(FinalSize);
        }

        private bool IsLeftMouseDown;
        private Point MousePosition;
        private double MouseMoveDelta;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _ = CaptureMouse();
                IsLeftMouseDown = true;
                MousePosition = e.GetPosition(TemplatePresenter);
                MouseMoveDelta = 0d;
                e.Handled = true;
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                e.Handled = true;
                Point Position = e.GetPosition(TemplatePresenter);
                double Dx = Position.X - MousePosition.X,
                       Dy = Position.Y - MousePosition.Y;

                if (MouseMoveDelta <= 25d)
                    MouseMoveDelta += Dx * Dx + Dy * Dy;

                if (double.IsNaN(ViewportCx) || double.IsNaN(ViewportCy))
                    return;

                double Scale = this.Scale;
                if (double.IsNaN(Scale) || Scale == FitScale)
                    return;

                ViewportCx -= Dx / Scale;
                ViewportCy -= Dy / Scale;
                InvalidateViewport();

                MousePosition = Position;
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
                OnRightClick();

            if (IsLeftMouseDown)
            {
                IsLeftMouseDown = false;
                ReleaseMouseCapture();

                if (MouseMoveDelta <= 25)
                    OnClick();
            }
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Zoom(e.Delta > 0, e.GetPosition(TemplatePresenter));
            e.Handled = true;
        }

        protected virtual void OnClick()
            => RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        protected virtual void OnRightClick()
            => RaiseEvent(new RoutedEventArgs(RightClickEvent, this));

        /// <summary>
        /// Scale to fit scale.
        /// </summary>
        public void Fit()
            => Scale = FitScale;

        /// <summary>
        /// Zooms the viewport around the reference point in viewer.
        /// </summary>
        /// <param name="ZoomIn">Indicates whether zoom in.</param>
        /// <param name="ReferencePoint">The reference point in viewer.</param>
        public void Zoom(bool ZoomIn, Point ReferencePoint)
            => Zoom(ZoomIn, ReferencePoint.X, ReferencePoint.Y);
        /// <summary>
        /// Zooms the viewport around the reference point in viewer.
        /// </summary>
        /// <param name="ZoomIn">Indicates whether zoom in.</param>
        /// <param name="ReferenceX">The x-coordinate of reference point in viewer.</param>
        /// <param name="ReferenceY">The y-coordinate of reference point in viewer.</param>
        public void Zoom(bool ZoomIn, double ReferenceX, double ReferenceY)
        {
            // Checks Scale
            double LastScale = this.Scale;
            if (double.IsNaN(LastScale))
                return;

            double Scale = MathHelper.Clamp(ZoomIn ? LastScale * ScaleRatio : LastScale / ScaleRatio, FitScale, MaxScale);
            if (this.Scale != Scale)
            {
                Rect Viewport = this.Viewport;
                ViewportCx = Viewport.X + ReferenceX / LastScale - (ReferenceX - TemplatePresenter.ActualWidth / 2d) / Scale;
                ViewportCy = Viewport.Y + ReferenceY / LastScale - (ReferenceY - TemplatePresenter.ActualHeight / 2d) / Scale;
                InvalidateViewport();

                this.Scale = Scale;
            }
        }

        /// <summary>
        /// Moves the viewport to the reference point in image.
        /// </summary>
        /// <param name="ReferencePoint">The reference point in image.</param>
        public void MoveTo(Point ReferencePoint)
            => MoveTo(ReferencePoint.X, ReferencePoint.Y);
        /// <summary>
        /// Moves the viewport to the reference point in image.
        /// </summary>
        /// <param name="ReferenceX">The x-coordinate of reference point in image.</param>
        /// <param name="ReferenceY">The y-coordinate of reference point in image.</param>
        public void MoveTo(double ReferenceX, double ReferenceY)
        {
            if (!double.IsNaN(ViewportCx) && !double.IsNaN(ViewportCy))
            {
                ViewportCx = ContextX + ReferenceX;
                ViewportCy = ContextY + ReferenceY;
                InvalidateViewport();
            }
        }

        /// <summary>
        /// Invalidates the viewport of the viewer, and forces to render canvas.
        /// </summary>
        public virtual void InvalidateViewport()
            => Manager.Add(ImageViewerAction.ComputeViewport);

        /// <summary>
        /// Invalidates the rendering of the canvas, and forces to render new one.
        /// </summary>
        public virtual void InvalidateCanvas()
            => Manager.Add(ImageViewerAction.RenderCanvas);

    }
}