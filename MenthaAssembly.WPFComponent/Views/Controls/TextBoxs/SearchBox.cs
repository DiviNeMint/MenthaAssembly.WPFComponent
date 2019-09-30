using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public partial class SearchBox : TextBox
    {
        protected ICollectionView CurrentCollectionView;
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SearchBox), new PropertyMetadata(default,
                (d, e) =>
                {
                    if (d is SearchBox This)
                    {
                        if (This.CurrentCollectionView is ICollectionView OCView)
                            OCView.Filter = null;
                        if (CollectionViewSource.GetDefaultView(e.NewValue) is ICollectionView NCView)
                        {
                            NCView.Filter = (o) => This.Predicate(o, This.Text);
                            This.CurrentCollectionView = NCView;
                        }
                    }
                }));
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty PredicateProperty =
            DependencyProperty.Register("Predicate", typeof(Func<object, string, bool>), typeof(SearchBox), new PropertyMetadata(
                new Func<object, string, bool>((o, s) => string.IsNullOrEmpty(s) ? true : o.ToString().IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0),
                (d, e) =>
                {
                    if (d is SearchBox This &&
                        This.CurrentCollectionView is ICollectionView CView)
                        CView.Refresh();
                }));

        public Func<object, string, bool> Predicate
        {
            get => (Func<object, string, bool>)GetValue(PredicateProperty);
            set => SetValue(PredicateProperty, value);
        }


        public static readonly DependencyProperty IsSearchedProperty =
            DependencyProperty.Register("IsSearched", typeof(bool), typeof(SearchBox), new PropertyMetadata(false));
        public bool IsSearched
        {
            get => (bool)GetValue(IsSearchedProperty);
            protected set => SetValue(IsSearchedProperty, value);
        }


        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }


        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (!e.Handled)
            {
                CurrentCollectionView?.Refresh();
                IsSearched = !string.IsNullOrEmpty(Text);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Button") is Button PARTButton)
                PARTButton.Click += OnPARTButton_Click;

        }

        private void OnPARTButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsSearched)
            {
                Clear();
            }
        }
    }
}
