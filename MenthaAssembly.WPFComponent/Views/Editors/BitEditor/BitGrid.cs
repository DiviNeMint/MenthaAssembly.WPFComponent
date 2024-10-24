using MenthaAssembly.Views.Primitives;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
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

        public static readonly DependencyProperty BitSelectedStrokeProperty =
            DependencyProperty.Register(nameof(BitSelectedStroke), typeof(Brush), typeof(BitGrid), new PropertyMetadata(Brushes.Red));
        public Brush BitSelectedStroke
        {
            get => (Brush)GetValue(BitSelectedStrokeProperty);
            set => SetValue(BitSelectedStrokeProperty, value);
        }

        public static readonly DependencyProperty BitMouseOverStrokeProperty =
            DependencyProperty.Register(nameof(BitMouseOverStroke), typeof(Brush), typeof(BitGrid), new PropertyMetadata(Brushes.Red));
        public Brush BitMouseOverStroke
        {
            get => (Brush)GetValue(BitMouseOverStrokeProperty);
            set => SetValue(BitMouseOverStrokeProperty, value);
        }

        private readonly BitGridPresenter TemplatePresenter;
        public BitGrid()
        {
            // Presenter
            TemplatePresenter = new BitGridPresenter(this);
            TemplatePresenter.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(Source)) { Source = this });

            AddVisualChild(TemplatePresenter);
            Loaded += OnLoaded;
        }

        private RectangleAdorner SelectedAdorner;
        private RectangleAdorner MouseOverAdorner;
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AdornerLayer Layer = AdornerLayer.GetAdornerLayer(this);

            // MouseOver Adorner
            MouseOverAdorner = new(this);
            MouseOverAdorner.SetBinding(Shape.StrokeProperty, new Binding(nameof(BitMouseOverStroke)) { Source = this });
            MouseOverAdorner.StrokeThickness = 2;
            MouseOverAdorner.Visibility = Visibility.Collapsed;
            Layer.Add(MouseOverAdorner);

            // Selected Adorner
            SelectedAdorner = new(this);
            SelectedAdorner.SetBinding(Shape.StrokeProperty, new Binding(nameof(BitSelectedStroke)) { Source = this });
            SelectedAdorner.StrokeThickness = 2;
            SelectedAdorner.Visibility = Visibility.Collapsed;
            Layer.Add(SelectedAdorner);
        }

        internal void SetMouseOverAdorner(BitBlock Target, bool IsMouseOver)
        {
            if (IsMouseOver)
            {
                MouseOverAdorner.Position = Target.TransformToAncestor(this).Transform(new Point(-1, -1));
                MouseOverAdorner.Width = Target.ActualWidth + 1;
                MouseOverAdorner.Height = Target.ActualHeight + 1;
                MouseOverAdorner.Visibility = Visibility.Visible;
            }
            else
            {
                MouseOverAdorner.Visibility = Visibility.Collapsed;
            }
        }
        public void SetSelectedAdorner(BitBlock SelectedBit)
        {
            if (SelectedBit is not null)
            {
                SelectedAdorner.Position = SelectedBit.TransformToAncestor(this).Transform(new Point(-1, -1));
                SelectedAdorner.Width = SelectedBit.ActualWidth + 1;
                SelectedAdorner.Height = SelectedBit.ActualHeight + 1;
                SelectedAdorner.Visibility = Visibility.Visible;
            }
            else
            {
                SelectedAdorner.Visibility = Visibility.Collapsed;
            }
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