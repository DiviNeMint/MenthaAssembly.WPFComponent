using MenthaAssembly.Media.Imaging;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MenthaAssembly.Views.Primitives
{
    /// <summary>
    /// A ImageViewerLayerCollection is a ordered collection of ImageViewerLayer.
    /// </summary>
    public sealed class ImageViewerLayerCollection : ObservableCollection<ImageViewerLayer>
    {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        private readonly ImageViewer Viewer;
        private readonly UIElementCollection Layers;
        internal ImageViewerLayerCollection(ImageViewer Viewer, UIElementCollection Layers)
        {
            this.Viewer = Viewer;
            this.Layers = Layers;
        }

        private bool IsUsingItemsSource = false;
        private INotifyCollectionChanged ItemsSource;
        internal void SetItemsSource(IEnumerable ItemsSource)
        {
            if (this.ItemsSource != null)
                this.ItemsSource.CollectionChanged -= OnItemsSourceCollectionChanged;

            InternalClearItems();

            if (ItemsSource is null)
            {
                IsUsingItemsSource = false;
                return;
            }

            int Count = Items.Count;
            foreach (object Item in ItemsSource.Where(i => i != null))
                if (CreateImageViewerLayer(Item) is ImageViewerLayer Layer)
                    InternalInsertItem(Count++, Layer);

            if (ItemsSource is INotifyCollectionChanged NotifySource)
            {
                NotifySource.CollectionChanged += OnItemsSourceCollectionChanged;
                this.ItemsSource = NotifySource;
            }

            IsUsingItemsSource = true;
        }
        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        int Count = Items.Count;
                        foreach (object Data in e.NewItems.Where(i => i != null))
                            if (CreateImageViewerLayer(Data) is ImageViewerLayer Layer)
                                InternalInsertItem(Count++, Layer);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (object Data in e.OldItems.Where(i => i != null))
                        {
                            int Index = Items.IndexOf(i => Data.Equals(i) || Data.Equals(i.DataContext));
                            if (Index > -1)
                                InternalRemoveItem(Index);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        object Data;
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            Data = e.OldItems[i];
                            if (Data is null)
                                continue;

                            int Index = Items.IndexOf(i => Data.Equals(i) || Data.Equals(i.DataContext));
                            if (Index > -1)
                            {
                                Data = e.NewItems[i];
                                if (Data != null && CreateImageViewerLayer(Data) is ImageViewerLayer Layer)
                                    InternalSetItem(Index, Layer);
                                else
                                    InternalRemoveItem(Index);
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Move:
                    {
                        if (sender is IEnumerable Enumerable)
                        {
                            int InsertIndex = e.NewStartingIndex,
                                LastIndex = e.OldStartingIndex,
                                i = 0,
                                Count = e.NewItems.Count;

                            object Data;
                            if (InsertIndex < LastIndex)
                            {
                                // Check the amount of item between oldindex and newindex.
                                int Length = LastIndex - InsertIndex;

                                // Find the InsertIndex of Layers.
                                InsertIndex = -1;
                                for (; i < Count; i++)
                                {
                                    Data = e.NewItems[i];
                                    if (Data is null)
                                        continue;

                                    int Index = Items.IndexOf(i => Data.Equals(i) || Data.Equals(i.DataContext));
                                    if (Index > -1)
                                    {
                                        foreach (object Item in Enumerable.Skip(LastIndex)
                                                                          .Take(Length)
                                                                          .Where(i => i != null))
                                        {
                                            InsertIndex = Items.IndexOf(i => Item.Equals(i) || Item.Equals(i.DataContext));
                                            if (InsertIndex > -1)
                                                break;
                                        }

                                        // Checks it need to be Inserted.
                                        if (InsertIndex < 0)
                                            return;

                                        i++;
                                        InternalMoveItem(Index, --InsertIndex);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                // Check the amount of item between oldindex and newindex.
                                int Length = InsertIndex - LastIndex - Count;
                                if (Length == 0)
                                    return;

                                // Find the InsertIndex of Layers.
                                InsertIndex = -1;
                                for (; i < Count; i++)
                                {
                                    Data = e.NewItems[i];
                                    if (Data is null)
                                        continue;

                                    int Index = Items.IndexOf(i => Data.Equals(i) || Data.Equals(i.DataContext));
                                    if (Index > -1)
                                    {
                                        foreach (object Item in Enumerable.Skip(LastIndex)
                                                                          .Take(Length)
                                                                          .Where(i => i != null)
                                                                          .Reverse())
                                        {
                                            InsertIndex = Items.IndexOf(i => Item.Equals(i) || Item.Equals(i.DataContext));
                                            if (InsertIndex > -1)
                                                break;
                                        }

                                        // Checks it need to be Inserted.
                                        if (InsertIndex < 0)
                                            return;

                                        i++;
                                        InternalMoveItem(Index, ++InsertIndex);
                                        break;
                                    }
                                }
                            }

                            for (; i < Count; i++)
                            {
                                Data = e.NewItems[i];
                                if (Data is null)
                                    continue;

                                int Index = Items.IndexOf(i => Data.Equals(i) || Data.Equals(i.DataContext));
                                if (Index > -1)
                                    InternalMoveItem(Index, ++InsertIndex);
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    InternalClearItems();
                    break;
            }
        }

        protected override void InsertItem(int Index, ImageViewerLayer Item)
        {
            CheckIsUsingInnerView();
            InternalInsertItem(Index, Item);
        }
        private void InternalInsertItem(int Index, ImageViewerLayer Item)
        {
            CheckReentrancy();
            PrepareImageViewerLayer(Item);

            Items.Insert(Index, Item);
            Layers.Insert(Index, Item);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Item, Index));
        }

        protected override void SetItem(int Index, ImageViewerLayer Item)
        {
            CheckIsUsingInnerView();
            InternalSetItem(Index, Item);
        }
        private void InternalSetItem(int Index, ImageViewerLayer Item)
        {
            CheckReentrancy();
            PrepareImageViewerLayer(Item);

            ImageViewerLayer OldItem = Items[Index];
            try
            {
                Items[Index] = Item;
                Layers[Index] = Item;

                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, OldItem, Item, Index));
            }
            finally
            {
                ResetImageViewerLayer(OldItem);
            }
        }

        protected override void MoveItem(int OldIndex, int NewIndex)
        {
            CheckIsUsingInnerView();
            InternalMoveItem(OldIndex, NewIndex);
        }
        private void InternalMoveItem(int OldIndex, int NewIndex)
        {
            CheckReentrancy();

            ImageViewerLayer Layer = this[OldIndex];

            int InsertIndex = OldIndex < NewIndex ? NewIndex : NewIndex;
            Items.RemoveAt(OldIndex);
            Items.Insert(InsertIndex, Layer);
            Layers.RemoveAt(OldIndex);
            Layers.Insert(InsertIndex, Layer);

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, Layer, NewIndex, OldIndex));
        }

        protected override void RemoveItem(int Index)
        {
            CheckIsUsingInnerView();
            InternalRemoveItem(Index);
        }
        private void InternalRemoveItem(int Index)
        {
            CheckReentrancy();

            ImageViewerLayer Item = Items[Index];
            try
            {
                Items.RemoveAt(Index);
                Layers.RemoveAt(Index);

                OnPropertyChanged(CountString);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Item, Index));
            }
            finally
            {
                ResetImageViewerLayer(Item);
            }
        }

        protected override void ClearItems()
        {
            CheckIsUsingInnerView();
            InternalClearItems();
        }
        private void InternalClearItems()
        {
            CheckReentrancy();

            for (int i = Items.Count - 1; i >= 0; i--)
            {
                ImageViewerLayer Item = Items[i];
                Items.RemoveAt(i);

                ResetImageViewerLayer(Item);
            }

            Layers.Clear();
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void CheckIsUsingInnerView()
        {
            if (IsUsingItemsSource)
                throw new InvalidOperationException("Operation is not valid while ItemsSource is in use. Access and modify elements with ItemsControl.ItemsSource instead.");
        }

        private ImageViewerLayer CreateImageViewerLayer(object Context)
        {
            if (Context is ImageViewerLayer Layer)
                return Layer;

            if (Context is IImageContext ImageContext)
            {
                ImageViewerLayer NewLayer = new ImageViewerLayer(true)
                {
                    DataContext = Context,
                    SourceContext = ImageContext
                };
                return NewLayer;
            }

            if (Context is ImageSource Image)
            {
                ImageViewerLayer NewLayer = new ImageViewerLayer(true)
                {
                    DataContext = Context,
                    Source = Image
                };
                return NewLayer;
            }

            return new ImageViewerLayer(true) { DataContext = Context };
        }
        private void PrepareImageViewerLayer(ImageViewerLayer Layer)
        {
            Layer.StatusChanged += OnLayerStatusChanged;
            //Layer.ChannelChanged += OnLayerChannelChanged;
        }
        private void ResetImageViewerLayer(ImageViewerLayer Layer)
        {
            Layer.StatusChanged -= OnLayerStatusChanged;
            //Layer.ChannelChanged -= OnLayerChannelChanged;

            if (Layer.IsGeneratedFromCollection)
            {
                Layer.ClearValue(FrameworkElement.DataContextProperty);
                Layer.ClearValue(ImageViewerLayer.SourceProperty);
                Layer.ClearValue(ImageViewerLayer.SourceContextProperty);

                //Layer.ClearValue(ImageViewerLayer.EnableLayerMarksProperty);
                //Layer.Marks.Reset();
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            Viewer._Attachments.ForEach(i => i.OnLayerCollectionChanged(e));
        }

        private void OnLayerStatusChanged(object sender, ImageViewerLayer e)
            => Viewer._Attachments.ForEach(i => i.InvalidateCanvas());

        private void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => OnPropertyChanged(new PropertyChangedEventArgs(PropertyName));

    }
}