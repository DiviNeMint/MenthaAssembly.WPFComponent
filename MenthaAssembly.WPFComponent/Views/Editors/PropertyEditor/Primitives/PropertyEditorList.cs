using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MenthaAssembly.Views.Primitives
{
    [StyleTypedProperty(Property = "MenuStyleStyle", StyleTargetType = typeof(PropertyEditorItem))]
    internal class PropertyEditorList : ItemsControl
    {
        public static readonly DependencyProperty EnableMenuProperty =
            PropertyEditorItem.EnableMenuProperty.AddOwner(typeof(PropertyEditorList), new PropertyMetadata(true));
        public bool EnableMenu
        {
            get => (bool)GetValue(EnableMenuProperty);
            set => SetValue(EnableMenuProperty, value);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            PropertyEditorItem Item = new PropertyEditorItem();
            Item.SetBinding(PropertyEditorItem.EnableMenuProperty, new Binding(nameof(EnableMenu)) { Source = this });
            Item.SetBinding(PropertyEditorItem.TargetObjectProperty, new Binding("Content") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(PropertyEditor), 1) });
            return Item;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is PropertyEditorItem;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Grid.SetIsSharedSizeScope(this, true);
        }

    }
}
