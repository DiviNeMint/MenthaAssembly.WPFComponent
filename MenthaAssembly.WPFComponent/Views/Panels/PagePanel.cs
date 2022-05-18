using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class PagePanel : VirtualizingPanel, IScrollInfo
    {
        public static readonly DependencyProperty ItemWidthProperty =
              DependencyProperty.Register("ItemWidth", typeof(double), typeof(PagePanel),
                  new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        [TypeConverter(typeof(LengthConverter))]
        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static readonly DependencyProperty ItemHeightProperty =
              DependencyProperty.Register("ItemHeight", typeof(double), typeof(PagePanel),
                  new FrameworkPropertyMetadata(100d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        [TypeConverter(typeof(LengthConverter))]
        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public static readonly DependencyProperty HorizontalSpacingProperty =
              DependencyProperty.Register("HorizontalSpacing", typeof(double), typeof(PagePanel),
                  new FrameworkPropertyMetadata(5d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        [TypeConverter(typeof(LengthConverter))]
        public double HorizontalSpacing
        {
            get => (double)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, value);
        }

        public static readonly DependencyProperty VerticalSpacingProperty =
              DependencyProperty.Register("VerticalSpacing", typeof(double), typeof(PagePanel),
                  new FrameworkPropertyMetadata(5d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        [TypeConverter(typeof(LengthConverter))]
        public double VerticalSpacing
        {
            get => (double)GetValue(VerticalSpacingProperty);
            set => SetValue(VerticalSpacingProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
              StackPanel.OrientationProperty.AddOwner(typeof(PagePanel),
                  new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty PageProperty =
              DependencyProperty.Register("Page", typeof(int), typeof(PagePanel),
                  new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
        public int Page
        {
            get => (int)GetValue(PageProperty);
            set => SetValue(PageProperty, value);
        }

        public static readonly DependencyProperty PageCountProperty =
              DependencyProperty.Register("PageCount", typeof(int), typeof(PagePanel), new PropertyMetadata(0));
        public int PageCount
        {
            get => (int)GetValue(PageCountProperty);
            protected set => SetValue(PageCountProperty, value);
        }

        protected ScrollViewer ScrollOwner { set; get; }
        ScrollViewer IScrollInfo.ScrollOwner
        {
            get => ScrollOwner;
            set => ScrollOwner = value;
        }

        bool IScrollInfo.CanVerticallyScroll { set; get; }
        bool IScrollInfo.CanHorizontallyScroll { set; get; }

        protected double ViewportWidth { set; get; }
        protected double ViewportHeight { set; get; }

        double IScrollInfo.ExtentWidth
            => 0d;
        double IScrollInfo.ExtentHeight
            => 0d;
        double IScrollInfo.HorizontalOffset
            => 0d;
        double IScrollInfo.VerticalOffset
            => 0d;
        double IScrollInfo.ViewportWidth
            => ViewportWidth;
        double IScrollInfo.ViewportHeight
            => ViewportHeight;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (!IsItemsHost &&
                Parent is ScrollViewer Viewer)
            {
                Viewer.CanContentScroll = true;
                Viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                Viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            }
        }

        protected Dictionary<int, Rect> Locations = new Dictionary<int, Rect>();
        protected int SelectedIndex { set; get; } = -1;
        protected int PageItemColumn { set; get; }
        protected int PageItemRow { set; get; }
        protected override Size MeasureOverride(Size AvailableSize)
        {
            if (double.IsInfinity(AvailableSize.Width))
                AvailableSize.Width = ActualWidth;

            if (double.IsInfinity(AvailableSize.Height))
                AvailableSize.Height = ActualHeight;

            Locations.Clear();

            bool UpdateScroll = ViewportWidth != AvailableSize.Width || ViewportHeight != AvailableSize.Height;
            if (UpdateScroll)
            {
                ViewportWidth = AvailableSize.Width;
                ViewportHeight = AvailableSize.Height;
            }

            if (IsItemsHost)
            {
                if (ItemsControl.GetItemsOwner(this) is ItemsControl ItemsOwner &&
                    ItemsOwner.ItemContainerGenerator is IItemContainerGenerator Generator)
                {
                    int Count = ItemsOwner.Items.Count;
                    double IW = ItemWidth,
                           IH = ItemHeight,
                           SpaceX = HorizontalSpacing,
                           SpaceY = VerticalSpacing;

                    if (double.IsNaN(IW) || double.IsInfinity(IW))
                        IW = 100d;

                    if (double.IsNaN(IH) || double.IsInfinity(IH))
                        IH = 100d;

                    if (double.IsNaN(SpaceX) || double.IsInfinity(SpaceX))
                        SpaceX = 0d;

                    if (double.IsNaN(SpaceY) || double.IsInfinity(SpaceY))
                        SpaceY = 0d;

                    Size ItemSize = new Size(IW, IH);

                    // Update Page Info
                    PageItemColumn = (int)Math.Floor((ViewportWidth + SpaceX) / (IW + SpaceX));
                    PageItemRow = (int)Math.Floor((ViewportHeight + SpaceY) / (IH + SpaceY));

                    int PageItemCount = PageItemColumn * PageItemRow,
                        StartIndex = Page * PageItemCount,
                        EndIndex = Math.Min(StartIndex + PageItemCount, Count),
                        LastChildrenCount = InternalChildren.Count;

                    PageCount = (int)Math.Ceiling(Count / (double)PageItemCount);

                    GeneratorPosition Position = Generator.GeneratorPositionFromIndex(StartIndex);
                    using (Generator.StartAt(Position, GeneratorDirection.Forward, true))
                    {
                        double Tx = 0d, Ty = 0d;
                        switch (Orientation)
                        {
                            case Orientation.Vertical:
                                {
                                    for (int i = StartIndex; i < EndIndex; i++)
                                    {
                                        if (!(Generator.GenerateNext(out bool IsNewlyRealized) is UIElement Item))
                                            break;

                                        if (IsNewlyRealized)
                                        {
                                            base.AddInternalChild(Item);
                                            Generator.PrepareItemContainer(Item);
                                        }

                                        Item.Measure(ItemSize);

                                        double Ny = Ty + IH;
                                        if (Ny > ViewportWidth)
                                        {
                                            Tx += IW + SpaceX;
                                            Ty = 0d;
                                            Ny = IH;
                                        }

                                        Locations[i] = new Rect(Tx, Ty, IW, IH);
                                        Ty = Ny + SpaceY;
                                    }
                                    break;
                                }
                            case Orientation.Horizontal:
                            default:
                                {
                                    for (int i = StartIndex; i < EndIndex; i++)
                                    {
                                        if (!(Generator.GenerateNext(out bool IsNewlyRealized) is UIElement Item))
                                            break;

                                        if (IsNewlyRealized)
                                        {
                                            base.AddInternalChild(Item);
                                            Generator.PrepareItemContainer(Item);
                                        }

                                        Item.Measure(ItemSize);

                                        double Nx = Tx + IW;
                                        if (Nx > ViewportWidth)
                                        {
                                            Tx = 0d;
                                            Ty += IH + SpaceY;
                                            Nx = IW;
                                        }

                                        Locations[i] = new Rect(Tx, Ty, IW, IH);
                                        Tx = Nx + SpaceX;
                                    }
                                    break;
                                }
                        }

                    }

                    for (int i = LastChildrenCount - 1; i >= 0; i--)
                    {
                        if (InternalChildren[i] is DependencyObject Child)
                        {
                            int itemIndex = ItemsOwner.ItemContainerGenerator.IndexFromContainer(Child);
                            if (itemIndex < StartIndex || EndIndex <= itemIndex)
                            {
                                Generator.Remove(Generator.GeneratorPositionFromIndex(itemIndex), 1);
                                RemoveInternalChildRange(i, 1);
                            }
                        }
                    }
                }
            }
            else
            {
                int Count = VisualChildrenCount;
                if (Count > 0)
                {
                    double IW = ItemWidth,
                           IH = ItemHeight,
                           SpaceX = HorizontalSpacing,
                           SpaceY = VerticalSpacing;

                    if (double.IsNaN(IW) || double.IsInfinity(IW))
                        IW = 100d;

                    if (double.IsNaN(IH) || double.IsInfinity(IH))
                        IH = 100d;

                    if (double.IsNaN(SpaceX) || double.IsInfinity(SpaceX))
                        SpaceX = 0d;

                    if (double.IsNaN(SpaceY) || double.IsInfinity(SpaceY))
                        SpaceY = 0d;

                    Size ItemSize = new Size(IW, IH);

                    // Update Page Info
                    PageItemColumn = (int)Math.Floor((ViewportWidth + SpaceX) / (IW + SpaceX));
                    PageItemRow = (int)Math.Floor((ViewportHeight + SpaceY) / (IH + SpaceY));

                    if (PageItemColumn == 0 || PageItemRow == 0)
                    {
                        PageCount = 0;
                        return AvailableSize;
                    }

                    int PageItemCount = PageItemColumn * PageItemRow,
                        StartIndex = Page * PageItemCount,
                        EndIndex = Math.Min(StartIndex + PageItemCount, Count),
                        LastChildrenCount = InternalChildren.Count;

                    PageCount = (int)Math.Ceiling(Count / (double)PageItemCount);

                    double Tx = 0d, Ty = 0d;
                    switch (Orientation)
                    {
                        case Orientation.Vertical:
                            {
                                for (int i = StartIndex; i < EndIndex; i++)
                                {
                                    if (GetVisualChild(i) is UIElement Item)
                                    {
                                        Item.Measure(ItemSize);

                                        double Ny = Ty + IH;
                                        if (Ny > ViewportHeight)
                                        {
                                            Tx += IW + SpaceX;
                                            Ty = 0d;
                                            Ny = IH;
                                        }

                                        Locations[i] = new Rect(Tx, Ty, IW, IH);
                                        Ty = Ny + SpaceY;
                                    }
                                }
                                break;
                            }
                        case Orientation.Horizontal:
                        default:
                            {
                                for (int i = StartIndex; i < EndIndex; i++)
                                {
                                    if (GetVisualChild(i) is UIElement Item)
                                    {
                                        Item.Measure(ItemSize);

                                        double Nx = Tx + IW;
                                        if (Nx > ViewportWidth)
                                        {
                                            Tx = 0d;
                                            Ty += IH + SpaceY;
                                            Nx = IW;
                                        }

                                        Locations[i] = new Rect(Tx, Ty, IW, IH);

                                        Tx = Nx + SpaceX;
                                    }
                                }
                                break;
                            }
                    }
                }
            }

            if (UpdateScroll)
                ScrollOwner?.InvalidateScrollInfo();

            return AvailableSize;
        }
        protected override Size ArrangeOverride(Size FinalSize)
        {
            if (IsItemsHost)
            {
                if (ItemsControl.GetItemsOwner(this) is ItemsControl ItemsOwner)
                {
                    foreach (UIElement Child in InternalChildren.OfType<UIElement>())
                    {
                        if (Child is ListBoxItem ListViewItem)
                        {
                            ListViewItem.Selected -= ListBoxItemSelected;
                            ListViewItem.Selected += ListBoxItemSelected;
                        }

                        int Index = ItemsOwner.ItemContainerGenerator.IndexFromContainer(Child);
                        Child.Arrange(Locations[Index]);
                    }
                }
            }
            else
            {
                int Count = VisualChildrenCount,
                    PageItemCount = PageItemColumn * PageItemRow,
                    StartIndex = Page * PageItemCount,
                    EndIndex = Math.Min(StartIndex + PageItemCount, VisualChildrenCount),
                    LastChildrenCount = InternalChildren.Count;

                Rect Empty = new Rect();
                for (int i = 0; i < Count; i++)
                    if (GetVisualChild(i) is UIElement Item)
                        Item.Arrange(Locations.TryGetValue(i, out Rect Location) ? Location : Empty);
            }

            return FinalSize;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (PageCount == 0 &&
                sizeInfo.NewSize.Width > 0 &&
                sizeInfo.NewSize.Height > 0)
            {
                MeasureOverride(sizeInfo.NewSize);
                UpdateLayout();
            }
        }

        protected override void BringIndexIntoView(int Index)
            => Page = (int)Math.Floor((double)Index / (PageItemColumn * PageItemRow));

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (SelectedIndex > -1 &&
                ItemsControl.GetItemsOwner(this) is ItemsControl ItemsOwner)
            {
                int ItemsMaxIndex = ItemsOwner.Items.Count - 1;
                switch (e.Key)
                {
                    case Key.Down:
                        {
                            if (SelectedIndex < ItemsMaxIndex)
                            {
                                SelectedIndex = Math.Min(SelectedIndex + PageItemColumn, ItemsMaxIndex);
                                if (SelectedIndex >= (Page + 1) * PageItemColumn * PageItemRow)
                                {
                                    Page++;
                                    UpdateLayout();
                                }

                                if (ItemsOwner.ItemContainerGenerator.ContainerFromIndex(SelectedIndex) is UIElement Item)
                                    Item.Focus();
                            }

                            e.Handled = true;
                            return;
                        }
                    case Key.Left:
                        {
                            if (SelectedIndex > 0)
                            {
                                SelectedIndex--;

                                if (SelectedIndex < Page * PageItemColumn * PageItemRow)
                                {
                                    Page--;
                                    UpdateLayout();
                                }

                                if (ItemsOwner.ItemContainerGenerator.ContainerFromIndex(SelectedIndex) is UIElement Item)
                                    Item.Focus();
                            }

                            e.Handled = true;
                            return;
                        }
                    case Key.Right:
                        {
                            if (SelectedIndex < ItemsMaxIndex)
                            {
                                SelectedIndex++;

                                if (SelectedIndex >= (Page + 1) * PageItemColumn * PageItemRow)
                                {
                                    Page++;
                                    UpdateLayout();
                                }

                                if (ItemsOwner.ItemContainerGenerator.ContainerFromIndex(SelectedIndex) is UIElement Item)
                                    Item.Focus();
                            }

                            e.Handled = true;
                            return;
                        }
                    case Key.Up:
                        {
                            if (SelectedIndex > 0)
                            {
                                SelectedIndex = Math.Max(SelectedIndex - PageItemColumn, 0);

                                if (SelectedIndex < Page * PageItemColumn * PageItemRow)
                                {
                                    Page--;
                                    UpdateLayout();
                                }

                                if (ItemsOwner.ItemContainerGenerator.ContainerFromIndex(SelectedIndex) is UIElement Item)
                                    Item.Focus();
                            }

                            e.Handled = true;
                            return;
                        }
                }
            }

            base.OnKeyDown(e);
        }

        private void ListBoxItemSelected(object sender, RoutedEventArgs e)
        {
            if (sender is ListBoxItem ThisItem &&
                !(ThisItem.Content is CollectionViewGroup) &&   // item is a group header don't click
                ItemsControl.GetItemsOwner(this) is ItemsControl ItemsOwner)
            {
                int Index = ItemsOwner.ItemContainerGenerator.IndexFromContainer(ThisItem);
                if (Index > -1)
                {
                    BringIndexIntoView(Index);
                    SelectedIndex = Index;
                }
            }
        }

        void IScrollInfo.LineUp()
        {
        }
        void IScrollInfo.LineDown()
        {
        }
        void IScrollInfo.LineLeft()
        {
        }
        void IScrollInfo.LineRight()
        {
        }

        void IScrollInfo.PageUp()
            => Page = Math.Max(Page - 1, 0);
        void IScrollInfo.PageDown()
            => Page = Math.Min(Page + 1, PageCount - 1);
        void IScrollInfo.PageLeft()
            => Page = Math.Max(Page - 1, 0);
        void IScrollInfo.PageRight()
            => Page = Math.Min(Page + 1, PageCount);

        void IScrollInfo.MouseWheelUp()
            => Page = Math.Max(Page - 1, 0);
        void IScrollInfo.MouseWheelDown()
            => Page = Math.Min(Page + 1, PageCount - 1);
        void IScrollInfo.MouseWheelLeft()
            => Page = Math.Max(Page - 1, 0);
        void IScrollInfo.MouseWheelRight()
            => Page = Math.Min(Page + 1, PageCount - 1);

        void IScrollInfo.SetHorizontalOffset(double offset)
        {
        }
        void IScrollInfo.SetVerticalOffset(double offset)
        {
        }

        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
            => rectangle;

    }
}
