using System.Windows;

namespace MenthaAssembly.Views.Primitives
{
    internal sealed class ImageViewerLayerItemsElementPresenter : Panel<ImageViewerLayerItemsElement>
    {
        public ImageViewerLayerItemsElementPresenter(ImageViewerLayerItemsElement LogicalParent) : base(LogicalParent)
        {
            ClipToBounds = true;
        }

        protected override Size MeasureOverride(Size AvailableSize)
        {
            int Count = Children.Count;
            for (int i = 0; i < Count; i++)
            {
                UIElement Child = Children[i];
                if (Child.IsMeasureValid)
                    Child.InvalidateMeasure();

                Child.Measure(AvailableSize);
            }

            return AvailableSize;
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            Rect Rect = new(FinalSize);
            int Count = Children.Count;

            double Scale = LogicalParent.GetScale();
            LogicalParent.GetLayerPosition(0d, 0d, out double ILx, out double ILy);
            for (int i = 0; i < Count; i++)
            {
                UIElement Child = Children[i];
                if (Child is ImageViewerLayerElement Element)
                {
                    Point Location = Element.Location;
                    double Lx = Location.X * Scale + ILx,
                           Ly = Location.Y * Scale + ILy,
                           Ew, Eh, Px, Py;

                    if (double.IsNaN(Lx) || double.IsNaN(Ly))
                        continue;

                    Size ElementSize = Element.ZoomedDesiredSize;
                    Ew = ElementSize.Width;
                    Eh = ElementSize.Height;

                    Px = Element.HorizontalAlignment switch
                    {
                        HorizontalAlignment.Left => Lx,
                        HorizontalAlignment.Right => Lx - Ew,
                        _ => Lx - Ew / 2d,
                    };
                    Py = Element.VerticalAlignment switch
                    {
                        VerticalAlignment.Top => Ly,
                        VerticalAlignment.Bottom => Ly - Eh,
                        _ => Ly - Eh / 2d,
                    };

                    if (Element.IsArrangeValid)
                        Element.InvalidateArrange();

                    Element.Arrange(new Rect(Px, Py, Ew, Eh));
                }

                else
                {
                    if (Child.IsArrangeValid)
                        Child.InvalidateArrange();

                    Child.Arrange(Rect);
                }
            }

            return FinalSize;
        }

    }
}