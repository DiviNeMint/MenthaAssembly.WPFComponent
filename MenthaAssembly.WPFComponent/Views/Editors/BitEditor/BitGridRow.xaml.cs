using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Synpower4Net.Views
{
    public class BitGridRow : HeaderedItemsControl
    {
        internal readonly BitGridPresenter Owner;
        internal BitGridRow(BitGridPresenter Owner)
        {
            this.Owner = Owner;
        }
        public BitGridRow()
        {
        }

        static BitGridRow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BitGridRow), new FrameworkPropertyMetadata(typeof(BitGridRow)));
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is BitEditor;

        protected override DependencyObject GetContainerForItemOverride()
            => new BitEditor(this);

        protected override void PrepareContainerForItemOverride(DependencyObject Element, object Item)
        {
            base.PrepareContainerForItemOverride(Element, Item);
            BindingOperations.SetBinding(Element, BitEditor.SourceProperty, new Binding());
        }

        protected override void ClearContainerForItemOverride(DependencyObject Element, object Item)
        {
            base.ClearContainerForItemOverride(Element, Item);
            BindingOperations.ClearBinding(Element, BitEditor.SourceProperty);
        }

    }
}