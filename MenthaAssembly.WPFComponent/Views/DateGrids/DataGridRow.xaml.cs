using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public class DataGridRow : System.Windows.Controls.DataGridRow
    {
        internal static ComponentResourceKey DefaultHighlightingStyleKey { get; } = new ComponentResourceKey(typeof(DataGridRow), nameof(DefaultHighlightingStyle));
        public static Style DefaultHighlightingStyle
            => Application.Current.TryFindResource(DefaultHighlightingStyleKey) as Style;

        internal static readonly DependencyPropertyKey IsNewItemPlaceholderPropertyKey =
              DependencyProperty.RegisterReadOnly(nameof(IsNewItemPlaceholder), typeof(bool), typeof(DataGridRow), new PropertyMetadata(false));
        public static readonly DependencyProperty IsNewItemPlaceholderProperty = IsNewItemPlaceholderPropertyKey.DependencyProperty;
        public bool IsNewItemPlaceholder
        {
            get => (bool)GetValue(IsNewItemPlaceholderProperty);
            private set => SetValue(IsNewItemPlaceholderPropertyKey, value);
        }

        public static readonly DependencyProperty IsHighlightingProperty =
            DependencyProperty.Register(nameof(IsHighlighting), typeof(bool), typeof(DataGridRow), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is DataGridRow This)
                        This.OnIsHighlightingChanged(e.ToChangedEventArgs<bool>());
                }));
        public bool IsHighlighting
        {
            get => (bool)GetValue(IsHighlightingProperty);
            set => SetValue(IsHighlightingProperty, value);
        }

        public static readonly DependencyProperty HighlightingIndexProperty =
            DataGridRowAdorner.HighlightingIndexProperty.AddOwner(typeof(DataGridRow));
        public int HighlightingIndex
        {
            get => (int)GetValue(HighlightingIndexProperty);
            set => SetValue(HighlightingIndexProperty, value);
        }

        public static readonly DependencyProperty HighlightingCountProperty =
            DataGridRowAdorner.HighlightingCountProperty.AddOwner(typeof(DataGridRow));
        public int HighlightingCount
        {
            get => (int)GetValue(HighlightingCountProperty);
            set => SetValue(HighlightingCountProperty, value);
        }

        public static readonly DependencyProperty HighlightingStyleProperty =
            DependencyProperty.Register(nameof(HighlightingStyle), typeof(Style), typeof(DataGridRow), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is DataGridRow This)
                        This.InvalidateHighlighting();
                }));
        public Style HighlightingStyle
        {
            get => (Style)GetValue(HighlightingStyleProperty);
            set => SetValue(HighlightingStyleProperty, value);
        }

        public static readonly DependencyProperty HighlightingRangesProperty =
            DependencyProperty.Register(nameof(HighlightingRanges), typeof(ObservableCollection<DataGridRowHighlightingRange>), typeof(DataGridRow), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is DataGridRow This)
                        This.OnHighlightingRangesChanged(e.ToChangedEventArgs<ObservableCollection<DataGridRowHighlightingRange>>());
                }));
        public ObservableCollection<DataGridRowHighlightingRange> HighlightingRanges
        {
            get => (ObservableCollection<DataGridRowHighlightingRange>)GetValue(HighlightingRangesProperty);
            set => SetValue(HighlightingRangesProperty, value);
        }

        private static readonly PropertyInfo GetCellsPresenter;
        internal DataGridCellsPresenter CellsPresenter
            => GetCellsPresenter?.GetValue(this) as DataGridCellsPresenter;

        static DataGridRow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(typeof(DataGridRow)));
            ReflectionHelper.TryGetInternalProperty(typeof(System.Windows.Controls.DataGridRow), nameof(CellsPresenter), out GetCellsPresenter);
        }
        public DataGridRow() : base()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private AdornerLayer Layer;
        private void OnIsHighlightingChanged(ChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                Loaded += OnLoaded;
                Unloaded += OnUnloaded;
                OnLoaded(this, null);
            }
            else
            {
                Loaded -= OnLoaded;
                Unloaded -= OnUnloaded;
                OnUnloaded(this, null);
            }

            void OnLoaded(object sender, RoutedEventArgs e)
            {
                Layer ??= AdornerLayer.GetAdornerLayer(this);
                if (Layer is null)
                    return;

                Adorner[] Adorners = Layer.GetAdorners(this);
                if (Adorners != null)
                    foreach (Adorner Adorner in Adorners)
                        if (Adorner is DataGridRowAdorner)
                            return;

                Layer.Add(new DataGridRowAdorner(this));
            }

            void OnUnloaded(object sender, RoutedEventArgs e)
            {
                if (Layer?.GetAdorners(this) is Adorner[] Adorners)
                    foreach (Adorner Adorner in Adorners)
                        if (Adorner is DataGridRowAdorner)
                            Layer.Remove(Adorner);
            }
        }

        private static readonly DependencyPropertyDescriptor HighlightingRangeIndexDescriptor =
            DependencyPropertyDescriptor.FromProperty(DataGridRowHighlightingRange.IndexProperty, typeof(DataGridRowHighlightingRange));
        private static readonly DependencyPropertyDescriptor HighlightingRangeCountDescriptor =
            DependencyPropertyDescriptor.FromProperty(DataGridRowHighlightingRange.CountProperty, typeof(DataGridRowHighlightingRange));
        private static readonly DependencyPropertyDescriptor HighlightingRangeStyleDescriptor =
            DependencyPropertyDescriptor.FromProperty(DataGridRowHighlightingRange.StyleProperty, typeof(DataGridRowHighlightingRange));
        private readonly List<DataGridRowHighlightingRange> HookedHighlightingRanges = [];
        private void OnHighlightingRangesChanged(ChangedEventArgs<ObservableCollection<DataGridRowHighlightingRange>> e)
        {
            if (e.OldValue != null)
            {
                e.OldValue.CollectionChanged -= OnHighlightingRangesCollectionChanged;
                UnhookAllHighlightingRanges();
            }

            if (e.NewValue != null)
            {
                e.NewValue.CollectionChanged += OnHighlightingRangesCollectionChanged;
                foreach (DataGridRowHighlightingRange Range in e.NewValue)
                    HookHighlightingRange(Range);
            }

            InvalidateHighlighting();
        }

        private void OnHighlightingRangesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UnhookAllHighlightingRanges();
                if (HighlightingRanges != null)
                    foreach (DataGridRowHighlightingRange Range in HighlightingRanges)
                        HookHighlightingRange(Range);
            }
            else if (e.OldItems != null)
            {
                foreach (DataGridRowHighlightingRange Range in e.OldItems)
                    UnhookHighlightingRange(Range);
            }

            if (e.NewItems != null)
                foreach (DataGridRowHighlightingRange Range in e.NewItems)
                    HookHighlightingRange(Range);

            InvalidateHighlighting();
        }

        private void HookHighlightingRange(DataGridRowHighlightingRange Range)
        {
            if (Range is null ||
                HookedHighlightingRanges.Contains(Range))
                return;

            HookedHighlightingRanges.Add(Range);
            HighlightingRangeIndexDescriptor.AddValueChanged(Range, OnHighlightingRangeChanged);
            HighlightingRangeCountDescriptor.AddValueChanged(Range, OnHighlightingRangeChanged);
            HighlightingRangeStyleDescriptor.AddValueChanged(Range, OnHighlightingRangeChanged);
        }

        private void UnhookHighlightingRange(DataGridRowHighlightingRange Range)
        {
            if (Range is null ||
                !HookedHighlightingRanges.Remove(Range))
                return;

            HighlightingRangeIndexDescriptor.RemoveValueChanged(Range, OnHighlightingRangeChanged);
            HighlightingRangeCountDescriptor.RemoveValueChanged(Range, OnHighlightingRangeChanged);
            HighlightingRangeStyleDescriptor.RemoveValueChanged(Range, OnHighlightingRangeChanged);
        }

        private void UnhookAllHighlightingRanges()
        {
            foreach (DataGridRowHighlightingRange Range in HookedHighlightingRanges.ToArray())
                UnhookHighlightingRange(Range);
        }

        private void OnHighlightingRangeChanged(object sender, EventArgs e)
            => InvalidateHighlighting();

        internal void InvalidateHighlighting()
        {
            if (Layer?.GetAdorners(this) is Adorner[] Adorners)
                foreach (Adorner Adorner in Adorners)
                    Adorner.InvalidateArrange();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => IsNewItemPlaceholder = DataGrid.IsNewItemPlaceholder(e.NewValue);

        private class DataGridRowAdorner : Adorner
        {
            public static readonly DependencyProperty HighlightingIndexProperty =
                DependencyProperty.Register(nameof(HighlightingIndex), typeof(int), typeof(DataGridRowAdorner),
                    new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsArrange));
            public int HighlightingIndex
            {
                get => (int)GetValue(HighlightingIndexProperty);
                set => SetValue(HighlightingIndexProperty, value);
            }

            public static readonly DependencyProperty HighlightingCountProperty =
                DependencyProperty.Register(nameof(HighlightingCount), typeof(int), typeof(DataGridRowAdorner),
                    new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsArrange));
            public int HighlightingCount
            {
                get => (int)GetValue(HighlightingCountProperty);
                set => SetValue(HighlightingCountProperty, value);
            }

            private readonly DataGridRow Row;
            private readonly List<Rectangle> Rects = [];
            public DataGridRowAdorner(DataGridRow Row) : base(Row)
            {
                this.Row = Row;

                SetBinding(HighlightingIndexProperty, new Binding(nameof(HighlightingIndex)) { Source = Row });
                SetBinding(HighlightingCountProperty, new Binding(nameof(HighlightingCount)) { Source = Row });

                EnsureRectCount(1);
            }

            protected override int VisualChildrenCount
                => Rects.Count;

            protected override Visual GetVisualChild(int index)
                => Rects[index];

            protected override Size ArrangeOverride(Size FinalSize)
            {
                ObservableCollection<DataGridRowHighlightingRange> Ranges = Row.HighlightingRanges;
                if (Ranges?.Count > 0)
                {
                    EnsureRectCount(Ranges.Count);
                    for (int i = 0; i < Ranges.Count; i++)
                    {
                        DataGridRowHighlightingRange Range = Ranges[i];
                        if (Range is null)
                            ArrangeRect(Rects[i], -1, 0, null, FinalSize);
                        else
                            ArrangeRect(Rects[i], Range.Index, Range.Count, Range.Style, FinalSize);
                    }

                    return FinalSize;
                }

                EnsureRectCount(1);
                ArrangeRect(Rects[0], HighlightingIndex, HighlightingCount, null, FinalSize);
                return FinalSize;
            }

            private void EnsureRectCount(int Count)
            {
                while (Rects.Count < Count)
                {
                    Rectangle Rect = new();
                    Rects.Add(Rect);
                    AddVisualChild(Rect);
                }

                while (Count < Rects.Count)
                {
                    Rectangle Rect = Rects[Rects.Count - 1];
                    Rects.RemoveAt(Rects.Count - 1);
                    RemoveVisualChild(Rect);
                }
            }

            private void ArrangeRect(Rectangle Rect, int StartIndex, int Count, Style Style, Size FinalSize)
            {
                Rect.Style = Style ?? Row.HighlightingStyle ?? DefaultHighlightingStyle;
                if (Count is 0 or < (-1))
                {
                    Rect.Visibility = Visibility.Collapsed;
                    return;
                }

                DataGridCellsPresenter CellsPresenter = Row.CellsPresenter;
                if (CellsPresenter is null)
                {
                    Rect.Visibility = Visibility.Collapsed;
                    return;
                }

                int MaxIndex = CellsPresenter.Items.Count - 1;
                if (StartIndex < 0 ||
                    MaxIndex < StartIndex)
                {
                    Rect.Visibility = Visibility.Collapsed;
                    return;
                }

                FrameworkElement StartCell = CellsPresenter.ItemContainerGenerator.ContainerFromIndex(StartIndex) as FrameworkElement;
                if (StartCell is null)
                {
                    Rect.Visibility = Visibility.Collapsed;
                    return;
                }

                Rect HighlightingRect;
                int EndIndex = Count == -1 ? MaxIndex : MathHelper.Clamp(StartIndex + Count - 1, 0, MaxIndex);
                if (StartIndex == EndIndex)
                {
                    HighlightingRect = StartCell.TransformToAncestor(Row).TransformBounds(new Rect(0d, 0d, StartCell.ActualWidth, StartCell.ActualHeight));
                }
                else
                {
                    FrameworkElement EndCell = CellsPresenter.ItemContainerGenerator.ContainerFromIndex(EndIndex) as FrameworkElement;
                    if (EndCell is null)
                    {
                        Rect.Visibility = Visibility.Collapsed;
                        return;
                    }

                    Point Start = StartCell.TransformToAncestor(Row).Transform(new Point(0, 0)),
                          End = EndCell.TransformToAncestor(Row).Transform(new Point(EndCell.ActualWidth, 0));

                    HighlightingRect = new Rect(Start, new Size(Math.Max(End.X - Start.X, 0), FinalSize.Height));
                }

                Rect.Width = HighlightingRect.Width;
                Rect.Height = HighlightingRect.Height;
                Rect.Visibility = Visibility.Visible;
                Rect.Arrange(HighlightingRect);
            }
        }

    }

    public class DataGridRowHighlightingRange : DependencyObject
    {
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register(nameof(Index), typeof(int), typeof(DataGridRowHighlightingRange), new PropertyMetadata(0));
        public int Index
        {
            get => (int)GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register(nameof(Count), typeof(int), typeof(DataGridRowHighlightingRange), new PropertyMetadata(1));
        public int Count
        {
            get => (int)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        public static readonly DependencyProperty StyleProperty =
            DependencyProperty.Register(nameof(Style), typeof(Style), typeof(DataGridRowHighlightingRange), new PropertyMetadata(null));
        public Style Style
        {
            get => (Style)GetValue(StyleProperty);
            set => SetValue(StyleProperty, value);
        }

    }

}
