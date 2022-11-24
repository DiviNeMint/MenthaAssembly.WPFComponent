using MenthaAssembly.Views.Primitives;
using System.Windows.Markup;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(Elements))]
    public class ImageViewerLayerElementPanel : ImageViewerLayerItemsElement
    {
        public new ImageViewerLayerElementCollection Elements
            => base.Elements;

    }
}