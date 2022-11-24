using MenthaAssembly.Views.Primitives;
using System;
using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public abstract class ImageViewerLayerItemsElement : ImageViewerLayerObject
    {
        protected ImageViewerLayerElementCollection Elements { get; }

        private readonly ImageViewerLayerItemsElementPresenter Presenter;
        public ImageViewerLayerItemsElement()
        {
            Presenter = new ImageViewerLayerItemsElementPresenter(this);
            Elements = new ImageViewerLayerElementCollection(this, Presenter.Children);

            AddVisualChild(Presenter);
        }

        protected override int VisualChildrenCount
            => 1;

        protected override Visual GetVisualChild(int Index)
            => Index == 0 ? Presenter : throw new IndexOutOfRangeException();

        protected override Size MeasureOverride(Size AvailableSize)
        {
            Presenter.Measure(AvailableSize);
            return base.MeasureOverride(AvailableSize);
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            if (Presenter.IsArrangeValid)
                Presenter.InvalidateArrange();

            Presenter.Arrange(new Rect(FinalSize));
            return base.ArrangeOverride(FinalSize);
        }

    }
}