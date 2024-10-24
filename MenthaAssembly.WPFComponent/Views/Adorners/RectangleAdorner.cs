using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public class RectangleAdorner : Adorner
    {
        private static readonly FrameworkPropertyMetadataOptions RenderMetadataOption = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender;

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(Point), typeof(RectangleAdorner),
                new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));
        public Point Position
        {
            get => (Point)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        public static readonly DependencyProperty StrokeProperty =
            Shape.StrokeProperty.AddOwner(typeof(RectangleAdorner),
                new FrameworkPropertyMetadata(Brushes.Red, RenderMetadataOption, OnPenChanged));
        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            Shape.StrokeThicknessProperty.AddOwner(typeof(RectangleAdorner),
                new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsMeasure | RenderMetadataOption, OnPenChanged));
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        private static void OnPenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RectangleAdorner This)
                This.InvalidatePen();
        }

        public RectangleAdorner(UIElement Element) : base(Element)
        {
            IsHitTestVisible = false;
            IsClipEnabled = true;
        }

        protected override void OnRender(DrawingContext Context)
        {
            base.OnRender(Context);

            // Draw
            Point Position = this.Position;
            Context.DrawRectangle(null, GetPen(), new Rect(Position.X, Position.Y, Width, Height));
        }

        private Pen Pen;
        protected virtual Pen GetPen()
        {
            if (Pen != null)
                return Pen;

            double Thickness = StrokeThickness;
            if (double.IsInfinity(Thickness) || Thickness is 0d || double.IsNaN(Thickness))
                return null;

            Brush Brush = Stroke;
            if (Brush is null || Brush.Equals(Brushes.Transparent))
                return null;

            // This pen is internal to the system and
            // must not participate in freezable treeness
            Pen = new Pen
            {
                Thickness = Math.Abs(Thickness),
                Brush = Brush,
            };

            Pen.Freeze();
            return Pen;
        }
        public void InvalidatePen()
            => Pen = null;

    }
}