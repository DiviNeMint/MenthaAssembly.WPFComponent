using MenthaAssembly.Views.Primitives;
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(Content))]
    public class ImageViewerLayerContentElement : ImageViewerLayerElement
    {
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(UIElement), typeof(ImageViewerLayerContentElement),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) =>
                    {
                        if (d is ImageViewerLayerContentElement This)
                            This.OnContentChanged(e.ToChangedEventArgs<UIElement>());
                    }));
        public UIElement Content
        {
            get => (UIElement)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        protected override int VisualChildrenCount
            => Content is null ? 0 : 1;

        protected override Visual GetVisualChild(int Index)
            => Content is null || Index != 0 ? throw new IndexOutOfRangeException() : Content;

        protected override Size MeasureOverride(Size AvailableSize)
        {
            ZoomedDesiredSize = base.MeasureOverride(AvailableSize);

            if (Content is UIElement Child)
                Child.Measure(ZoomedDesiredSize);

            return ZoomedDesiredSize;
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            if (Content is UIElement Child)
            {
                Child.Arrange(new Rect(FinalSize));
                return Child.RenderSize;
            }

            return base.ArrangeOverride(FinalSize);
        }

        private void OnContentChanged(ChangedEventArgs<UIElement> e)
        {
            if (e.OldValue != null)
            {
                RemoveVisualChild(e.OldValue);
                RemoveLogicalChild(e.OldValue);
            }

            if (e.NewValue != null)
            {
                AddVisualChild(e.NewValue);
                AddLogicalChild(e.NewValue);
            }
        }

    }
}