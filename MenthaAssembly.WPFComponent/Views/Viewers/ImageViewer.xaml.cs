﻿using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Views.Primitives;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly.Views
{
    public class ImageViewer : ImageViewerBase
    {
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ImageViewer));
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public event EventHandler<ChangedEventArgs<IImageContext>> SourceChanged;

        public event EventHandler<ChangedEventArgs<Int32Size>> ViewBoxChanged;

        public event EventHandler<ChangedEventArgs<Int32Rect>> ViewportChanged;

        public static readonly DependencyProperty SourceProperty =
              DependencyProperty.Register("Source", typeof(BitmapSource), typeof(ImageViewer), new PropertyMetadata(default,
                  (d, e) =>
                  {
                      if (d is ImageViewer This)
                          This.SourceContext = (BitmapContext)(e.NewValue as BitmapSource);
                  }));
        public BitmapSource Source
        {
            get => (BitmapSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty =
              DependencyProperty.Register("Scale", typeof(double), typeof(ImageViewer), new PropertyMetadata(-1d,
                  (d, e) =>
                  {
                      if (d is ImageViewer This)
                          This.OnScaleChanged(new ChangedEventArgs<double>(e.OldValue, e.NewValue));
                  },
                  (d, v) =>
                  {
                      if (v is double Value)
                          return Value <= 0 ? -1d : Value;

                      return DependencyProperty.UnsetValue;
                  }));
        public new double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty ScaleRatioProperty =
              DependencyProperty.Register("ScaleRatio", typeof(double), typeof(ImageViewer), new PropertyMetadata(2d));
        public double ScaleRatio
        {
            get => (double)GetValue(ScaleRatioProperty);
            set => SetValue(ScaleRatioProperty, value);
        }

        protected double MinScale { set; get; }

        public static readonly DependencyProperty ViewportProperty =
              DependencyProperty.Register("Viewport", typeof(Int32Rect), typeof(ImageViewer), new PropertyMetadata(Int32Rect.Empty,
                  (d, e) =>
                  {
                      if (d is ImageViewer This)
                          This.OnViewportChanged(new ChangedEventArgs<Int32Rect>(e.OldValue, e.NewValue));
                  }));
        public new Int32Rect Viewport
        {
            get => (Int32Rect)GetValue(ViewportProperty);
            set => SetValue(ViewportProperty, value);
        }

        protected override Int32Size ViewBox
        {
            get => base.ViewBox;
            set
            {
                ChangedEventArgs<Int32Size> e = new ChangedEventArgs<Int32Size>(base.ViewBox, value);
                base.ViewBox = value;
                OnViewBoxChanged(e);
            }
        }

        public new BitmapContext DisplayContext
            => base.DisplayContext;
        public new IImageContext SourceContext
        {
            get => base.SourceContext;
            set
            {
                if (base.SourceContext is null && 
                    value is null)
                    return;
                
                ChangedEventArgs<IImageContext> e = new ChangedEventArgs<IImageContext>(base.SourceContext, value);
                base.SourceContext = value;
                this.Dispatcher.Invoke(() => OnSourceChanged(e));
            }
        }

        internal Size DisplayArea { private set; get; }

        static ImageViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewer), new FrameworkPropertyMetadata(typeof(ImageViewer)));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (ActualHeight > 0 && ActualWidth > 0)
            {
                DisplayArea = new Size(this.ActualWidth - (this.BorderThickness.Left + this.BorderThickness.Right),
                                       this.ActualHeight - (this.BorderThickness.Top + this.BorderThickness.Bottom));

                IsResizeViewer = true;
                Resize_ViewportCenterInImage = new Int32Point(Viewport.X + Viewport.Width / 2 - SourceLocation.X,
                                                              Viewport.Y + Viewport.Height / 2 - SourceLocation.Y);
                ViewBox = CalculateViewBox();
                IsResizeViewer = false;
            }
        }

        protected virtual void OnSourceChanged(ChangedEventArgs<IImageContext> e)
        {
            if (e != null)
                SourceChanged?.Invoke(this, e);

            Int32Size ViewBox = CalculateViewBox();
            if (ViewBox.Equals(this.ViewBox))
            {
                OnViewBoxChanged(null);
                return;
            }
            this.ViewBox = ViewBox;
        }

        protected virtual void OnViewBoxChanged(ChangedEventArgs<Int32Size> e)
        {
            if (e != null)
                ViewBoxChanged?.Invoke(this, e);

            SourceLocation = (base.SourceContext is null || base.SourceContext.Width > ViewBox.Width) ?
                             new Int32Point() :
                             new Int32Point((ViewBox.Width - base.SourceContext.Width) / 2,
                                            (ViewBox.Height - base.SourceContext.Height) / 2);

            double NewScale = CalculateScale();
            if (NewScale.Equals(this.Scale))
            {
                OnScaleChanged(null);
                return;
            }
            this.Scale = NewScale;
        }

        internal protected bool IsMinFactor = true;
        protected virtual void OnScaleChanged(ChangedEventArgs<double> e)
        {
            if (e != null)
            {
                base.Scale = e.NewValue;
                IsMinFactor = MinScale.Equals(Scale);
            }

            Int32Rect Viewport = CalculateViewport();
            if (Viewport.Equals(this.Viewport))
            {
                OnViewportChanged(null);
                return;
            }
            this.Viewport = Viewport;
        }

        protected virtual void OnViewportChanged(ChangedEventArgs<Int32Rect> e)
        {
            if (e != null)
            {
                ViewportChanged?.Invoke(this, e);
                base.Viewport = e.NewValue;
            }
            OnRenderImage();
        }

        protected Int32Size CalculateViewBox()
        {
            if (base.SourceContext is null || DisplayArea.IsEmpty)
                return Int32Size.Empty;

            double Ratio = 1;
            double Scale = Math.Max(base.SourceContext.Width / DisplayArea.Width, base.SourceContext.Height / DisplayArea.Height);

            while (Ratio < Scale)
                Ratio *= ScaleRatio;

            MinScale = 1d / Ratio;

            return new Int32Size(DisplayArea.Width * Ratio, DisplayArea.Height * Ratio);
        }

        protected double CalculateScale()
        {
            if (base.SourceContext is null)
                return -1d;

            if (IsMinFactor || Scale.Equals(-1d))
                return MinScale;

            return Math.Max(MinScale, Scale);
        }

        private bool IsZoomWithMouse;
        private Point Zoom_MousePosition;
        private Vector Zoom_MouseMoveDelta;
        private bool IsResizeViewer;
        private Int32Point Resize_ViewportCenterInImage;
        protected Int32Rect CalculateViewport()
        {
            if (base.SourceContext is null || DisplayArea.IsEmpty)
                return Int32Rect.Empty;

            double UnderFactor = 1 / Scale,
                   HalfFactor = MinScale * UnderFactor * 0.5;

            Size ViewportHalfSize = new Size(ViewBox.Width * HalfFactor, ViewBox.Height * HalfFactor);
            Point C0 = IsZoomWithMouse ? new Point(Zoom_MousePosition.X - Zoom_MouseMoveDelta.X * UnderFactor, Zoom_MousePosition.Y - Zoom_MouseMoveDelta.Y * UnderFactor) :
                       IsResizeViewer ? new Point(Resize_ViewportCenterInImage.X + SourceLocation.X, Resize_ViewportCenterInImage.Y + SourceLocation.Y) :
                                        new Point(Viewport.X + Viewport.Width * 0.5, Viewport.Y + Viewport.Height * 0.5);

            Int32Rect Result = new Int32Rect((int)Math.Round(C0.X - ViewportHalfSize.Width),
                                             (int)Math.Round(C0.Y - ViewportHalfSize.Height),
                                             (int)Math.Round(ViewportHalfSize.Width * 2),
                                             (int)Math.Round(ViewportHalfSize.Height * 2));
            Result.X = Math.Min(Math.Max(0, Result.X), ViewBox.Width - Result.Width);
            Result.Y = Math.Min(Math.Max(0, Result.Y), ViewBox.Height - Result.Height);

            return Result;
        }

        protected void OnRenderImage()
        {
            if (DisplayArea.Width is 0 || ActualHeight is 0)
                return;

            Int32Size ImageSize = new Int32Size(DisplayArea.Width, DisplayArea.Height);
            if (DisplayContext is null ||
                DisplayContext.Width != ImageSize.Width ||
                DisplayContext.Height != ImageSize.Height)
            {
                SetDisplayImage(this, new WriteableBitmap(ImageSize.Width, ImageSize.Height, 96, 96, PixelFormats.Bgra32, null));
                LastImageRect = new Int32Rect(0, 0, ImageSize.Width, ImageSize.Height);
            }

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                BitmapContext Display = DisplayContext;
                if (Display.TryLock(1))
                {
                    try
                    {
                        Int32Rect DirtyRect = OnDraw();
                        Display.AddDirtyRect(DirtyRect);
                    }
                    catch { }
                    finally
                    {
                        Display.Unlock();
                    }
                }
            }));
        }

        private bool IsLeftMouseDown;
        private Point MousePosition;
        private Vector MouseMoveDelta;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.CaptureMouse();
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

                Vector TempVector = new Vector(Math.Ceiling(Position.X - MousePosition.X), Math.Ceiling(Position.Y - MousePosition.Y));
                MouseMoveDelta += TempVector;

                if (IsMinFactor)
                    return;

                Viewport = new Int32Rect(Math.Min(Math.Max(0, Viewport.X - (int)(TempVector.X / Scale)), ViewBox.Width - Viewport.Width),
                                         Math.Min(Math.Max(0, Viewport.Y - (int)(TempVector.Y / Scale)), ViewBox.Height - Viewport.Height),
                                         Viewport.Width,
                                         Viewport.Height);
                MousePosition += TempVector;
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                this.ReleaseMouseCapture();
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
                 new Vector(Position.X - DisplayArea.Width / 2, Position.Y - DisplayArea.Height / 2));
        }

        protected virtual void OnClick(RoutedEventArgs e)
            => RaiseEvent(e);


        public void Zoom(bool ZoomIn)
        {
            if (ZoomIn)
            {
                if (0 < Scale && Scale < 1)
                    Scale = Math.Min(1, Scale * ScaleRatio);
            }
            else
            {
                if (0 < Scale && Scale > MinScale)
                    Scale = Math.Max(MinScale, Scale / ScaleRatio);
            }
        }
        public void Zoom(bool ZoomIn, Point Zoom_MousePosition, Vector Zoom_MouseMoveDelta)
        {
            if (ZoomIn)
            {
                if (0 < Scale && Scale < 1)
                {
                    IsZoomWithMouse = true;
                    this.Zoom_MousePosition = Zoom_MousePosition;
                    this.Zoom_MouseMoveDelta = Zoom_MouseMoveDelta;
                    Scale = Math.Min(1, Scale * ScaleRatio);
                    IsZoomWithMouse = false;
                }
            }
            else
            {
                if (0 < Scale && Scale > MinScale)
                {
                    IsZoomWithMouse = true;
                    this.Zoom_MousePosition = Zoom_MousePosition;
                    this.Zoom_MouseMoveDelta = Zoom_MouseMoveDelta;
                    Scale = Math.Max(MinScale, Scale / ScaleRatio);
                    IsZoomWithMouse = false;
                }
            }
        }

        /// <summary>
        /// Move Viewport To Point(X, Y) at SourceImage.
        /// </summary>
        public void MoveTo(Int32Point Position)
            => MoveTo(Position.X, Position.Y);
        /// <summary>
        /// Move Viewport To Point(X, Y) at SourceImage.
        /// </summary>
        public void MoveTo(int X, int Y)
        {
            Int32Rect TempViewport = new Int32Rect(SourceLocation.X + X - Viewport.Width / 2,
                                                   SourceLocation.Y + Y - Viewport.Height / 2,
                                                   Viewport.Width,
                                                   Viewport.Height);

            TempViewport.X = Math.Min(Math.Max(0, TempViewport.X), ViewBox.Width - TempViewport.Width);
            TempViewport.Y = Math.Min(Math.Max(0, TempViewport.Y), ViewBox.Height - TempViewport.Height);
            Viewport = TempViewport;
        }

        /// <summary>
        /// Get PixelInfo of current mouse position in ImageViewer.
        /// </summary>
        public IPixel GetPixel()
        {
            Point MousePosition = Mouse.GetPosition(this);
            return GetPixel(MousePosition.X, MousePosition.Y);
        }
        /// <summary>
        /// Get PixelInfo of Point(X, Y) at ImageViewer.
        /// </summary>
        public IPixel GetPixel(double X, double Y)
        {
            if (X < 0 || ActualWidth < X ||
                Y < 0 || ActualHeight < Y)
                throw new ArgumentOutOfRangeException();

            double AreaX = X - BorderThickness.Left;
            double AreaY = Y - BorderThickness.Top;

            if (LastImageRect.X <= AreaX && AreaX <= LastImageRect.X + LastImageRect.Width &&
                LastImageRect.Y <= AreaY && AreaY <= LastImageRect.Y + LastImageRect.Height)
                return GetPixel(Math.Min(Viewport.X + (int)(AreaX / Scale) - SourceLocation.X, base.SourceContext.Width - 1),
                                Math.Min(Viewport.Y + (int)(AreaY / Scale) - SourceLocation.Y, base.SourceContext.Height - 1));

            return new BGRA();
        }
        /// <summary>
        /// Get PixelInfo of Point(X, Y) at SourceImage.
        /// </summary>
        public IPixel GetPixel(int X, int Y)
            => SourceContext[X, Y];

        /// <summary>
        /// Get Pixel's Point of current mouse position in ImageViewer.
        /// </summary>
        public Int32Point GetPixelPoint()
        {
            Point MousePosition = Mouse.GetPosition(this);
            return GetPixelPoint(MousePosition.X, MousePosition.Y);
        }
        /// <summary>
        /// Get Pixel's Point of Point(X, Y) at ImageViewer.
        /// </summary>
        public Int32Point GetPixelPoint(double X, double Y)
        {
            if (SourceContext is null)
                throw new ArgumentNullException("Source is null.");

            double AreaX = X - BorderThickness.Left;
            double AreaY = Y - BorderThickness.Top;

            return new Int32Point(Viewport.X + (int)(AreaX / Scale) - SourceLocation.X,
                                  Viewport.Y + (int)(AreaY / Scale) - SourceLocation.Y);
        }

        /// <summary>
        /// Get Point of ImageViewer at PixelPoint(X, Y).
        /// </summary>
        public Point GetViewerPoint(Int32Point PixelPoint)
            => GetViewerPoint(PixelPoint.X, PixelPoint.Y);
        /// <summary>
        /// Get Point of ImageViewer at PixelPoint(X, Y).
        /// </summary>
        public Point GetViewerPoint(int X, int Y)
        {
            if (base.SourceContext is null)
                throw new ArgumentNullException("Source is null.");

            return new Point((X + SourceLocation.X - Viewport.X) * Scale + BorderThickness.Left,
                             (Y + SourceLocation.Y - Viewport.Y) * Scale + BorderThickness.Top);
        }

    }
}