using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MenthaAssembly.Views
{
    public abstract class VirtualizingPanel : System.Windows.Controls.VirtualizingPanel
    {
        public static readonly DependencyProperty IsTemplateGeneratedProperty =
            DependencyProperty.RegisterAttached("IsTemplateGenerated", typeof(bool), typeof(VirtualizingPanel),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static bool GetIsTemplateGenerated(DependencyObject obj)
            => (bool)obj.GetValue(IsTemplateGeneratedProperty);
        public static void SetIsTemplateGenerated(DependencyObject obj, bool value)
            => obj.SetValue(IsTemplateGeneratedProperty, value);

        private ItemContainerGenerator _itemContainerGenerator;
        protected new ItemContainerGenerator ItemContainerGenerator
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

        protected ReadOnlyCollection<object> Items
            => ItemContainerGenerator.Items;

        private readonly bool IsDesignMode;
        protected VirtualizingPanel()
        {
            IsDesignMode = DesignerProperties.GetIsInDesignMode(this);
        }

        protected bool IsTemplateGenerated
        {
            get => !IsDesignMode && GetIsTemplateGenerated(this) && !Items.Any(i => i is FrameworkElement);
            set => SetIsTemplateGenerated(this, value);
        }

        protected readonly Dictionary<int, FrameworkElement> RealizedContainers = [];
        protected FrameworkElement Realize(IRecyclingItemContainerGenerator Generator, int Index)
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
        protected FrameworkElement GetContainer(IRecyclingItemContainerGenerator Generator, int Index, out bool IsNewlyRealized)
        {
            if (RealizedContainers.TryGetValue(Index, out FrameworkElement Container))
            {
                if (DependencyHelper.IsDisconnectedItem(Container.DataContext))
                {
                    RealizedContainers.Remove(Index);
                }
                else
                {
                    IsNewlyRealized = false;
                    return Container;
                }
            }

            Container = (FrameworkElement)ItemContainerGenerator.ContainerFromIndex(Index);
            if (Container is null)
            {
                GeneratorPosition Position = Generator.GeneratorPositionFromIndex(Index);
                using (Generator.StartAt(Position, GeneratorDirection.Forward))
                {
                    Container = (FrameworkElement)Generator.GenerateNext();
                    Generator.PrepareItemContainer(Container);
                }
            }

            IsNewlyRealized = true;
            return Container;
        }

        protected void Virtualize(IRecyclingItemContainerGenerator Generator, int Index)
        {
            if (IsDesignMode)
                return;

            GeneratorPosition Position = Generator.GeneratorPositionFromIndex(Index);
            if (Position.Offset == 0)
            {
                Generator.Remove(Position, 1);

#if !NET
                if (RealizedContainers.TryGetValue(Index, out FrameworkElement Container) &&
                    RealizedContainers.Remove(Index))
#else
                if (RealizedContainers.Remove(Index, out FrameworkElement Container))
#endif
                    RemoveInternalChildRange(InternalChildren.IndexOf(Container), 1);
            }
            else
            {
                RealizedContainers.Remove(Index);
            }
        }

    }
}