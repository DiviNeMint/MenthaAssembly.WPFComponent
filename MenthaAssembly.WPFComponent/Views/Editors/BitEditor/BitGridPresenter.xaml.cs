using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MenthaAssembly.Views
{
    internal class BitGridPresenter(BitGrid Owner) : ItemsControl
    {
        static BitGridPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BitGridPresenter), new FrameworkPropertyMetadata(typeof(BitGridPresenter)));
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is BitGridRow;

        protected override DependencyObject GetContainerForItemOverride()
            => new BitGridRow(this);

        protected override void PrepareContainerForItemOverride(DependencyObject Element, object Item)
        {
            base.PrepareContainerForItemOverride(Element, Item);

            // Source
            BindingOperations.SetBinding(Element, ItemsSourceProperty, new Binding());

            // Style
            BindingOperations.SetBinding(Element, StyleProperty, new Binding(nameof(BitGrid.BitRowStyle)) { Source = Owner });
            BindingOperations.SetBinding(Element, ItemContainerStyleProperty, new Binding(nameof(BitGrid.BitEditorStyle)) { Source = Owner });
        }

        protected override void ClearContainerForItemOverride(DependencyObject Element, object Item)
        {
            base.ClearContainerForItemOverride(Element, Item);

            // Source
            BindingOperations.ClearBinding(Element, ItemsSourceProperty);

            // Style
            BindingOperations.ClearBinding(Element, StyleProperty);
            BindingOperations.ClearBinding(Element, ItemContainerStyleProperty);
        }

    }
}