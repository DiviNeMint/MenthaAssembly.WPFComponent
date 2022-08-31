using System.Windows;
using System.Windows.Data;

namespace MenthaAssembly.Views
{
    public class TreeView : System.Windows.Controls.TreeView
    {
        public static readonly DependencyProperty IndentProperty =
                TreeViewItem.IndentProperty.AddOwner(typeof(TreeView), new FrameworkPropertyMetadata(20d,
                    (d, e) =>
                    {
                        if (d is TreeView This)
                            This.OnIndentChanged(e.ToChangedEventArgs<double>());
                    }));
        public double Indent
        {
            get => (double)GetValue(IndentProperty);
            set => SetValue(IndentProperty, value);
        }


        public static readonly DependencyProperty IndentExpandButtonProperty =
                TreeViewItem.IndentExpandButtonProperty.AddOwner(typeof(TreeView), new FrameworkPropertyMetadata(true,
                    (d, e) =>
                    {
                        if (d is TreeView This)
                            This.OnIndentExpandButtonChanged(e.ToChangedEventArgs<bool>());
                    }));
        public bool IndentExpandButton
        {
            get => (bool)GetValue(IndentExpandButtonProperty);
            set => SetValue(IndentExpandButtonProperty, value);
        }

        static TreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(typeof(TreeView)));
        }

        protected virtual void OnIndentChanged(ChangedEventArgs<double> e)
        {

        }

        protected virtual void OnIndentExpandButtonChanged(ChangedEventArgs<bool> e)
        {

        }

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is TreeViewItem;
        protected override DependencyObject GetContainerForItemOverride()
            => new TreeViewItem();

        protected override void PrepareContainerForItemOverride(DependencyObject Element, object Data)
        {
            base.PrepareContainerForItemOverride(Element, Data);

            if (Element is TreeViewItem Item)
                PrepareTreeViewItem(Item, Data);
        }
        private void PrepareTreeViewItem(TreeViewItem Item, object Data)
        {
            Item.DataContext = Data;
            Item.SetValue(TreeViewItem.DepthPropertyKey, 0);
            Item.SetBinding(TreeViewItem.IndentProperty, new Binding(nameof(Indent)) { Source = this });
            Item.SetBinding(TreeViewItem.IndentExpandButtonProperty, new Binding(nameof(IndentExpandButton)) { Source = this });
        }

        protected override void ClearContainerForItemOverride(DependencyObject Element, object Data)
        {
            base.ClearContainerForItemOverride(Element, Data);

            if (Element is TreeViewItem Item)
                ResetTreeViewItem(Item, Data);
        }
        private void ResetTreeViewItem(TreeViewItem Item, object Data)
        {
            Item.DataContext = null;
            Item.ClearValue(TreeViewItem.DepthPropertyKey);
            Item.ClearValue(TreeViewItem.IndentProperty);
            Item.ClearValue(TreeViewItem.IndentExpandButtonProperty);
        }

    }
}