using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MenthaAssembly.Views
{
    public class TreeViewItem : System.Windows.Controls.TreeViewItem
    {
        internal static readonly DependencyPropertyKey DepthPropertyKey =
                DependencyProperty.RegisterReadOnly("Depth", typeof(int), typeof(TreeViewItem), new FrameworkPropertyMetadata(-1,
                    (d, e) =>
                    {
                        if (d is TreeViewItem This)
                            This.OnDepthChanged(e.ToChangedEventArgs<int>());
                    }));
        public static readonly DependencyProperty DepthProperty =
                DepthPropertyKey.DependencyProperty;
        public int Depth
            => (int)GetValue(DepthProperty);

        public static readonly DependencyProperty IndentProperty =
                DependencyProperty.Register("Indent", typeof(double), typeof(TreeViewItem), new FrameworkPropertyMetadata(20d,
                    (d, e) =>
                    {
                        if (d is TreeViewItem This)
                            This.OnIndentChanged(e.ToChangedEventArgs<double>());
                    }));
        public double Indent
        {
            get => (double)GetValue(IndentProperty);
            set => SetValue(IndentProperty, value);
        }

        public static readonly DependencyProperty IndentExpandButtonProperty =
                DependencyProperty.Register("IndentExpandButton", typeof(bool), typeof(TreeViewItem), new FrameworkPropertyMetadata(true,
                    (d, e) =>
                    {
                        if (d is TreeViewItem This)
                            This.OnIndentExpandButtonChanged(e.ToChangedEventArgs<bool>());
                    }));
        public bool IndentExpandButton
        {
            get => (bool)GetValue(IndentExpandButtonProperty);
            set => SetValue(IndentExpandButtonProperty, value);
        }

        public static readonly DependencyProperty ExpandButtonStyleProperty =
                DependencyProperty.Register("ExpandButtonStyle", typeof(Style), typeof(TreeViewItem), new FrameworkPropertyMetadata(null));
        public Style ExpandButtonStyle
        {
            get => (Style)GetValue(ExpandButtonStyleProperty);
            set => SetValue(ExpandButtonStyleProperty, value);
        }

        static TreeViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeViewItem), new FrameworkPropertyMetadata(typeof(TreeViewItem)));
        }

        private DockPanel PART_Header;
        private ContentControl PART_HeaderContent;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Header") is DockPanel PART_Header)
                this.PART_Header = PART_Header;

            if (GetTemplateChild("PART_HeaderContent") is ContentControl PART_HeaderContent)
                this.PART_HeaderContent = PART_HeaderContent;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsVisualIndentInvalid)
                UpdateVisualIndent();
        }

        protected virtual void OnDepthChanged(ChangedEventArgs<int> e)
            => InvalidateVisualIndent();

        protected virtual void OnIndentChanged(ChangedEventArgs<double> e)
            => InvalidateVisualIndent();

        protected virtual void OnIndentExpandButtonChanged(ChangedEventArgs<bool> e)
            => InvalidateVisualIndent();

        protected void InvalidateVisualIndent()
        {
            if (!IsLoaded)
                IsVisualIndentInvalid = true;
            else
                UpdateVisualIndent();
        }

        private bool IsVisualIndentInvalid = false;
        protected virtual void UpdateVisualIndent()
        {
            Thickness Padding = new Thickness(Indent * Depth, 0, 0, 0);

            if (IndentExpandButton)
            {
                PART_Header.Margin = Padding;
                PART_HeaderContent.Margin = new Thickness();
            }
            else
            {
                PART_Header.Margin = new Thickness();
                PART_HeaderContent.Margin = Padding;
            }
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
            Item.SetValue(DepthPropertyKey, Depth + 1);
            Item.SetBinding(IndentProperty, new Binding(nameof(Indent)) { Source = this });
            Item.SetBinding(IndentExpandButtonProperty, new Binding(nameof(IndentExpandButton)) { Source = this });
            Item.SetBinding(ExpandButtonStyleProperty, new Binding(nameof(ExpandButtonStyle)) { Source = this });
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
            Item.ClearValue(DepthPropertyKey);
            Item.ClearValue(IndentProperty);
            Item.ClearValue(IndentExpandButtonProperty);
            Item.ClearValue(ExpandButtonStyleProperty);
        }

    }
}
