using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views.Primitives.Adorners
{
    internal class HuePickerAdorner : PickerAdorner
    {
        private static readonly Pen InnerBorderPen = new Pen(Brushes.Black, 1);
        private static readonly Pen OuterBorderPen = new Pen(Brushes.White, 3);
        public HuePickerAdorner(UIElement Element) : base(Element)
        {
            this.IsClipEnabled = true;
        }

        protected override void OnRender(DrawingContext Context)
        {
            base.OnRender(Context);

            // Calculate
            double Length = this.ActualWidth * .2;
            PathFigure[] Paths =
            {
                // Left
                new PathFigure(new Point(Length, Position.Y),
                               new PathSegment[]
                               {
                                   new LineSegment(new Point(0, Position.Y - Length), true),
                                   new LineSegment(new Point(0, Position.Y + Length), true)
                               },
                               true),
                // Right
                new PathFigure(new Point(this.ActualWidth - Length, Position.Y),
                               new PathSegment[]
                               {
                                   new LineSegment(new Point(this.ActualWidth, Position.Y - Length), true),
                                   new LineSegment(new Point(this.ActualWidth, Position.Y + Length), true)
                               },
                               true)
            };
            PathGeometry Geometry = new PathGeometry(Paths);

            // Draw
            Context.DrawGeometry(null, OuterBorderPen, Geometry);
            Context.DrawGeometry(Brushes.Black, InnerBorderPen, Geometry);
        }
    }
}
