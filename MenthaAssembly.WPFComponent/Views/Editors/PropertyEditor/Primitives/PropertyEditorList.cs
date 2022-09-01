using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MenthaAssembly.Views.Primitives
{
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(PropertyEditorItem))]
    internal class PropertyEditorList : ItemsControl
    {
        public static readonly DependencyProperty EnableMenuProperty =
            PropertyEditorItem.EnableMenuProperty.AddOwner(typeof(PropertyEditorList), new PropertyMetadata(true));
        public bool EnableMenu
        {
            get => (bool)GetValue(EnableMenuProperty);
            set => SetValue(EnableMenuProperty, value);
        }

        private readonly ConcurrentQueue<PropertyEditorItem> CacheItems = new ConcurrentQueue<PropertyEditorItem>();
        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is PropertyEditorItem;
        protected override DependencyObject GetContainerForItemOverride()
            => CacheItems.TryDequeue(out PropertyEditorItem Item) ? Item : new PropertyEditorItem();

        protected override void PrepareContainerForItemOverride(DependencyObject Element, object Data)
        {
            base.PrepareContainerForItemOverride(Element, Data);

            if (Element is PropertyEditorItem Item)
                PreparePropertyEditorItem(Item, Data);
        }
        private void PreparePropertyEditorItem(PropertyEditorItem Item, object Data)
        {
            Item.DataContext = Data;
            Item.SetBinding(PropertyEditorItem.EnableMenuProperty, new Binding(nameof(EnableMenu)) { Source = this });
            Item.SetBinding(PropertyEditorItem.TargetObjectProperty, new Binding("Content") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(PropertyEditor), 1) });
        }

        protected override void ClearContainerForItemOverride(DependencyObject Element, object Data)
        {
            base.ClearContainerForItemOverride(Element, Data);

            if (Element is PropertyEditorItem Item)
            {
                ResetPropertyEditorItem(Item, Data);
                CacheItems.Enqueue(Item);
            }
        }
        private void ResetPropertyEditorItem(PropertyEditorItem Item, object Data)
        {
            Item.DataContext = null;
            Item.ClearValue(PropertyEditorItem.EnableMenuProperty);
            Item.ClearValue(PropertyEditorItem.TargetObjectProperty);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Grid.SetIsSharedSizeScope(this, true);
        }

    }
}