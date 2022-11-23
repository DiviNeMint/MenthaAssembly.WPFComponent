using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly.Views.Primitives
{
    internal sealed class ImageViewerLayerPresenter : Panel
    {
        private readonly ImageViewerLayer LogicalParent;
        public ImageViewerLayerPresenter(ImageViewerLayer LogicalParent)
        {
            this.LogicalParent = LogicalParent;
            ClipToBounds = true;
        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement Parent)
            => base.CreateUIElementCollection(LogicalParent);

        protected override Size MeasureOverride(Size AvailableSize)
        {
            int Count = Children.Count;
            for (int i = 0; i < Count; i++)
                Children[i].Measure(AvailableSize);

            return base.MeasureOverride(AvailableSize);
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

            return base.ArrangeOverride(FinalSize);
        }

    }
}