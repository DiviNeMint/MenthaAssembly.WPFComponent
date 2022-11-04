using MenthaAssembly.MarkupExtensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MenthaAssembly.Views.Primitives
{
    [StyleTypedProperty(Property = "MenuStyleStyle", StyleTargetType = typeof(PropertyEditorMenuItem))]
    public class PropertyEditorMenu : PopupMenu
    {
        public static readonly DependencyProperty PropertyNameProperty =
            PropertyEditorItem.PropertyNameProperty.AddOwner(typeof(PropertyEditorMenu));
        public string PropertyName
        {
            get => (string)GetValue(PropertyNameProperty);
            set => SetValue(PropertyNameProperty, value);
        }

        static PropertyEditorMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorMenu), new FrameworkPropertyMetadata(typeof(PropertyEditorMenu)));
        }

        protected override bool IsItemItsOwnContainerOverride(object Item)
            => Item is PropertyEditorMenuItem || Item is Separator;

        protected override DependencyObject GetContainerForItemOverride()
            => new PropertyEditorMenuItem();

        protected override void PrepareContainerForItemOverride(DependencyObject Element, object DataContext)
        {
            if (Element is PropertyEditorMenuItem Item &&
                DataContext is PropertyEditorItemMenuData Data)
            {
                Item.SetBinding(MenuItem.CommandProperty, new Binding());
                Item.SetBinding(MenuItem.CommandParameterProperty, new Binding("Content") { RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(PropertyEditor), 1) });

                Item.Icon = Data.Icon;
                Item.SetBinding(HeaderedItemsControl.HeaderProperty, LanguageExtension.Create(Data.Header, Data.Header));
            }
        }

    }

}
