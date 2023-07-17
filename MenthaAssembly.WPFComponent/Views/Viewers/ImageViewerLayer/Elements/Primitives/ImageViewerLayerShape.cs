using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MenthaAssembly.Views.Primitives
{
    public abstract class ImageViewerLayerShape : ImageViewerLayerElement
    {
        public static readonly DependencyProperty FillProperty =
            Shape.FillProperty.AddOwner(typeof(ImageViewerLayerShape),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        public static readonly DependencyProperty StrokeProperty =
            Shape.StrokeProperty.AddOwner(typeof(ImageViewerLayerShape),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                    FrameworkPropertyMetadataOptions.AffectsRender |
                                                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender, OnPenChanged));
        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            Shape.StrokeThicknessProperty.AddOwner(typeof(ImageViewerLayerShape),
                new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty StrokeStartLineCapProperty =
            Shape.StrokeStartLineCapProperty.AddOwner(typeof(ImageViewerLayerShape),
                new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                               FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));
        public PenLineCap StrokeStartLineCap
        {
            get => (PenLineCap)GetValue(StrokeStartLineCapProperty);
            set => SetValue(StrokeStartLineCapProperty, value);
        }

        public static readonly DependencyProperty StrokeEndLineCapProperty =
            Shape.StrokeEndLineCapProperty.AddOwner(typeof(ImageViewerLayerShape),
                new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));
        public PenLineCap StrokeEndLineCap
        {
            get => (PenLineCap)GetValue(StrokeEndLineCapProperty);
            set => SetValue(StrokeEndLineCapProperty, value);
        }

        public static readonly DependencyProperty StrokeDashCapProperty =
            Shape.StrokeDashCapProperty.AddOwner(typeof(ImageViewerLayerShape),
                new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));
        public PenLineCap StrokeDashCap
        {
            get => (PenLineCap)GetValue(StrokeDashCapProperty);
            set => SetValue(StrokeDashCapProperty, value);
        }

        public static readonly DependencyProperty StrokeLineJoinProperty =
            Shape.StrokeLineJoinProperty.AddOwner(typeof(ImageViewerLayerShape),
                new FrameworkPropertyMetadata(PenLineJoin.Miter, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));
        public PenLineJoin StrokeLineJoin
        {
            get => (PenLineJoin)GetValue(StrokeLineJoinProperty);
            set => SetValue(StrokeLineJoinProperty, value);
        }

        public static readonly DependencyProperty StrokeMiterLimitProperty =
            Shape.StrokeMiterLimitProperty.AddOwner(typeof(ImageViewerLayerShape),
                new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));
        public double StrokeMiterLimit
        {
            get => (double)GetValue(StrokeMiterLimitProperty);
            set => SetValue(StrokeMiterLimitProperty, value);
        }

        public static readonly DependencyProperty StrokeDashOffsetProperty =
            Shape.StrokeDashOffsetProperty.AddOwner(typeof(ImageViewerLayerShape),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));
        public double StrokeDashOffset
        {
            get => (double)GetValue(StrokeDashOffsetProperty);
            set => SetValue(StrokeDashOffsetProperty, value);
        }

        public static readonly DependencyProperty StrokeDashArrayProperty =
            Shape.StrokeDashArrayProperty.AddOwner(typeof(ImageViewerLayerShape),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));
        public DoubleCollection StrokeDashArray
        {
            get => (DoubleCollection)GetValue(StrokeDashArrayProperty);
            set => SetValue(StrokeDashArrayProperty, value);
        }

        private static void OnPenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageViewerLayerShape This)
                This.InvalidatePen();
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            InvalidateVisual();
            return base.ArrangeOverride(FinalSize);
        }

        private Pen Pen = null;
        protected virtual Pen GetPen()
        {
            Brush Stroke = this.Stroke;
            if (Stroke is null)
                return null;

            double StrokeThickness = this.StrokeThickness;
            if (double.IsNaN(StrokeThickness) || StrokeThickness == 0d)
                return null;

            if (Pen is null)
            {
                Pen = new Pen(Stroke, Math.Abs(StrokeThickness))
                {
                    //CanBeInheritanceContext = false,
                    StartLineCap = StrokeStartLineCap,
                    EndLineCap = StrokeEndLineCap,
                    DashCap = StrokeDashCap,
                    LineJoin = StrokeLineJoin,
                    MiterLimit = StrokeMiterLimit
                };

                if (StrokeDashArray is DoubleCollection DashArray)
                    Pen.DashStyle = new DashStyle(DashArray, StrokeDashOffset);
            }

            return Pen;
        }
        protected void InvalidatePen()
            => Pen = null;

    }
}