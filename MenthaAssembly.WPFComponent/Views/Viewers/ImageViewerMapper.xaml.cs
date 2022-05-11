using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Views.Primitives;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public sealed unsafe class ImageViewerMapper : ImageViewerBase
    {
        public static readonly DependencyProperty TargetViewerProperty =
              DependencyProperty.Register("TargetViewer", typeof(ImageViewer), typeof(ImageViewerMapper), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is ImageViewerMapper This)
                      {
                          if (e.OldValue is ImageViewer OldViewer)
                          {
                              OldViewer.Layers.CollectionChanged -= This.OnAttachedLayersCollectionChanged;
                              OldViewer.ViewBoxChanged -= This.OnViewBoxChanged;
                              OldViewer.ViewportChanged -= This.OnViewportChanged;
                          }

                          if (e.NewValue is ImageViewer NewViewer)
                          {
                              NewViewer.Layers.CollectionChanged += This.OnAttachedLayersCollectionChanged;
                              NewViewer.ViewBoxChanged += This.OnViewBoxChanged;
                              NewViewer.ViewportChanged += This.OnViewportChanged;

                              This.UpdateContextDatas();
                              foreach (ImageViewerLayer AttachedLayer in NewViewer.Layers)
                              {
                                  if (!This.Layers.CachePool.TryDequeue(out ImageViewerLayer Layer))
                                      Layer = new ImageViewerLayer { IsGeneratedFromSystem = true };

                                  Layer.AttachedLayer = AttachedLayer;
                                  This.Layers.Add(Layer);
                              }
                          }

                          This.UpdateViewBoxContainerAndInternalScale();
                          This.UpdateViewportRect();
                      }
                  }));
        public ImageViewer TargetViewer
        {
            get => (ImageViewer)GetValue(TargetViewerProperty);
            set => SetValue(TargetViewerProperty, value);
        }

        public static readonly DependencyProperty ViewportStrokeProperty =
              DependencyProperty.Register("ViewportStroke", typeof(SolidColorBrush), typeof(ImageViewerMapper), new PropertyMetadata(default));
        public SolidColorBrush ViewportStroke
        {
            get => (SolidColorBrush)GetValue(ViewportStrokeProperty);
            set => SetValue(ViewportStrokeProperty, value);
        }

        public static readonly DependencyProperty ViewportFillProperty =
              DependencyProperty.Register("ViewportFill", typeof(SolidColorBrush), typeof(ImageViewerMapper), new PropertyMetadata(default));
        public SolidColorBrush ViewportFill
        {
            get => (SolidColorBrush)GetValue(ViewportFillProperty);
            set => SetValue(ViewportFillProperty, value);
        }

        private ImageViewerLayerCollection Layers { get; }

        static ImageViewerMapper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewerMapper), new FrameworkPropertyMetadata(typeof(ImageViewerMapper)));
        }

        public ImageViewerMapper()
        {
            Layers = new ImageViewerLayerCollection(this);
            RenderParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1 };
        }

        private Panel PART_Container, PART_LayerContainer;
        private Rectangle PART_Rect;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Container") is Panel PART_Container)
            {
                this.PART_Container = PART_Container;
                this.PART_Container.SetBinding(BackgroundProperty, new Binding($"{nameof(TargetViewer)}.Background") { Source = this });

                PART_Container.PreviewMouseWheel += OnContainerPreviewMouseWheel;
                PART_Container.PreviewMouseDown += OnContainerPreviewMouseDown;
                PART_Container.PreviewMouseMove += OnContainerPreviewMouseMove;
                PART_Container.PreviewMouseUp += OnContainerPreviewMouseUp;
            }

            if (GetTemplateChild("PART_Rect") is Rectangle PART_Rect)
                this.PART_Rect = PART_Rect;

            if (GetTemplateChild("PART_LayerContainer") is Panel PART_LayerContainer)
            {
                this.PART_LayerContainer = PART_LayerContainer;

                foreach (ImageViewerLayer Layer in Layers)
                    PART_LayerContainer.Children.Add(Layer);
            }

            Layers.CollectionChanged += OnLayersCollectionChanged;
            Layers.LayerSourceChanged += OnLayersSourceChanged;
        }

        protected override Size MeasureOverride(Size Constraint)
        {
            try
            {
                return base.MeasureOverride(Constraint);
            }
            finally
            {
                UpdateViewBoxContainerAndInternalScale();
                UpdateViewportRect();
            }
        }

        private void OnLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (ImageViewerLayer Layer in e.NewItems.OfType<ImageViewerLayer>())
                        {
                            PART_LayerContainer.Children.Add(Layer);
                            Layer.UpdateCanvas();
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (ImageViewerLayer Layer in e.OldItems.OfType<ImageViewerLayer>())
                            PART_LayerContainer.Children.Remove(Layer);
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (ImageViewerLayer Layer in e.NewItems.OfType<ImageViewerLayer>())
                        {
                            PART_LayerContainer.Children.Add(Layer);
                            Layer.UpdateCanvas();
                        }

                        foreach (ImageViewerLayer Layer in e.OldItems.OfType<ImageViewerLayer>())
                            PART_LayerContainer.Children.Remove(Layer);
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    PART_LayerContainer.Children.Clear();
                    break;
            }
        }

        private void OnLayersSourceChanged(object sender, ChangedEventArgs<IImageContext> e)
        {
            if (sender is ImageViewerLayer Layer)
                Layer.UpdateCanvas();
        }

        private void OnAttachedLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (ImageViewerLayer AttachedLayer in e.NewItems.OfType<ImageViewerLayer>())
                        {
                            if (!Layers.CachePool.TryDequeue(out ImageViewerLayer Layer))
                                Layer = new ImageViewerLayer { IsGeneratedFromSystem = true };

                            Layer.AttachedLayer = AttachedLayer;
                            Layers.Add(Layer);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (ImageViewerLayer AttachedLayer in e.OldItems.OfType<ImageViewerLayer>())
                        {
                            int Index = Layers.IndexOf(i => i.AttachedLayer.Equals(AttachedLayer));
                            if (Index > -1)
                                Layers.RemoveAt(Index);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (ImageViewerLayer AttachedLayer in e.OldItems.OfType<ImageViewerLayer>())
                        {
                            int Index = Layers.IndexOf(i => i.AttachedLayer.Equals(AttachedLayer));
                            if (Index > -1)
                                Layers.RemoveAt(Index);
                        }

                        foreach (ImageViewerLayer AttachedLayer in e.NewItems.OfType<ImageViewerLayer>())
                        {
                            if (!Layers.CachePool.TryDequeue(out ImageViewerLayer Layer))
                                Layer = new ImageViewerLayer { IsGeneratedFromSystem = true };

                            Layer.AttachedLayer = AttachedLayer;
                            Layers.Add(Layer);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    Layers.Clear();
                    break;
            }
        }

        private void OnViewBoxChanged(object sender, ChangedEventArgs<Size<int>> e)
        {
            ViewBox = e.NewValue;
            InternalViewport = new Rect(0, 0, ViewBox.Width, ViewBox.Height);
            UpdateViewBoxContainerAndInternalScale();
            UpdateContextDatas();
            UpdateLayerCanvas();
        }

        private void OnViewportChanged(object sender, ChangedEventArgs<Rect> e)
            => UpdateViewportRect();

        private void UpdateViewBoxContainerAndInternalScale()
        {
            if (PART_Container is null)
                return;

            if (TargetViewer is null ||
                TargetViewer.DisplayAreaWidth == 0d ||
                TargetViewer.DisplayAreaHeight == 0d ||
                DisplayAreaWidth == 0d ||
                DisplayAreaHeight == 0d)
            {
                PART_Container.Width = 0d;
                PART_Container.Height = 0d;
                InternalScale = 0d;
                return;
            }

            double ScaleWidth = DisplayAreaWidth / TargetViewer.DisplayAreaWidth,
                   ScaleHeight = DisplayAreaHeight / TargetViewer.DisplayAreaHeight,
                   Scale = ScaleWidth <= 0 ? ScaleHeight :
                                             ScaleHeight <= 0 ? ScaleWidth :
                                                                Math.Min(ScaleWidth, ScaleHeight);

            PART_Container.Width = TargetViewer.DisplayAreaWidth * Scale;
            PART_Container.Height = TargetViewer.DisplayAreaHeight * Scale;

            InternalScale = InternalViewport.Width == 0d || InternalViewport.Height == 0d ? 0d : PART_Container.Width / ViewBox.Width;
        }

        private void UpdateViewportRect()
        {
            if (PART_Rect is null)
                return;

            if (TargetViewer is null ||
                double.IsNaN(InternalScale) || double.IsInfinity(InternalScale) ||
                InternalScale <= 0d)
            {
                PART_Rect.Margin = new Thickness();
                PART_Rect.Width = 0;
                PART_Rect.Height = 0;
                return;
            }

            if (TargetViewer.IsMinScale)
            {
                PART_Rect.Margin = new Thickness();
                PART_Rect.Width = PART_Container.Width;
                PART_Rect.Height = PART_Container.Height;
                return;
            }

            Rect Viewport = TargetViewer.InternalViewport,
                 Region = new Rect(Math.Round(Viewport.X * InternalScale),
                                   Math.Round(Viewport.Y * InternalScale),
                                   Math.Ceiling(Viewport.Width * InternalScale),
                                   Math.Ceiling(Viewport.Height * InternalScale));

            PART_Rect.Margin = new Thickness(Region.X, Region.Y, 0, 0);
            PART_Rect.Width = Region.Right > PART_Container.ActualWidth ? PART_Container.ActualWidth - Region.X : Region.Width;
            PART_Rect.Height = Region.Bottom > PART_Container.ActualHeight ? PART_Container.ActualHeight - Region.Y : Region.Height;
        }

        private void UpdateContextDatas()
        {
            if (TargetViewer is null)
            {
                ContextX = 0;
                ContextY = 0;
                ContextWidth = 0;
                ContextHeight = 0;
            }
            else
            {
                ContextX = TargetViewer.ContextX;
                ContextY = TargetViewer.ContextY;
                ContextWidth = TargetViewer.ContextWidth;
                ContextHeight = TargetViewer.ContextHeight;
            }
        }

        private void UpdateLayerCanvas()
        {
            foreach (ImageViewerLayer Layer in Layers)
                Layer.UpdateCanvas();
        }

        private bool IsLeftMouseDown = false;
        private Point Position;
        private void OnContainerPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                PART_Container.CaptureMouse();
                IsLeftMouseDown = true;
                Position = e.GetPosition(PART_Container);

                if (TargetViewer != null &&
                    !TargetViewer.IsMinScale)
                    TargetViewer.Viewport = CalculateViewport(new Point(MathHelper.Clamp(Position.X, 0, PART_Container.ActualWidth) / InternalScale,
                                                                        MathHelper.Clamp(Position.Y, 0, PART_Container.ActualHeight) / InternalScale));
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

                if (TargetViewer != null &&
                    !TargetViewer.IsMinScale)
                    TargetViewer.Viewport = CalculateViewport(new Point(MathHelper.Clamp(Position.X, 0, PART_Container.ActualWidth) / InternalScale,
                                                                        MathHelper.Clamp(Position.Y, 0, PART_Container.ActualHeight) / InternalScale));
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
        private void OnContainerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
            => TargetViewer?.Zoom(e.Delta > 0);

        private Rect CalculateViewport(Point Position)
        {
            Rect TargetViewport = TargetViewer.Viewport,
                 TempViewport = new Rect(Position.X - TargetViewport.Width * 0.5,
                                         Position.Y - TargetViewport.Height * 0.5,
                                         TargetViewport.Width,
                                         TargetViewport.Height);

            TempViewport.X = MathHelper.Clamp(TempViewport.X, 0, ViewBox.Width - TempViewport.Width);
            TempViewport.Y = MathHelper.Clamp(TempViewport.Y, 0, ViewBox.Height - TempViewport.Height);

            return TempViewport;
        }

    }
}
