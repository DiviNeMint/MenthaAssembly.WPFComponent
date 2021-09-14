using System;
using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public abstract class LineChartAuxiliaryLine : FrameworkElement
    {
        public static readonly DependencyProperty AxisProperty =
              DependencyProperty.Register("Axis", typeof(Axises), typeof(LineChartAuxiliaryLine), new FrameworkPropertyMetadata(Axises.Y, FrameworkPropertyMetadataOptions.AffectsRender));
        public Axises Axis
        {
            get => (Axises)this.GetValue(AxisProperty);
            set => this.SetValue(AxisProperty, value);
        }

        #region Stroke
        public static readonly DependencyProperty StrokeProperty =
              DependencyProperty.Register("Stroke", typeof(Brush), typeof(LineChartAuxiliaryLine), new FrameworkPropertyMetadata(Brushes.Red,
                                                                                                                                 FrameworkPropertyMetadataOptions.AffectsRender |
                                                                                                                                 FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                                                                                                                                 OnPenChanged));
        public Brush Stroke
        {
            get => (Brush)this.GetValue(StrokeProperty);
            set => this.SetValue(StrokeProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
              DependencyProperty.Register("StrokeThickness", typeof(double), typeof(LineChartAuxiliaryLine), new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));
        public double StrokeThickness
        {
            get => (double)this.GetValue(StrokeThicknessProperty);
            set => this.SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty StrokeStartLineCapProperty =
              DependencyProperty.Register("StrokeStartLineCap", typeof(PenLineCap), typeof(LineChartAuxiliaryLine), new FrameworkPropertyMetadata(PenLineCap.Flat,
                                                                                                                                                  FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                                  FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                                  OnPenChanged));
        public PenLineCap StrokeStartLineCap
        {
            get => (PenLineCap)this.GetValue(StrokeStartLineCapProperty);
            set => this.SetValue(StrokeStartLineCapProperty, value);
        }

        public static readonly DependencyProperty StrokeEndLineCapProperty =
                DependencyProperty.Register("StrokeEndLineCap", typeof(PenLineCap), typeof(LineChartAuxiliaryLine), new FrameworkPropertyMetadata(PenLineCap.Flat,
                                                                                                                                                  FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                                  FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                                  OnPenChanged));
        public PenLineCap StrokeEndLineCap
        {
            get => (PenLineCap)this.GetValue(StrokeEndLineCapProperty);
            set => this.SetValue(StrokeEndLineCapProperty, value);
        }

        public static readonly DependencyProperty StrokeDashCapProperty =
                DependencyProperty.Register("StrokeDashCap", typeof(PenLineCap), typeof(LineChartAuxiliaryLine), new FrameworkPropertyMetadata(PenLineCap.Flat,
                                                                                                                                               FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                               FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                               OnPenChanged));
        public PenLineCap StrokeDashCap
        {
            get => (PenLineCap)this.GetValue(StrokeDashCapProperty);
            set => this.SetValue(StrokeDashCapProperty, value);
        }

        public static readonly DependencyProperty StrokeLineJoinProperty =
                DependencyProperty.Register("StrokeLineJoin", typeof(PenLineJoin), typeof(LineChartAuxiliaryLine), new FrameworkPropertyMetadata(PenLineJoin.Miter,
                                                                                                                                                 FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                                 FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                                 OnPenChanged));
        public PenLineJoin StrokeLineJoin
        {
            get => (PenLineJoin)this.GetValue(StrokeLineJoinProperty);
            set => this.SetValue(StrokeLineJoinProperty, value);
        }

        public static readonly DependencyProperty StrokeDashOffsetProperty =
                DependencyProperty.Register("StrokeDashOffset", typeof(double), typeof(LineChartAuxiliaryLine), new FrameworkPropertyMetadata(0d,
                                                                                                                                              FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                              FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                              OnPenChanged));
        public double StrokeDashOffset
        {
            get => (double)this.GetValue(StrokeDashOffsetProperty);
            set => this.SetValue(StrokeDashOffsetProperty, value);
        }

        public static readonly DependencyProperty StrokeDashArrayProperty =
                DependencyProperty.Register("StrokeDashArray", typeof(DoubleCollection), typeof(LineChartAuxiliaryLine), new FrameworkPropertyMetadata(null,
                                                                                                                                                       FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                                       FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                                       OnPenChanged));
        public DoubleCollection StrokeDashArray
        {
            get => (DoubleCollection)this.GetValue(StrokeDashArrayProperty);
            set => this.SetValue(StrokeDashArrayProperty, value);
        }

        private static void OnPenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LineChartAuxiliaryLine ThisLine)
                ThisLine.InvalidatePen();
        }

        #endregion

        public LineChart Chart { internal set; get; }

        static LineChartAuxiliaryLine()
        {
            IsHitTestVisibleProperty.OverrideMetadata(typeof(LineChartAuxiliaryLine), new FrameworkPropertyMetadata(false));
        }

        public abstract void Update(LineChartItem Item);

        protected Pen Pen;
        protected virtual Pen GetPen()
        {
            double Thickness = this.StrokeThickness;
            if (double.IsInfinity(Thickness) || Thickness is 0d || double.IsNaN(Thickness))
                return null;

            Brush Brush = this.Stroke;
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
                    StartLineCap = StrokeStartLineCap,
                    EndLineCap = StrokeEndLineCap,
                    DashCap = StrokeDashCap,
                    LineJoin = StrokeLineJoin,
                };

                if (this.StrokeDashArray is DoubleCollection DashData && DashData.Count > 0)
                    Pen.DashStyle = new DashStyle(DashData, this.StrokeDashOffset);
            }

            return Pen;
        }
        public void InvalidatePen()
            => Pen = null;

    }
}