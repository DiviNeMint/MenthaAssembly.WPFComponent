using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MenthaAssembly.Views.Primitives
{
    public sealed class ImageViewerLayerMarkCollection : ObservableCollection<ImageViewerLayerMark>
    {
        public event EventHandler MarkChanged;

        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        protected override void InsertItem(int Index, ImageViewerLayerMark Mark)
        {
            base.InsertItem(Index, Mark);
            Mark.PropertyChanged += OnMarkPropertyChanged;
            Mark.CenterLocations.CollectionChanged += OnMarkLocationsCollectionChanged;
        }

        protected override void SetItem(int Index, ImageViewerLayerMark Mark)
        {
            CheckReentrancy();

            ImageViewerLayerMark OldMark = Items[Index];
            OldMark.PropertyChanged -= OnMarkPropertyChanged;
            OldMark.CenterLocations.CollectionChanged -= OnMarkLocationsCollectionChanged;

            Mark.PropertyChanged += OnMarkPropertyChanged;
            Mark.CenterLocations.CollectionChanged += OnMarkLocationsCollectionChanged;
            Items[Index] = Mark;

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, OldMark, Mark, Index));
        }

        protected override void RemoveItem(int Index)
        {
            CheckReentrancy();

            ImageViewerLayerMark Mark = Items[Index];
            Mark.PropertyChanged -= OnMarkPropertyChanged;
            Mark.CenterLocations.CollectionChanged -= OnMarkLocationsCollectionChanged;

            Items.RemoveAt(Index);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Mark, Index));
        }

        protected override void ClearItems()
        {
            CheckReentrancy();
            Reset();
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal void Reset()
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                ImageViewerLayerMark Mark = Items[i];
                Mark.PropertyChanged -= OnMarkPropertyChanged;
                Mark.CenterLocations.CollectionChanged -= OnMarkLocationsCollectionChanged;

                Items.RemoveAt(i);
            }
        }

        private void OnMarkLocationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => MarkChanged?.Invoke(sender, EventArgs.Empty);
        private void OnMarkPropertyChanged(object sender, PropertyChangedEventArgs e)
            => MarkChanged?.Invoke(sender, EventArgs.Empty);

        private void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => OnPropertyChanged(new PropertyChangedEventArgs(PropertyName));

    }
}
