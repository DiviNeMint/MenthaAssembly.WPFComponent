using MenthaAssembly.Views.Primitives;
using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class ImageViewerLayerLineElement : ImageViewerLayerShape
    {
        public static readonly DependencyProperty StartPointProperty =
              DependencyProperty.Register("StartPoint", typeof(Point), typeof(ImageViewerLayerLineElement),
                new FrameworkPropertyMetadata(new Point(double.NaN, double.NaN), FrameworkPropertyMetadataOptions.AffectsRender));
        public Point StartPoint
        {
            get => (Point)GetValue(StartPointProperty);
            set => SetValue(StartPointProperty, value);
        }

        public static readonly DependencyProperty EndPointProperty =
              DependencyProperty.Register("EndPoint", typeof(Point), typeof(ImageViewerLayerLineElement),
                new FrameworkPropertyMetadata(new Point(double.NaN, double.NaN), FrameworkPropertyMetadataOptions.AffectsRender));
        public Point EndPoint
        {
            get => (Point)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        protected override void OnRender(DrawingContext Context)
        {
            Point SP = StartPoint,
                  EP = EndPoint;

            TranslatePoint(this, out double Sx, out double Sy, SP.X, SP.Y);
            TranslatePoint(this, out double Ex, out double Ey, EP.X, EP.Y);

            if (double.IsNaN(Sx) || double.IsNaN(Sy) ||
                double.IsNaN(Ex) || double.IsNaN(Ey))
                return;

            if (GetPen() is Pen Pen)
                Context.DrawLine(Pen, new Point(Sx, Sy), new Point(Ex, Ey));

        }

    }
}