using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views.Primitives.Adorners
{
    internal class SaturationBrightnessPickerAdorner : PickerAdorner
    {
        private static readonly Brush FillBrush = Brushes.Transparent;
        private static readonly Pen InnerRingPen = new(Brushes.Black, 1);
        private static readonly Pen OuterRingPen = new(Brushes.White, 3);

        public SaturationBrightnessPickerAdorner(UIElement Element) : base(Element)
        {
            IsClipEnabled = true;
        }

        protected override void OnRender(DrawingContext Context)
        {
            base.OnRender(Context);

            Context.DrawEllipse(FillBrush, OuterRingPen, Position, 5, 5);
            Context.DrawEllipse(FillBrush, InnerRingPen, Position, 5, 5);
        }

    }
}