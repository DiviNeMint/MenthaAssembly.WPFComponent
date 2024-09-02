using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Synpower4Net.Views
{
    public class BitGrid : FrameworkElement
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(IEnumerable), typeof(BitGrid),
                new PropertyMetadata(null));
        public IEnumerable Source
        {
            get => (IEnumerable)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty BitEditorStyleProperty =
            DependencyProperty.Register(nameof(BitEditorStyle), typeof(Style), typeof(BitGrid),
                new PropertyMetadata(null));
        public Style BitEditorStyle
        {
            get => (Style)GetValue(BitEditorStyleProperty);
            set => SetValue(BitEditorStyleProperty, value);
        }

        public static readonly DependencyProperty BitRowStyleProperty =
            DependencyProperty.Register(nameof(BitRowStyle), typeof(Style), typeof(BitGrid),
                new PropertyMetadata(null));
        public Style BitRowStyle
        {
            get => (Style)GetValue(BitRowStyleProperty);
            set => SetValue(BitRowStyleProperty, value);
        }

        private readonly BitGridPresenter TemplatePresenter;
        public BitGrid()
        {
            TemplatePresenter = new BitGridPresenter(this);
            TemplatePresenter.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(Source)) { Source = this });

            AddVisualChild(TemplatePresenter);
        }

        protected override int VisualChildrenCount
            => 1;
        protected override Visual GetVisualChild(int index)
            => TemplatePresenter;

        protected override Size MeasureOverride(Size AvailableSize)
        {
            TemplatePresenter.Measure(AvailableSize);
            return TemplatePresenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            TemplatePresenter.Arrange(new(FinalSize));
            return FinalSize;
        }

    }
}