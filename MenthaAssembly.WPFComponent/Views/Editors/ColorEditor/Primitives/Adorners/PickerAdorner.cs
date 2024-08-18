using System.Windows;
using System.Windows.Documents;

namespace MenthaAssembly.Views.Primitives.Adorners
{
    internal abstract class PickerAdorner : Adorner
    {
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(Point), typeof(PickerAdorner),
                new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsRender));
        public Point Position
        {
            get => (Point)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        protected PickerAdorner(UIElement Element) : base(Element)
        {
            IsHitTestVisible = false;
        }

    }
}