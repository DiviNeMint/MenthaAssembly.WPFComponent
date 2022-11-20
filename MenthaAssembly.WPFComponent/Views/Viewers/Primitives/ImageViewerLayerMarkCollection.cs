using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MenthaAssembly.Views.Primitives
{
    public sealed class ImageViewerLayerMarkCollection : ObservableRangeCollection<ImageViewerLayerMark>
    {
        private readonly ImageViewerLayer Layer;
        internal ImageViewerLayerMarkCollection(ImageViewerLayer Layer)
        {
            this.Layer = Layer;
        }

        public override void AddRange(IEnumerable<ImageViewerLayerMark> Items)
        {
            base.AddRange(Items);

            foreach (ImageViewerLayerMark Item in Items)
                Item.VisualChanged += OnMarkVisualChanged;
        }

        protected override void InsertItem(int Index, ImageViewerLayerMark Item)
        {
            base.InsertItem(Index, Item);
            Item.VisualChanged += OnMarkVisualChanged;
        }

        protected override void SetItem(int Index, ImageViewerLayerMark Item)
        {
            ImageViewerLayerMark LastItem = Items[Index];
            LastItem.VisualChanged -= OnMarkVisualChanged;

            base.SetItem(Index, Item);
            Item.VisualChanged += OnMarkVisualChanged;
        }

        protected override void RemoveItem(int Index)
        {
            ImageViewerLayerMark Item = Items[Index];
            Item.VisualChanged -= OnMarkVisualChanged;

            base.RemoveItem(Index);
        }

        public override void RemoveRange(IEnumerable<ImageViewerLayerMark> Items)
        {
            foreach (ImageViewerLayerMark Item in Items)
                Item.VisualChanged -= OnMarkVisualChanged;

            base.RemoveRange(Items);
        }

        protected override void ClearItems()
        {
            foreach (ImageViewerLayerMark Item in Items)
                Item.VisualChanged -= OnMarkVisualChanged;

            base.ClearItems();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            Layer.InvalidateMarks();
        }

        private void OnMarkVisualChanged(object sender, EventArgs e)
            => Layer.InvalidateMarks();

    }
}