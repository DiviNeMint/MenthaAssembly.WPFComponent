using System.Windows;

namespace MenthaAssembly.Views.Primitives
{
    internal sealed class ImageViewerLayerItemsElementPresenter : Panel<ImageViewerLayerItemsElement>
    {
        public ImageViewerLayerItemsElementPresenter(ImageViewerLayerItemsElement LogicalParent) : base(LogicalParent)
        {
            ClipToBounds = true;
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
                    Size ElementSize = Element.DesiredSize;
                    Point Location = Element.Location;

                    double Lx = Location.X * Scale + ILx,
                           Ly = Location.Y * Scale + ILy;
                    Element.Arrange(new Rect(Lx - ElementSize.Width / 2d, Ly - ElementSize.Height / 2d, ElementSize.Width, ElementSize.Height));
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