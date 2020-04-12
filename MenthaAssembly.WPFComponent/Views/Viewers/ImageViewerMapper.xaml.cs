using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Views.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public class ImageViewerMapper : ImageViewerBase
    {
        public static readonly DependencyProperty TargetViewerProperty =
              DependencyProperty.Register("TargetViewer", typeof(ImageViewer), typeof(ImageViewerMapper), new PropertyMetadata(default,
                  (d, e) =>
                  {
                      if (d is ImageViewerMapper This)
                      {
                          if (e.OldValue is ImageViewer OldViewer)
                          {
                              BindingOperations.ClearBinding(This, BackgroundProperty);
                              OldViewer.SourceChanged -= This.OnSourceChanged;
                              OldViewer.ViewBoxChanged -= This.OnViewBoxChanged;
                              OldViewer.ViewportChanged -= This.OnViewportChanged;
                          }

                          ImageViewer NewViewer = e.NewValue as ImageViewer;
                          This._TargetViewer = NewViewer;
                          if (NewViewer != null)
                          {
                              This.PART_Container?.SetBinding(BackgroundProperty, new Binding
                              {
                                  Path = new PropertyPath(BackgroundProperty),
                                  Source = NewViewer
                              });
                              NewViewer.SourceChanged += This.OnSourceChanged;
                              NewViewer.ViewBoxChanged += This.OnViewBoxChanged;
                              NewViewer.ViewportChanged += This.OnViewportChanged;

                              This.OnRenderSizeChanged(new SizeChangedInfo(This, This.RenderSize, true, true));
                          }

                      }
                  }));
        protected ImageViewerBase _TargetViewer { set; get; }
        public ImageViewer TargetViewer
        {
            get => (ImageViewer)GetValue(TargetViewerProperty);
            set => SetValue(TargetViewerProperty, value);
        }

        public static readonly DependencyProperty RectStrokeProperty =
              DependencyProperty.Register("RectStroke", typeof(SolidColorBrush), typeof(ImageViewerMapper), new PropertyMetadata(default));
        public SolidColorBrush RectStroke
        {
            get => (SolidColorBrush)GetValue(RectStrokeProperty);
            set => SetValue(RectStrokeProperty, value);
        }

        public static readonly DependencyProperty RectFillProperty =
              DependencyProperty.Register("RectFill", typeof(SolidColorBrush), typeof(ImageViewerMapper), new PropertyMetadata(default));
        public SolidColorBrush RectFill
        {
            get => (SolidColorBrush)GetValue(RectFillProperty);
            set => SetValue(RectFillProperty, value);
        }

        protected internal override Int32Point SourceLocation
        {
            get => _TargetViewer?.SourceLocation ?? default;
            set { }
        }

        protected Panel PART_Container { set; get; }
        protected Rectangle PART_Rect { set; get; }

        static ImageViewerMapper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewerMapper), new FrameworkPropertyMetadata(typeof(ImageViewerMapper)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.GetTemplateChild("PART_Rect") is Rectangle PART_Rect)
                this.PART_Rect = PART_Rect;
            if (this.GetTemplateChild("PART_Container") is Panel PART_Container)
            {
                this.PART_Container = PART_Container;
                this.PART_Container.SetBinding(BackgroundProperty, new Binding
                {
                    Path = new PropertyPath(BackgroundProperty),
                    Source = TargetViewer
                });
                PART_Container.PreviewMouseWheel += (s, e) => TargetViewer?.Zoom(e.Delta > 0);
                PART_Container.PreviewMouseDown += OnContainerPreviewMouseDown;
                PART_Container.PreviewMouseMove += OnContainerPreviewMouseMove;
                PART_Container.PreviewMouseUp += OnContainerPreviewMouseUp;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.NewSize.IsEmpty || !(sizeInfo.WidthChanged || sizeInfo.HeightChanged) ||
                PART_Container is null || TargetViewer is null || TargetViewer.DisplayArea.IsEmpty ||
                ActualWidth <= 0d || ActualHeight <= 0d)
                return;

            double ScaleWidth = ActualWidth / TargetViewer.DisplayArea.Width,
                   ScaleHeight = ActualHeight / TargetViewer.DisplayArea.Height,
                   Scale = ScaleWidth <= 0 ? ScaleHeight :
                                             ScaleHeight <= 0 ? ScaleWidth :
                                                                Math.Min(ScaleWidth, ScaleHeight);

            PART_Container.Width = Math.Ceiling(TargetViewer.DisplayArea.Width * Scale);
            PART_Container.Height = Math.Ceiling(TargetViewer.DisplayArea.Height * Scale);

            this.Scale = PART_Container.Width / base.ViewBox.Width;
            OnViewportChanged(null, new ChangedEventArgs<Int32Rect>(Int32Rect.Empty, TargetViewer.Viewport));
        }

        private void OnSourceChanged(object sender, ChangedEventArgs<IImageContext> e)
        {
            SourceContext = e.NewValue;
            OnRenderImage();
        }

        protected virtual void OnViewBoxChanged(object sender, ChangedEventArgs<Int32Size> e)
        {
            if (PART_Container is null || 
                TargetViewer is null || 
                ActualWidth <= 0d ||
                ActualHeight <= 0d)
                return;

            double ScaleWidth = ActualWidth / TargetViewer.DisplayArea.Width,
                   ScaleHeight = ActualHeight / TargetViewer.DisplayArea.Height,
                   Scale = ScaleWidth <= 0 ? ScaleHeight :
                                             ScaleHeight <= 0 ? ScaleWidth :
                                                                Math.Min(ScaleWidth, ScaleHeight);

            PART_Container.Width = TargetViewer.DisplayArea.Width * Scale;
            PART_Container.Height = TargetViewer.DisplayArea.Height * Scale;

            ViewBox = e.NewValue;
            Viewport = new Int32Rect(0, 0, ViewBox.Width, ViewBox.Height);
            this.Scale = Viewport.IsEmpty ? -1 : PART_Container.Width / ViewBox.Width;
            OnRenderImage();
        }

        protected virtual void OnViewportChanged(object sender, ChangedEventArgs<Int32Rect> e)
        {
            if (double.IsNaN(Scale) || double.IsInfinity(Scale))
                return;

            if (Scale == -1)
            {
                PART_Rect.Margin = new Thickness();
                PART_Rect.Width = 0;
                PART_Rect.Height = 0;
                return;
            }

            if (TargetViewer.IsMinFactor)
            {
                PART_Rect.Margin = new Thickness();
                PART_Rect.Width = PART_Container.ActualWidth;
                PART_Rect.Height = PART_Container.ActualHeight;
                return;
            }

            Rect Region = new Rect(Math.Round(e.NewValue.X * Scale),
                                   Math.Round(e.NewValue.Y * Scale),
                                   Math.Ceiling(e.NewValue.Width * Scale),
                                   Math.Ceiling(e.NewValue.Height * Scale));

            PART_Rect.Margin = new Thickness(Region.X, Region.Y, 0, 0);
            PART_Rect.Width = Region.Right > PART_Container.ActualWidth ? PART_Container.ActualWidth - Region.X : Region.Width;
            PART_Rect.Height = Region.Bottom > PART_Container.ActualHeight ? PART_Container.ActualHeight - Region.Y : Region.Height;
        }

        protected Int32Rect CalculateViewport(Int32Point Position)
        {
            Int32Rect TargetViewport = TargetViewer.Viewport,
                      TempViewport = new Int32Rect(Position.X - (TargetViewport.Width >> 1),
                                                   Position.Y - (TargetViewport.Height >> 1),
                                                   TargetViewport.Width,
                                                   TargetViewport.Height);

            TempViewport.X = Math.Min(Math.Max(0, TempViewport.X), ViewBox.Width - TempViewport.Width);
            TempViewport.Y = Math.Min(Math.Max(0, TempViewport.Y), ViewBox.Height - TempViewport.Height);

            return TempViewport;
        }

        protected void OnRenderImage()
        {
            if (PART_Container is null ||
                PART_Container.ActualWidth is 0 ||
                PART_Container.ActualHeight is 0)
                return;

            Int32Size ImageSize = new Int32Size(PART_Container.Width, PART_Container.Height);
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

        protected bool IsLeftMouseDown { set; get; } = false;
        protected Point Position { set; get; }
        private void OnContainerPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                PART_Container.CaptureMouse();
                IsLeftMouseDown = true;
                Position = e.GetPosition(PART_Container);

                if (!TargetViewer.IsMinFactor)
                    TargetViewer.Viewport = CalculateViewport(new Int32Point(
                        Math.Min(Math.Max(0, Position.X), PART_Container.ActualWidth) / Scale,
                        Math.Min(Math.Max(0, Position.Y), PART_Container.ActualHeight) / Scale));
            }
        }
        private void OnContainerPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                Point TempPosition = e.GetPosition(PART_Container);
                if (Position.Equals(TempPosition))
                    return;

                Position = TempPosition;

                if (!TargetViewer.IsMinFactor)
                    TargetViewer.Viewport = CalculateViewport(new Int32Point(
                        Math.Min(Math.Max(0, Position.X), PART_Container.ActualWidth) / Scale,
                        Math.Min(Math.Max(0, Position.Y), PART_Container.ActualHeight) / Scale));
            }
        }
        private void OnContainerPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                PART_Container.ReleaseMouseCapture();
                IsLeftMouseDown = false;
            }
        }

    }
}
