using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views.Primitives.Adorners
{
    internal class SaturationBrightnessPickerAdorner : PickerAdorner
    {
        private static readonly Brush FillBrush = Brushes.Transparent;
        private static readonly Pen InnerRingPen = new Pen(Brushes.Black, 1);
        private static readonly Pen OuterRingPen = new Pen(Brushes.White, 3);

        public SaturationBrightnessPickerAdorner(UIElement Element) : base(Element)
        {
            this.IsClipEnabled = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.DrawEllipse(FillBrush, OuterRingPen, Position, 5, 5);
            drawingContext.DrawEllipse(FillBrush, InnerRingPen, Position, 5, 5);
        }

    }
}
