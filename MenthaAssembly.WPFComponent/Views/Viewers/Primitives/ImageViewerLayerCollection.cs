using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Utils;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views.Primitives
{
    public sealed class ImageViewerLayerCollection : ObservableCollection<ImageViewerLayer>
    {
        public event EventHandler<ChangedEventArgs<IImageContext>> LayerSourceChanged;

        public event EventHandler<ChangedEventArgs<ImageChannel>> LayerChannelChanged;

        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        internal readonly Pool<ImageViewerLayer> CachePool = new Pool<ImageViewerLayer>();

        private readonly ImageViewerBase ParentViewer;
        public ImageViewerLayerCollection(ImageViewerBase Viewer)
        {
            ParentViewer = Viewer;
        }

        private INotifyCollectionChanged ItemsSource;
        internal void SetItemsSource(IEnumerable ItemsSource)
        {
            if (this.ItemsSource != null)
                this.ItemsSource.CollectionChanged -= OnItemsSourceCollectionChanged;

            Clear();

            foreach (object Item in ItemsSource)
            {
                ImageViewerLayer Layer = CreateImageViewerLayer(Item);
                if (Layer != null)
                    Add(Layer);
            }

            if (ItemsSource is INotifyCollectionChanged NotifySource)
            {
                NotifySource.CollectionChanged += OnItemsSourceCollectionChanged;
                this.ItemsSource = NotifySource;
            }
        }

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (object Data in e.NewItems)
                        {
                            ImageViewerLayer Layer = CreateImageViewerLayer(Data);
                            if (Layer != null)
                                Add(Layer);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (object Data in e.OldItems)
                        {
                            int Index = Items.IndexOf(i => i.Equals(Data) || i.DataContext.Equals(Data));
                            if (Index > -1)
                                RemoveAt(Index);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (object Data in e.OldItems)
                        {
                            int Index = Items.IndexOf(i => i.Equals(Data) || i.DataContext.Equals(Data));
                            if (Index > -1)
                                RemoveAt(Index);
                        }

                        foreach (object Data in e.NewItems)
                        {
                            ImageViewerLayer Layer = CreateImageViewerLayer(Data);
                            if (Layer != null)
                                Add(Layer);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    break;
            }
        }

        protected override void InsertItem(int Index, ImageViewerLayer Item)
        {
            CheckReentrancy();
            PrepareImageViewerLayer(Item);

            Items.Insert(Index, Item);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Item, Index));
        }

        protected override void SetItem(int Index, ImageViewerLayer Item)
        {
            CheckReentrancy();
            PrepareImageViewerLayer(Item);

            ImageViewerLayer OldItem = Items[Index];
            try
            {
                Items[Index] = Item;

                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, OldItem, Item, Index));
            }
            finally
            {
                ResetImageViewerLayer(OldItem);
                if (OldItem.IsGeneratedFromSystem)
                    CachePool.Enqueue(OldItem);
            }
        }

        protected override void RemoveItem(int Index)
        {
            CheckReentrancy();

            ImageViewerLayer Item = Items[Index];
            try
            {
                Items.RemoveAt(Index);

                OnPropertyChanged(CountString);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Item, Index));
            }
            finally
            {
                ResetImageViewerLayer(Item);
                if (Item.IsGeneratedFromSystem)
                    CachePool.Enqueue(Item);
            }
        }

        protected override void ClearItems()
        {
            CheckReentrancy();

            for (int i = Items.Count - 1; i >= 0; i--)
            {
                ImageViewerLayer Item = Items[i];
                Items.RemoveAt(i);

                ResetImageViewerLayer(Item);
                if (Item.IsGeneratedFromSystem)
                    CachePool.Enqueue(Item);
            }

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private ImageViewerLayer CreateImageViewerLayer(object Context)
        {
            if (Context is ImageViewerLayer Layer)
                return Layer;

            if (Context is IImageContext ImageContext)
            {
                if (!CachePool.TryDequeue(out ImageViewerLayer NewLayer))
                    NewLayer = new ImageViewerLayer { IsGeneratedFromSystem = true };

                NewLayer.DataContext = Context;
                NewLayer.SourceContext = ImageContext;
                return NewLayer;
            }

            if (Context is ImageSource Image)
            {
                if (!CachePool.TryDequeue(out ImageViewerLayer NewLayer))
                    NewLayer = new ImageViewerLayer { IsGeneratedFromSystem = true };

                NewLayer.DataContext = Context;
                NewLayer.Source = Image;
                return NewLayer;
            }

            return null;
        }
        private void PrepareImageViewerLayer(ImageViewerLayer Layer)
        {
            Layer.Viewer = ParentViewer;
            Layer.SourceChanged += OnLayerSourceChanged;
            Layer.ChannelChanged += OnLayerChannelChanged;
        }
        private void ResetImageViewerLayer(ImageViewerLayer Layer)
        {
            Layer.Viewer = null;
            Layer.SourceChanged -= OnLayerSourceChanged;
            Layer.ChannelChanged -= OnLayerChannelChanged;

            if (Layer.IsGeneratedFromSystem)
            {
                Layer.ClearValue(FrameworkElement.DataContextProperty);
                Layer.ClearValue(ImageViewerLayer.SourceProperty);
                Layer.AttachedLayer = null;
                Layer.SourceContext = null;
            }
        }

        private void OnLayerSourceChanged(object sender, ChangedEventArgs<IImageContext> e)
            => LayerSourceChanged?.Invoke(sender, e);
        private void OnLayerChannelChanged(object sender, ChangedEventArgs<ImageChannel> e)
            => LayerChannelChanged?.Invoke(sender, e);

        private void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => OnPropertyChanged(new PropertyChangedEventArgs(PropertyName));

    }
}