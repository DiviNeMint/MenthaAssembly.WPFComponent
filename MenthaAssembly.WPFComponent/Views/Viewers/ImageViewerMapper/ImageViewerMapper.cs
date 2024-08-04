using MenthaAssembly.Views.Primitives;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public class ImageViewerMapper : FrameworkElement
    {
        public static readonly DependencyProperty BackgroundProperty =
            Border.BackgroundProperty.AddOwner(typeof(ImageViewerMapper),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E))));
        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty BorderBrushProperty =
            Border.BorderBrushProperty.AddOwner(typeof(ImageViewerMapper),
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x3F, 0x3F, 0x46))));
        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static readonly DependencyProperty BorderThicknessProperty =
            Border.BorderThicknessProperty.AddOwner(typeof(ImageViewerMapper),
                new PropertyMetadata(new Thickness(1)));
        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public static readonly DependencyProperty ViewerProperty =
                DependencyProperty.Register(nameof(Viewer), typeof(ImageViewer), typeof(ImageViewerMapper),
                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                        (d, e) =>
                        {
                            if (d is ImageViewerMapper This)
                                This.OnViewerChanged(e.ToChangedEventArgs<ImageViewer>());
                        }));
        public ImageViewer Viewer
        {
            get => (ImageViewer)GetValue(ViewerProperty);
            set => SetValue(ViewerProperty, value);
        }

        public static readonly DependencyProperty StrokeProperty =
            Shape.StrokeProperty.AddOwner(typeof(ImageViewerMapper), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x08, 0x7E, 0xCE))));
        public Brush Stroke
        {
            get => (SolidColorBrush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            Shape.StrokeThicknessProperty.AddOwner(typeof(ImageViewerMapper), new PropertyMetadata(1d));
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty FillProperty =
            Shape.FillProperty.AddOwner(typeof(ImageViewerMapper), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0x4C, 0x08, 0x7E, 0xCE))));
        public Brush Fill
        {
            get => (SolidColorBrush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        private readonly Border TemplateBorder;
        private readonly ImageViewerMapperPresenter TemplatePresenter;
        public ImageViewerMapper()
        {
            TemplatePresenter = new ImageViewerMapperPresenter(this);
            TemplateBorder = new Border { Child = TemplatePresenter };
            _ = TemplateBorder.SetBinding(BackgroundProperty, new Binding(nameof(Background)) { Source = this });
            _ = TemplateBorder.SetBinding(BorderBrushProperty, new Binding(nameof(BorderBrush)) { Source = this });
            _ = TemplateBorder.SetBinding(BorderThicknessProperty, new Binding(nameof(BorderThickness)) { Source = this });
            _ = TemplateBorder.SetBinding(SnapsToDevicePixelsProperty, new Binding(nameof(SnapsToDevicePixels)) { Source = this });
            _ = TemplateBorder.SetBinding(UseLayoutRoundingProperty, new Binding(nameof(UseLayoutRounding)) { Source = this });
            AddVisualChild(TemplateBorder);
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
            if (!IsLoaded && Viewer is null)
                Viewer = this.FindVisuaBrothers<ImageViewer>().FirstOrDefault();

            Rect Rect = new(FinalSize);
            TemplateBorder.Arrange(Rect);
            return base.ArrangeOverride(FinalSize);
        }

        private void OnViewerChanged(ChangedEventArgs<ImageViewer> e)
        {
            if (e.OldValue is ImageViewer Old)
                TemplatePresenter.Detach(Old);

            if (e.NewValue is ImageViewer New)
                TemplatePresenter.Attach(New);

            TemplatePresenter.InvalidateViewBox();
            TemplatePresenter.InvalidateCanvas();
        }

    }
}