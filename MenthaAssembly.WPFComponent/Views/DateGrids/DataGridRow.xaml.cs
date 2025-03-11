using System;
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
            get => (int)GetValue(HighlightingIndexProperty);
            set => SetValue(HighlightingIndexProperty, value);
        }

        public static readonly DependencyProperty HighlightingStyleProperty =
            DependencyProperty.Register(nameof(HighlightingStyle), typeof(Style), typeof(DataGridRow), new PropertyMetadata(null));
        public Style HighlightingStyle
        {
            get => (Style)GetValue(HighlightingStyleProperty);
            set => SetValue(HighlightingStyleProperty, value);
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
                if (Layer != null)
                {
                    Adorner[] Adorners = Layer.GetAdorners(this);
                    if (Adorners is null ||
                        Adorners.Length == 0)
                        Layer.Add(new DataGridRowAdorner(this));
                }
            }

            void OnUnloaded(object sender, RoutedEventArgs e)
            {
                if (Layer?.GetAdorners(this) is Adorner[] Adorners)
                    foreach (Adorner Adorner in Adorners)
                        Layer.Remove(Adorner);
            }
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
            private readonly Rectangle Rect;
            public DataGridRowAdorner(DataGridRow Row) : base(Row)
            {
                this.Row = Row;

                SetBinding(HighlightingIndexProperty, new Binding(nameof(HighlightingIndex)) { Source = Row });
                SetBinding(HighlightingCountProperty, new Binding(nameof(HighlightingCount)) { Source = Row });

                Rect = new Rectangle();
                Rect.SetBinding(StyleProperty, new Binding(nameof(HighlightingStyle))
                {
                    Source = Row,
                    TargetNullValue = DefaultHighlightingStyle
                });

                AddVisualChild(Rect);
            }

            protected override int VisualChildrenCount
                => 1;

            protected override Visual GetVisualChild(int index)
                => Rect;

            protected override Size ArrangeOverride(Size FinalSize)
            {
                int Count = HighlightingCount;
                if (Count is 0 or < (-1))
                {
                    Rect.Visibility = Visibility.Collapsed;
                    return FinalSize;
                }

                DataGridCellsPresenter CellsPresenter = Row.CellsPresenter;
                int StartIndex = HighlightingIndex,
                    MaxIndex = CellsPresenter.Items.Count - 1;
                if (MaxIndex < StartIndex)
                {
                    Rect.Visibility = Visibility.Collapsed;
                    return FinalSize;
                }

                Rect HighlightingRect;
                FrameworkElement StartCell = CellsPresenter.ItemContainerGenerator.ContainerFromIndex(StartIndex) as FrameworkElement;
                int EndIndex = Count == -1 ? MaxIndex : MathHelper.Clamp(StartIndex + Count - 1, 0, MaxIndex);
                if (StartIndex == EndIndex)
                {
                    HighlightingRect = StartCell.TransformToAncestor(Row).TransformBounds(new Rect(0d, 0d, StartCell.ActualWidth, StartCell.ActualHeight));
                }
                else
                {
                    FrameworkElement EndCell = CellsPresenter.ItemContainerGenerator.ContainerFromIndex(EndIndex) as FrameworkElement;
                    Point Start = StartCell.TransformToAncestor(Row).Transform(new Point(0, 0)),
                          End = EndCell.TransformToAncestor(Row).Transform(new Point(EndCell.ActualWidth, 0));

                    HighlightingRect = new Rect(Start, new Size(Math.Max(End.X - Start.X, 0), FinalSize.Height));
                }

                Rect.Width = HighlightingRect.Width;
                Rect.Height = HighlightingRect.Height;
                Rect.Visibility = Visibility.Visible;
                Rect.Arrange(HighlightingRect);
                return FinalSize;
            }
        }

    }
}