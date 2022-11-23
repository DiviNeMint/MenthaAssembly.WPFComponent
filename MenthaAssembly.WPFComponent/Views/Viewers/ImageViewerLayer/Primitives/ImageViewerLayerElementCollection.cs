using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace MenthaAssembly.Views.Primitives
{
    /// <summary>
    /// A ImageViewerLayerElementCollection is a ordered collection of ImageViewerLayerObject.
    /// </summary>
    public sealed class ImageViewerLayerElementCollection : ObservableCollection<ImageViewerLayerObject>
    {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        private readonly ImageViewerLayer Layer;
        private readonly UIElementCollection Elements;
        internal ImageViewerLayerElementCollection(ImageViewerLayer Layer, UIElementCollection Elements)
        {
            this.Layer = Layer;
            this.Elements = Elements;
        }

        protected override void InsertItem(int Index, ImageViewerLayerObject Item)
        {
            CheckReentrancy();
            PrepareImageViewerLayerObject(Item);

            Items.Insert(Index, Item);
            Elements.Insert(Index, Item);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Item, Index));
        }

        protected override void SetItem(int Index, ImageViewerLayerObject Item)
        {
            CheckReentrancy();
            PrepareImageViewerLayerObject(Item);

            ImageViewerLayerObject OldItem = Items[Index];
            try
            {
                Items[Index] = Item;
                Elements[Index] = Item;

                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, OldItem, Item, Index));
            }
            finally
            {
                ResetImageViewerLayerObject(OldItem);
            }
        }

        protected override void MoveItem(int OldIndex, int NewIndex)
        {
            CheckReentrancy();

            ImageViewerLayerObject Layer = this[OldIndex];

            int InsertIndex = OldIndex < NewIndex ? NewIndex : NewIndex;
            Items.RemoveAt(OldIndex);
            Items.Insert(InsertIndex, Layer);
            Elements.RemoveAt(OldIndex);
            Elements.Insert(InsertIndex, Layer);

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, Layer, NewIndex, OldIndex));
        }

        protected override void RemoveItem(int Index)
        {
            CheckReentrancy();

            ImageViewerLayerObject Item = Items[Index];
            try
            {
                Items.RemoveAt(Index);
                Elements.RemoveAt(Index);

                OnPropertyChanged(CountString);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Item, Index));
            }
            finally
            {
                ResetImageViewerLayerObject(Item);
            }
        }

        protected override void ClearItems()
        {
            CheckReentrancy();

            for (int i = Items.Count - 1; i >= 0; i--)
            {
                ImageViewerLayerObject Item = Items[i];
                Items.RemoveAt(i);

                ResetImageViewerLayerObject(Item);
            }

            Elements.Clear();
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void PrepareImageViewerLayerObject(ImageViewerLayerObject Object)
        {
            //Layer.StatusChanged += OnLayerStatusChanged;
            //Layer.ChannelChanged += OnLayerChannelChanged;
        }
        private void ResetImageViewerLayerObject(ImageViewerLayerObject Object)
        {
            //Layer.StatusChanged -= OnLayerStatusChanged;
            ////Layer.ChannelChanged -= OnLayerChannelChanged;

            //if (Layer.IsGeneratedFromCollection)
            //{
            //    Layer.ClearValue(FrameworkElement.DataContextProperty);
            //    Layer.ClearValue(ImageViewerLayerObject.SourceProperty);
            //    Layer.ClearValue(ImageViewerLayerObject.SourceContextProperty);

            //    //Layer.ClearValue(ImageViewerLayerObject.EnableLayerMarksProperty);
            //    Layer.Marks.Clear();
            //}
        }

        //protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        //{
        //    base.OnCollectionChanged(e);
        //    Layer._Attachments.ForEach(i => i.OnLayerCollectionChanged(e));
        //}

        //private void OnLayerStatusChanged(object sender, ImageViewerLayerObject e)
        //    => Layer._Attachments.ForEach(i => i.InvalidateCanvas());

        private void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => OnPropertyChanged(new PropertyChangedEventArgs(PropertyName));

    }
}
