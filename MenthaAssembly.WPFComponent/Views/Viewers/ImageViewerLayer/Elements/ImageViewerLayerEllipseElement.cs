using MenthaAssembly.Views.Primitives;
using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class ImageViewerLayerEllipseElement : ImageViewerLayerShape
    {
        public static readonly DependencyProperty RadiusAProperty =
            DependencyProperty.Register("RadiusA", typeof(double), typeof(ImageViewerLayerEllipseElement),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));
        public double RadiusA
        {
            get => (double)GetValue(RadiusAProperty);
            set => SetValue(RadiusAProperty, value);
        }

        public static readonly DependencyProperty RadiusBProperty =
            DependencyProperty.Register("RadiusB", typeof(double), typeof(ImageViewerLayerEllipseElement),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));
        public double RadiusB
        {
            get => (double)GetValue(RadiusBProperty);
            set => SetValue(RadiusBProperty, value);
        }

        protected override void OnRender(DrawingContext Context)
        {
            Point Center = Location;

            TranslatePoint(this, out double Cx, out double Cy, Center.X, Center.Y);
            if (double.IsNaN(Cx) || double.IsNaN(Cy))
                return;

            double Scale = GetScale();
            if (double.IsNaN(Scale) || double.IsNaN(Scale))
                return;

            Context.DrawEllipse(Fill, GetPen(), new(Cx, Cy), RadiusA * Scale, RadiusB * Scale);
        }

    }
}