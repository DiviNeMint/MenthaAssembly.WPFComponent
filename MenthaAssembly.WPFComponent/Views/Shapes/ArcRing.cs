using System;
using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class ArcRing : FrameworkElement
    {
        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(ArcRing), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0xB4, 0xB4, 0xB4)),
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                (d, e) =>
                {
                    if (d is ArcRing This)
                        This.InvalidatePen();
                }));
        public Brush BorderBrush
        {
            get => (Brush)this.GetValue(BorderBrushProperty);
            set => this.SetValue(BorderBrushProperty, value);
        }

        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(double), typeof(ArcRing), new FrameworkPropertyMetadata(1d,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) =>
                {
                    if (d is ArcRing This)
                        This.InvalidatePen();
                }));
        public double BorderThickness
        {
            get => (double)this.GetValue(BorderThicknessProperty);
            set => this.SetValue(BorderThicknessProperty, value);
        }

        public static readonly DependencyProperty ArcRingThicknessProperty =
            DependencyProperty.Register("ArcRingThickness", typeof(double), typeof(ArcRing), new FrameworkPropertyMetadata(10d,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) =>
                {
                    if (d is ArcRing This)
                        This.InvalidatePen();
                }));
        public double ArcRingThickness
        {
            get => (double)this.GetValue(ArcRingThicknessProperty);
            set => this.SetValue(ArcRingThicknessProperty, value);
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(Brush), typeof(ArcRing), new FrameworkPropertyMetadata(Brushes.LightGray, FrameworkPropertyMetadataOptions.AffectsRender |
                                                                                                                                 FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public Brush Fill
        {
            get => (Brush)this.GetValue(FillProperty);
            set => this.SetValue(FillProperty, value);
        }

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(ArcRing), new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) =>
                {
                    if (d is ArcRing This)
                        This.InvalidatePen();
                }));
        public double StartAngle
        {
            get => (double)this.GetValue(StartAngleProperty);
            set => this.SetValue(StartAngleProperty, value);
        }

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register("EndAngle", typeof(double), typeof(ArcRing), new FrameworkPropertyMetadata(360d,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) =>
                {
                    if (d is ArcRing This)
                        This.InvalidatePen();
                }));
        public double EndAngle
        {
            get => (double)this.GetValue(EndAngleProperty);
            set => this.SetValue(EndAngleProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
            => dc.DrawGeometry(this.Fill, this.GetPen(), this.GetGeometry());

        protected Pen Pen;
        protected virtual Pen GetPen()
        {
            double Thickness = this.BorderThickness;
            if (double.IsInfinity(Thickness) || Thickness is 0d || double.IsNaN(Thickness))
                return null;

            Brush Brush = this.BorderBrush;
            if (Brush is null || Brush.Equals(Brushes.Transparent))
                return null;

            if (Pen == null)
            {
                // This pen is internal to the system and
                // must not participate in freezable treeness
                Pen = new Pen
                {
                    Thickness = Math.Abs(Thickness),
                    Brush = Brush,
                };

                //// StrokeDashArray is usually going to be its default value and GetValue
                //// on a mutable default has a per-instance cost associated with it so we'll
                //// try to avoid caching the default value
                //DoubleCollection strokeDashArray = null;
                //bool hasModifiers;
                //if (GetValueSource(StrokeDashArrayProperty, null, out hasModifiers)
                //    != BaseValueSourceInternal.Default || hasModifiers)
                //{
                //    strokeDashArray = StrokeDashArray;
                //}

                //// Avoid creating the DashStyle if we can
                //double strokeDashOffset = StrokeDashOffset;
                //if (strokeDashArray != null || strokeDashOffset != 0.0)
                //{
                //    _Pen.DashStyle = new DashStyle(strokeDashArray, strokeDashOffset);
                //}

            }

            return Pen;
        }
        public void InvalidatePen()
            => Pen = null;

        private Geometry Geometry;
        protected virtual Geometry GetGeometry()
        {
            double Radius = Math.Min(this.ActualWidth, this.ActualHeight) / 2d;
            if (double.IsInfinity(Radius) || Radius is 0 || double.IsNaN(Radius))
                return null;

            if (Geometry is null)
            {
                double LargeR = Math.Max(Radius - this.BorderThickness / 2, 0d),
                       SmallR = Math.Max(LargeR - this.ArcRingThickness, 0d),
                       RenderAngle = (double)((decimal)this.EndAngle % 360);

                StreamGeometry Temp = new StreamGeometry { FillRule = FillRule.EvenOdd };
                if (this.EndAngle != 0 && RenderAngle == 0)
                {
                    // Ring
                    Point P1 = new Point(Radius + LargeR, Radius),
                          P2 = new Point(Radius - LargeR, Radius),
                          P3 = new Point(Radius + SmallR, Radius),
                          P4 = new Point(Radius - SmallR, Radius);
                    Size LS = new Size(LargeR, LargeR),
                         SS = new Size(SmallR, SmallR);

                    using StreamGeometryContext Context = Temp.Open();
                    Context.BeginFigure(P2, true, true);
                    Context.ArcTo(P1, LS, 0, true, SweepDirection.Clockwise, true, true);
                    Context.ArcTo(P2, LS, 0, true, SweepDirection.Clockwise, true, true);
                    Context.BeginFigure(P4, true, true);
                    Context.ArcTo(P3, SS, 0, true, SweepDirection.Clockwise, true, true);
                    Context.ArcTo(P4, SS, 0, true, SweepDirection.Clockwise, true, true);
                }
                else
                {
                    // Arc
                    double Theta = this.StartAngle * MathHelper.UnitTheta,
                           Sin = Math.Sin(Theta),
                           Cos = Math.Cos(Theta);

                    Point P1 = new Point(LargeR * Sin + Radius, Radius - LargeR * Cos),
                          P3 = new Point(SmallR * Sin + Radius, Radius - SmallR * Cos);

                    Theta = (this.StartAngle + RenderAngle) * MathHelper.UnitTheta;
                    Sin = Math.Sin(Theta);
                    Cos = Math.Cos(Theta);

                    Point P2 = new Point(LargeR * Sin + Radius, Radius - LargeR * Cos),
                          P4 = new Point(SmallR * Sin + Radius, Radius - SmallR * Cos);

                    bool IsLargeArc = Math.Abs(RenderAngle) > 180;

                    using StreamGeometryContext Context = Temp.Open();
                    Context.BeginFigure(P1, true, true);
                    Context.ArcTo(P2, new Size(LargeR, LargeR), 0, IsLargeArc, RenderAngle > 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true, true);
                    Context.LineTo(P4, true, true);
                    Context.ArcTo(P3, new Size(SmallR, SmallR), 0, IsLargeArc, RenderAngle > 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise, true, true);
                }

                Temp.Freeze();
                Geometry = Temp;
            }

            return Geometry;
        }
        public void InvalidateGeometry()
            => Geometry = null;

    }
}
