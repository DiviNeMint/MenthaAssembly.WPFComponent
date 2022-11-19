using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly.Views.Primitives
{
    internal sealed class ImageViewerPresenter : Panel
    {
        private readonly ImageViewer LogicalParent;
        public ImageViewerPresenter(ImageViewer LogicalParent)
        {
            this.LogicalParent = LogicalParent;
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
            LogicalParent.Manager.Add(ImageViewerAction.ComputeViewBox);

            Rect Rect = new(FinalSize);
            int Count = Children.Count;
            for (int i = 0; i < Count; i++)
                Children[i].Arrange(Rect);

            return base.ArrangeOverride(FinalSize);
        }

    }
}