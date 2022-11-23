using System.Windows;

namespace MenthaAssembly.Views.Primitives
{
    internal sealed class ImageViewerLayerPresenter : Panel<ImageViewerLayer>
    {
        public ImageViewerLayerPresenter(ImageViewerLayer LogicalParent) : base(LogicalParent)
        {
            ClipToBounds = true;
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            Rect Rect = new(FinalSize);
            int Count = Children.Count;
            for (int i = 0; i < Count; i++)
            {
                UIElement Child = Children[i];
                if (Child is ImageViewerLayerElement Element)
                {
                    Size ElementSize = Element.DesiredSize;
                    Point Location = Element.Location;
                    LogicalParent.Renderer.GetLayerPosition(Location.X, Location.Y, out double Lx, out double Ly);
                    Element.Arrange(new Rect(Lx - ElementSize.Width / 2d, Ly - ElementSize.Height / 2d, ElementSize.Width, ElementSize.Height));
                }

                else
                {
                    Child.Arrange(Rect);
                }
            }

            return FinalSize;
        }

    }
}