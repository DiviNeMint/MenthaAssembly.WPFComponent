using System.Collections.Specialized;

namespace MenthaAssembly.Views.Primitives
{
    public interface IImageViewerAttachment
    {
        public void InvalidateViewBox();

        public void InvalidateViewport();

        public void InvalidateMarks();

        public void InvalidateCanvas();

        public void OnLayerCollectionChanged(NotifyCollectionChangedEventArgs e);

    }
}