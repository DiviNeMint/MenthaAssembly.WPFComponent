using System.Windows;

namespace MenthaAssembly.Views.Primitives
{
    internal sealed class ImageViewerPresenter : Panel<ImageViewer>
    {
        public ImageViewerPresenter(ImageViewer LogicalParent) : base(LogicalParent)
        {

        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            LogicalParent.Manager.Add(ImageViewerAction.ComputeViewBox);
            return base.ArrangeOverride(FinalSize);
        }

    }
}