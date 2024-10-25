using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class VirtualizingSpacingStackPanel : VirtualizingPanel, IScrollInfo
    {
        public static readonly DependencyProperty VerticalSpacingProperty =
            DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(VirtualizingSpacingStackPanel),
                new FrameworkPropertyMetadata(5d, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double VerticalSpacing
        {
            get => (double)GetValue(VerticalSpacingProperty);
            set => SetValue(VerticalSpacingProperty, value);
        }

        public static readonly DependencyProperty HorizontalSpacingProperty =
            DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(VirtualizingSpacingStackPanel),
                new FrameworkPropertyMetadata(5d, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double HorizontalSpacing
        {
            get => (double)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            StackPanel.OrientationProperty.AddOwner(typeof(VirtualizingSpacingStackPanel));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        protected Size Extent { set; get; } = new(0, 0);

        protected Size Viewport { set; get; } = new(0, 0);

        protected double OffsetX { set; get; } = 0d;

        protected double OffsetY { set; get; } = 0d;

        public ScrollViewer ScrollOwner { get; set; }

        public bool CanHorizontallyScroll { get; set; }

        public bool CanVerticallyScroll { get; set; }

        protected readonly Dictionary<int, Rect> Locations = [];
        protected override Size MeasureOverride(Size AvailableSize)
        {
            int Count = Items.Count;
            if (Count == 0)
                return new();

            GetValidMeasureBound(AvailableSize, out double Rw, out double Rh);
            if (Orientation == Orientation.Horizontal)
            {
                Extent = MeasureHorizontalStacking(AvailableSize, Rw, Rh, Count);
                Viewport = new(Rw, Extent.Height);
            }
            else
            {
                Extent = MeasureVerticalStacking(AvailableSize, Rw, Rh, Count);
                Viewport = new(Extent.Width, Rh);
            }

            ScrollOwner?.InvalidateScrollInfo();
            return Extent;
        }
        private Size MeasureHorizontalStacking(Size AvailableSize, double Rw, double Rh, int Count)
        {
            double Ex = 0d, Ey = 0d;
            Size LayoutSlotSize = AvailableSize;

            LayoutSlotSize.Width = double.PositiveInfinity;
            if (CanVerticallyScroll)
                LayoutSlotSize.Height = double.PositiveInfinity;

            double Spacing = VerticalSpacing,
                   Ox = OffsetX + GlobalOffsetX,
                   Oy = OffsetY + GlobalOffsetY;
            IRecyclingItemContainerGenerator Generator = ItemContainerGenerator;
            if (IsTemplateGenerated)
            {
                // Calculate First Item Size
                FrameworkElement Child = GetContainer(Generator, 0, out bool IsNewlyRealized);
                Child.Measure(LayoutSlotSize);

                Size ChildSize = Child.DesiredSize;
                double Ch = ChildSize.Height,
                       Cw = ChildSize.Width;
                if (Cw == 0d && Ch == 0d)
                {
                    if (IsNewlyRealized)
                    {
                        // Realize
                        AddInternalChild(Child);
                        RealizedContainers.Add(0, Child);

                        // Remeasure
                        Child.Measure(LayoutSlotSize);
                        ChildSize = Child.DesiredSize;
                        Ch = ChildSize.Height;
                        Cw = ChildSize.Width;
                        IsNewlyRealized = false;
                    }
                }

                // Zero size
                if (Cw == 0d || Ch == 0d)
                {
                    for (int i = 0; i < Count; i++)
                        Virtualize(Generator, i);

                    return new();
                }

                // First Item Loaction
                if (!Locations.ContainsKey(0))
                    Locations[0] = new Rect(0d, 0d, Cw, Ch);

                Ex = Cw;
                Ey = Ch;

                // Realize & Virtualize
                if (Ex - Ox <= 0d || 0d > Rw + Ox ||
                    Ey - Oy <= 0d || 0d > Rh + Oy)
                {
                    if (RealizedContainers.ContainsKey(0))
                        Virtualize(Generator, 0);
                }
                else if (IsNewlyRealized)
                {
                    AddInternalChild(Child);
                    RealizedContainers.Add(0, Child);
                }

                for (int i = 1; i < Count; i++)
                {
                    // Spacing
                    Ex += Spacing;

                    // Location
                    double Sx = Ex;
                    if (!Locations.ContainsKey(i))
                        Locations[i] = new Rect(Sx, 0d, Cw, Ch);

                    // Item Size
                    Ex += Cw;

                    // Out of viewport
                    if (Ex - Ox <= 0d || Sx > Rw + Ox ||
                        Ey - Oy <= 0d || 0d > Rh + Oy)
                    {
                        if (RealizedContainers.ContainsKey(i))
                            Virtualize(Generator, i);
                    }
                    else
                    {
                        Realize(Generator, i);
                    }
                }
            }
            else
            {
                bool IsNotFirst = false;
                for (int i = 0; i < Count; i++)
                {
                    FrameworkElement Child = Realize(Generator, i);
                    Child.Measure(LayoutSlotSize);

                    Size ChildSize = Child.DesiredSize;
                    double Cw = ChildSize.Width,
                           Ch = ChildSize.Height;

                    // Spacing
                    if (IsNotFirst)
                        Ex += Spacing;
                    else
                        IsNotFirst = true;

                    // Location
                    double Sx = Ex;
                    if (!Locations.ContainsKey(i))
                        Locations[i] = new Rect(Sx, 0d, Cw, Ch);

                    // Item Size
                    Ex += Cw;
                    Ey = Math.Max(Ey, Ch);

                    // Out of viewport
                    if (Ex - Ox <= 0d || Sx > Rw + Ox ||
                        Ey - Oy <= 0d || 0d > Rh + Oy)
                        Virtualize(Generator, i);
                }
            }

            return new(Math.Max(Ex, 0d), Ey);
        }
        private Size MeasureVerticalStacking(Size AvailableSize, double Rw, double Rh, int Count)
        {
            double Ex = 0d, Ey = 0d;
            Size LayoutSlotSize = AvailableSize;

            LayoutSlotSize.Height = double.PositiveInfinity;
            if (CanHorizontallyScroll)
                LayoutSlotSize.Width = double.PositiveInfinity;

            double Spacing = VerticalSpacing,
                   Ox = OffsetX + GlobalOffsetX,
                   Oy = OffsetY + GlobalOffsetY;
            IRecyclingItemContainerGenerator Generator = ItemContainerGenerator;
            if (IsTemplateGenerated)
            {
                // Calculate First Item Size
                FrameworkElement Child = GetContainer(Generator, 0, out bool IsNewlyRealized);
                Child.Measure(LayoutSlotSize);

                Size ChildSize = Child.DesiredSize;
                double Ch = ChildSize.Height,
                       Cw = ChildSize.Width;
                if (Cw == 0d && Ch == 0d)
                {
                    if (IsNewlyRealized)
                    {
                        // Realize
                        AddInternalChild(Child);
                        RealizedContainers.Add(0, Child);

                        // Remeasure
                        Child.Measure(LayoutSlotSize);
                        ChildSize = Child.DesiredSize;
                        Ch = ChildSize.Height;
                        Cw = ChildSize.Width;
                        IsNewlyRealized = false;
                    }
                }

                // Zero size
                if (Cw == 0d || Ch == 0d)
                {
                    for (int i = 0; i < Count; i++)
                        Virtualize(Generator, i);

                    return new();
                }

                // Checks Child Width
                Cw = double.IsPositiveInfinity(Rw) ? Cw : Rw;

                // First Item Loaction
                if (!Locations.ContainsKey(0))
                    Locations[0] = new Rect(0d, 0d, Cw, Ch);

                Ex = Cw;
                Ey = Ch;

                // Realize & Virtualize
                if (Ex - Ox <= 0d || 0d > Rw + Ox ||
                    Ey - Oy <= 0d || 0d > Rh + Oy)
                {
                    if (RealizedContainers.ContainsKey(0))
                        Virtualize(Generator, 0);
                }
                else if (IsNewlyRealized)
                {
                    AddInternalChild(Child);
                    RealizedContainers.Add(0, Child);
                }

                for (int i = 1; i < Count; i++)
                {
                    // Spacing
                    Ey += Spacing;

                    // Location
                    double Sy = Ey;
                    if (!Locations.ContainsKey(i))
                        Locations[i] = new Rect(0d, Sy, Cw, Ch);

                    // Item Size
                    Ey += Ch;

                    // Out of viewport
                    if (Ex - Ox <= 0d || 0d > Rw + Ox ||
                        Ey - Oy <= 0d || Sy > Rh + Oy)
                    {
                        if (RealizedContainers.ContainsKey(i))
                            Virtualize(Generator, i);
                    }
                    else
                    {
                        Realize(Generator, i);
                    }
                }
            }
            else
            {
                bool IsNotFirst = false;
                for (int i = 0; i < Count; i++)
                {
                    FrameworkElement Child = Realize(Generator, i);
                    Child.Measure(LayoutSlotSize);

                    Size ChildSize = Child.DesiredSize;
                    double Cw = ChildSize.Width,
                           Ch = ChildSize.Height;

                    // Spacing
                    if (IsNotFirst)
                        Ey += Spacing;
                    else
                        IsNotFirst = true;

                    // Location
                    double Sy = Ey;
                    if (!Locations.ContainsKey(i))
                        Locations[i] = new Rect(0d, Sy, Cw, Ch);

                    // Item Size
                    Ex = Math.Max(Ex, Cw);
                    Ey += Ch;

                    // Out of viewport
                    if (Ex - Ox <= 0d || 0d > Rw + Ox ||
                        Ey - Oy <= 0d || Sy > Rh + Oy)
                        Virtualize(Generator, i);
                }
            }

            return new(Ex, Math.Max(Ey, 0d));
        }

        private ScrollViewer GlobalViewer;
        private double GlobalOffsetX = 0d, GlobalOffsetY = 0d;
        protected void GetValidMeasureBound(Size AvailableSize, out double Rw, out double Rh)
        {
            Rw = AvailableSize.Width;
            Rh = AvailableSize.Height;
            if (double.IsPositiveInfinity(Rw) || double.IsPositiveInfinity(Rh))
            {
                if (this.FindVisualParents<ScrollViewer>().FirstOrDefault() is ScrollViewer Viewer)
                {
                    Rw = Viewer.ViewportWidth;
                    Rh = Viewer.ViewportHeight;

                    if (GlobalViewer != Viewer)
                    {
                        if (GlobalViewer != null)
                            GlobalViewer.ScrollChanged -= OnGlobalViewerScrollChanged;

                        Viewer.ScrollChanged += OnGlobalViewerScrollChanged;
                        GlobalViewer = Viewer;
                    }
                }
                else if (GlobalViewer != null)
                {
                    GlobalOffsetX = GlobalOffsetY = 0d;
                    GlobalViewer.ScrollChanged -= OnGlobalViewerScrollChanged;
                    GlobalViewer = null;
                }
            }
        }
        private void OnGlobalViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (this.FindVisualParents<ScrollViewer>().FirstOrDefault(i => i == GlobalViewer) is null)
            {
                GlobalOffsetX = GlobalOffsetY = 0d;
                GlobalViewer.ScrollChanged -= OnGlobalViewerScrollChanged;
                GlobalViewer = null;
                return;
            }

            // Update Global Offset
            GeneralTransform ViewerTransform = GlobalViewer.TransformToDescendant(this);
            Point StartPoint = ViewerTransform.Transform(new Point(0, 0));
            GlobalOffsetX = StartPoint.X;
            GlobalOffsetY = StartPoint.Y;

            // Update Measure
            InvalidateMeasure();
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            foreach (KeyValuePair<int, FrameworkElement> Data in RealizedContainers)
                Data.Value.Arrange(Rect.Offset(Locations[Data.Key], -OffsetX, -OffsetY));

            return FinalSize;
        }

        protected override void BringIndexIntoView(int index)
        {
            if (!IsLoaded)
                return;

            if (Orientation == Orientation.Horizontal)
                BringHorizontalIndexIntoView(index);
            else
                BringVerticalIndexIntoView(index);
        }
        private void BringHorizontalIndexIntoView(int Index)
        {
            Size MeasureSize = new(double.PositiveInfinity, ActualHeight);
            if (!Locations.TryGetValue(Index, out Rect Location))
            {
                Measure(MeasureSize);
                if (!Locations.TryGetValue(Index, out Location))
                    return;
            }

            GetValidMeasureBound(MeasureSize, out double Rw, out double Rh);
            if (GlobalViewer != null)
            {
                double Oy = GlobalViewer.ContentHorizontalOffset + OffsetX;
                if (Location.Right > Oy + Rw)
                {
                    GlobalViewer.ScrollToHorizontalOffset(Location.X);
                    GlobalOffsetX = Location.X;
                }
            }
            else
            {
                OffsetY = Location.Right <= Rw ? 0 : Location.X;
            }

            MeasureOverride(MeasureSize);
            UpdateLayout();
        }
        private void BringVerticalIndexIntoView(int Index)
        {
            Size MeasureSize = new(ActualWidth, double.PositiveInfinity);
            if (!Locations.TryGetValue(Index, out Rect Location))
            {
                MeasureOverride(MeasureSize);
                if (!Locations.TryGetValue(Index, out Location))
                    return;
            }

            GetValidMeasureBound(MeasureSize, out double Rw, out double Rh);
            if (GlobalViewer != null)
            {
                double Oy = GlobalViewer.ContentVerticalOffset + OffsetY;
                if (Location.Bottom > Oy + Rh)
                {
                    GlobalViewer.ScrollToVerticalOffset(Location.Y);
                    GlobalOffsetY = Location.Y;
                }
            }
            else
            {
                OffsetY = Location.Bottom <= Rh ? 0 : Location.Y;
            }

            MeasureOverride(MeasureSize);
            UpdateLayout();
        }

        #region IScrollInfo
        double IScrollInfo.ExtentHeight
            => Extent.Height;
        double IScrollInfo.ExtentWidth
            => Extent.Width;
        double IScrollInfo.ViewportHeight
            => Viewport.Height;
        double IScrollInfo.ViewportWidth
            => Viewport.Width;
        double IScrollInfo.HorizontalOffset
            => OffsetX;
        double IScrollInfo.VerticalOffset
            => OffsetY;

        void IScrollInfo.LineDown()
            => SetVerticalOffset(OffsetY + 20);
        void IScrollInfo.LineLeft()
            => SetHorizontalOffset(OffsetX - 20);
        void IScrollInfo.LineRight()
            => SetHorizontalOffset(OffsetX + 20);
        void IScrollInfo.LineUp()
            => SetVerticalOffset(OffsetY - 20);
        void IScrollInfo.MouseWheelDown()
            => SetVerticalOffset(OffsetY + 30);
        void IScrollInfo.MouseWheelLeft()
            => SetHorizontalOffset(OffsetX - 30);
        void IScrollInfo.MouseWheelRight()
            => SetHorizontalOffset(OffsetX + 30);
        void IScrollInfo.MouseWheelUp()
            => SetVerticalOffset(OffsetY - 30);
        void IScrollInfo.PageDown()
            => SetVerticalOffset(OffsetY + Viewport.Height);
        void IScrollInfo.PageLeft()
            => SetHorizontalOffset(OffsetX - Viewport.Width);
        void IScrollInfo.PageRight()
            => SetHorizontalOffset(OffsetX + Viewport.Width);
        void IScrollInfo.PageUp()
            => SetVerticalOffset(OffsetY - Viewport.Height);

        public void SetHorizontalOffset(double Offset)
        {
            if (Offset < 0 || Viewport.Width >= Extent.Width)
                Offset = 0;
            else if (Offset + Viewport.Width >= Extent.Width)
                Offset = Extent.Width - Viewport.Width;

            OffsetX = Offset;

            ScrollOwner?.InvalidateScrollInfo();
            InvalidateMeasure();
        }
        public void SetVerticalOffset(double Offset)
        {
            if (Offset < 0 || Viewport.Height >= Extent.Height)
                Offset = 0;
            else if (Offset + Viewport.Height >= Extent.Height)
                Offset = Extent.Height - Viewport.Height;

            OffsetY = Offset;

            ScrollOwner?.InvalidateScrollInfo();
            InvalidateMeasure();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return new Rect();
        }

        #endregion

    }
}