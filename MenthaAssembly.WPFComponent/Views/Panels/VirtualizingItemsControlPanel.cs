using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public sealed class VirtualizingItemsControlPanel : VirtualizingPanel
    {
        private ScrollViewer ScrollOwner { set; get; }

        private ItemContainerGenerator _itemContainerGenerator;
        private new ItemContainerGenerator ItemContainerGenerator
        {
            get
            {
                if (_itemContainerGenerator is null)
                {
                    // The ItemContainerGenerator is null until InternalChildren is accessed at least one time.
                    _ = InternalChildren;
                    _itemContainerGenerator = base.ItemContainerGenerator.GetItemContainerGeneratorForPanel(this);

                }
                return _itemContainerGenerator;
            }
        }

        private ReadOnlyCollection<object> Items
            => ItemContainerGenerator.Items;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (this.FindVisualParents<ScrollViewer>().FirstOrDefault() is ScrollViewer Viewer)
            {
                ScrollOwner = Viewer;
                Viewer.ScrollChanged += OnViewerScrollChanged;
            }
        }

        private void OnViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0d)
                InvalidateMeasure();
        }

        private Size? ItemSize;
        private readonly Dictionary<int, Rect> RealizedLocations = [];
        protected override Size MeasureOverride(Size AvailableSize)
        {
            if (IsItemsHost)
            {
                int Count = Items.Count;
                if (Count == 0)
                    return new();

                IRecyclingItemContainerGenerator Generator = ItemContainerGenerator;
                if (ScrollOwner is ScrollViewer Viewer)
                {
                    if (!ItemSize.HasValue)
                    {
                        FrameworkElement Container = GetContainer(Generator, 0, out _);

                        Container.Measure(AvailableSize);
                        ItemSize = Container.DesiredSize;

                        Virtualize(Generator, 0);
                    }

                    double Iw = ItemSize.Value.Width,
                           Ih = ItemSize.Value.Height,
                           Vh = Viewer.ActualHeight;

                    GeneralTransform Transform = Viewer.TransformToDescendant(this);
                    double Vy0 = Transform.Transform(new Point(0, 0)).Y,
                           Vy1 = Transform.Transform(new Point(0, Vh)).Y;

                    int Start = Math.Max((int)Math.Floor(Vy0 / Ih), 0),
                        End = Math.Min((int)Math.Floor(Vy1 / Ih), Count - 1);

                    Vy0 = Start * Ih;
                    for (int i = Start; i <= End; i++, Vy0 += Ih)
                    {
                        FrameworkElement Container = Realize(Generator, i);
                        if (!RealizedLocations.ContainsKey(i))
                            RealizedLocations[i] = new Rect(0, Vy0, Iw, Ih);
                    }

                    foreach (int Key in RealizedContainers.Keys.Where(i => i < Start || End < i))
                        Virtualize(Generator, Key);

                    return new Size(Iw, Ih * Count);
                }
                else
                {
                    double Vy0 = 0d;
                    for (int i = 0; i < Count; i++)
                    {
                        FrameworkElement Container = Realize(Generator, i);
                        if (!RealizedLocations.ContainsKey(i))
                        {
                            Container.Measure(AvailableSize);

                            Size ItemSize = Container.DesiredSize;
                            RealizedLocations[i] = new Rect(0, Vy0, ItemSize.Width, ItemSize.Height);
                            Vy0 += ItemSize.Height;
                        }
                    }

                    return AvailableSize;
                }
            }
            else
            {
                int Count = InternalChildren.Count;
                if (Count == 0)
                    return new();

                double Pw = 0d,
                       Ph = 0d;
                for (int i = 0; i < Count; i++)
                {
                    UIElement Item = InternalChildren[i];
                    Item.Measure(AvailableSize);

                    Size Size = Item.DesiredSize;
                    Pw = Math.Max(Pw, Size.Width);
                    Ph += Size.Height;
                }

                return new Size(Pw, Ph);
            }
        }

        private readonly Dictionary<int, FrameworkElement> RealizedContainers = [];
        private FrameworkElement Realize(IRecyclingItemContainerGenerator Generator, int Index)
        {
            FrameworkElement Container = GetContainer(Generator, Index, out bool IsNewlyRealized);

            if (IsNewlyRealized)
            {
                AddInternalChild(Container);
                RealizedContainers.Add(Index, Container);
            }
            else
            {
                Container.InvalidateMeasure();
            }

            return Container;
        }
        private FrameworkElement GetContainer(IRecyclingItemContainerGenerator Generator, int Index, out bool IsNewlyRealized)
        {
            if (RealizedContainers.TryGetValue(Index, out FrameworkElement Container))
            {
                IsNewlyRealized = false;
                return Container;
            }

            GeneratorPosition Position = Generator.GeneratorPositionFromIndex(Index);
            using (Generator.StartAt(Position, GeneratorDirection.Forward))
            {
                Container = (FrameworkElement)Generator.GenerateNext();
                Generator.PrepareItemContainer(Container);
            }

            IsNewlyRealized = true;
            return Container;
        }

        private void Virtualize(IRecyclingItemContainerGenerator Generator, int Index)
        {
            GeneratorPosition Position = Generator.GeneratorPositionFromIndex(Index);
            Generator.Remove(Position, 1);

#if !NET
            if (RealizedContainers.TryGetValue(Index, out FrameworkElement Container) &&
                RealizedContainers.Remove(Index))
#else
            if (RealizedContainers.Remove(Index, out FrameworkElement Container))
#endif
                RemoveInternalChildRange(InternalChildren.IndexOf(Container), 1);
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            if (IsItemsHost)
            {
                foreach (KeyValuePair<int, FrameworkElement> Data in RealizedContainers)
                    Data.Value.Arrange(RealizedLocations[Data.Key]);
            }
            else
            {
                int Count = InternalChildren.Count;
                if (Count == 0)
                    return FinalSize;

                double Iy = 0d;
                for (int i = 0; i < Count; i++)
                {
                    UIElement Item = InternalChildren[i];

                    Size Size = Item.DesiredSize;
                    Item.Arrange(new Rect(0, Iy, Size.Width, Size.Height));

                    Iy += Size.Height;
                }
            }

            return FinalSize;
        }

    }
}