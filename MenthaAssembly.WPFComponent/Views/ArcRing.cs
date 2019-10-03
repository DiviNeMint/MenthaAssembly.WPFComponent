using System;
using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class ArcRing : FrameworkElement
    {
        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(ArcRing), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xB4, 0xB4, 0xB4)), (d, e) => (d as UIElement).InvalidateVisual()));
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(double), typeof(ArcRing), new PropertyMetadata(1d, (d, e) => (d as UIElement).InvalidateVisual()));
        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty ArcRingThicknessProperty =
            DependencyProperty.Register("ArcRingThickness", typeof(double), typeof(ArcRing), new PropertyMetadata(10d, (d, e) => (d as UIElement).InvalidateVisual()));
        public double ArcRingThickness
        {
            get { return (double)GetValue(ArcRingThicknessProperty); }
            set { SetValue(ArcRingThicknessProperty, value); }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(ArcRing), new PropertyMetadata(Brushes.LightGray, (d, e) => (d as UIElement).InvalidateVisual()));
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(ArcRing), new PropertyMetadata(0d, (d, e) => (d as UIElement).InvalidateVisual()));
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(ArcRing), new PropertyMetadata(360d, (d, e) => (d as UIElement).InvalidateVisual()));
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        private decimal RenderAngle = 0;
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // Calculate
            double Radius = Math.Min(ActualWidth, ActualHeight) / 2;
            double LargeR = Math.Max(Radius - BorderThickness / 2, 0d);
            double SmallR = Math.Max(LargeR - ArcRingThickness, 0d);
            Point CenterPoint = new Point(Radius, Radius);
            RenderAngle = (decimal)Angle % 360;

            // Draw
            dc.DrawGeometry(
                Fill,
                new Pen(BorderBrush, BorderThickness),
                Angle != 0 && RenderAngle == 0 ? DrawingRing(CenterPoint, LargeR, SmallR) : DrawingArc(CenterPoint, LargeR, SmallR));
        }

        protected Geometry DrawingArc(Point _Center, double _LargeR, double _SmallR)
        {
            StreamGeometry Result = new StreamGeometry
            {
                FillRule = FillRule.EvenOdd
            };

            Point P1 = CalculateArcPoint(_Center, _LargeR, StartAngle);
            Point P2 = CalculateArcPoint(_Center, _LargeR, StartAngle + (double)RenderAngle);
            Point P3 = CalculateArcPoint(_Center, _SmallR, StartAngle);
            Point P4 = CalculateArcPoint(_Center, _SmallR, StartAngle + (double)RenderAngle);

            bool _IsLargeArc = Math.Abs(RenderAngle) > 180;
            using (StreamGeometryContext ctx = Result.Open())
            {
                ctx.BeginFigure(P1, true, true);
                ctx.ArcTo(P2, new Size(_LargeR, _LargeR), 0, _IsLargeArc, RenderAngle > 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true, true);
                ctx.LineTo(P4, true, true);
                ctx.ArcTo(P3, new Size(_SmallR, _SmallR), 0, _IsLargeArc, RenderAngle > 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise, true, true);
            }
            Result.Freeze();
            return Result;
        }

        protected Geometry DrawingRing(Point _Center, double _LargeR, double _SmallR)
        {
            StreamGeometry Result = new StreamGeometry
            {
                FillRule = FillRule.EvenOdd
            };

            Point P1 = new Point(_Center.X + _LargeR, _Center.Y);
            Point P2 = new Point(_Center.X - _LargeR, _Center.Y);
            Point P3 = new Point(_Center.X + _SmallR, _Center.Y);
            Point P4 = new Point(_Center.X - _SmallR, _Center.Y);

            using (StreamGeometryContext ctx = Result.Open())
            {
                ctx.BeginFigure(P2, true, true);
                ctx.ArcTo(P1, new Size(_LargeR, _LargeR), 0, true, SweepDirection.Clockwise, true, true);
                ctx.ArcTo(P2, new Size(_LargeR, _LargeR), 0, true, SweepDirection.Clockwise, true, true);
                ctx.BeginFigure(P4, true, true);
                ctx.ArcTo(P3, new Size(_SmallR, _SmallR), 0, true, SweepDirection.Clockwise, true, true);
                ctx.ArcTo(P4, new Size(_SmallR, _SmallR), 0, true, SweepDirection.Clockwise, true, true);
            }
            Result.Freeze();
            return Result;
        }

        public static Point CalculateArcPoint(Point _CenterPoint, double _Radius, double _Angle)
            => new Point(Math.Sin(_Angle * Math.PI / 180) * _Radius + _CenterPoint.X, _CenterPoint.Y - Math.Cos(_Angle * Math.PI / 180) * _Radius);

    }
}
