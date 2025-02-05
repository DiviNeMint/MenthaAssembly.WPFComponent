using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly.MarkupExtensions
{
    public static class ScrollViewerEx
    {
        public static readonly DependencyProperty AutoScrollToEndProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEnd", typeof(bool), typeof(ScrollViewerEx), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is ScrollViewer Viewer)
                    {
                        if (e.NewValue is true)
                        {
                            Viewer.ScrollToEnd();
                            Viewer.ScrollChanged += OnScrollChanged;
                        }
                        else
                        {
                            Viewer.ScrollChanged -= OnScrollChanged;
                        }
                    }
                    else if (d is FrameworkElement Element)
                    {
                        if (Element.IsLoaded)
                        {
                            if (d.FindVisualChildren<ScrollViewer>().FirstOrDefault() is ScrollViewer Child)
                                SetAutoScrollToEnd(Child, true);
                        }
                        else
                        {
                            Element.Loaded += OnElementLoaded;
                            void OnElementLoaded(object sender, RoutedEventArgs e)
                            {
                                if (Element.IsArrangeValid)
                                {
                                    Element.Loaded -= OnElementLoaded;
                                    if (Element.FindVisualChildren<ScrollViewer>().FirstOrDefault() is ScrollViewer Child)
                                        SetAutoScrollToEnd(Child, true);
                                }
                            }
                        }
                    }
                }));

        public static bool GetAutoScrollToEnd(DependencyObject obj)
            => (bool)obj.GetValue(AutoScrollToEndProperty);
        public static void SetAutoScrollToEnd(DependencyObject obj, bool value)
            => obj.SetValue(AutoScrollToEndProperty, value);

        public static readonly DependencyProperty IsScrolledToEndProperty =
            DependencyProperty.RegisterAttached("IsScrolledToEnd", typeof(bool), typeof(ScrollViewerEx), new PropertyMetadata(true));
        public static bool GetIsScrolledToEnd(ScrollViewer obj)
            => (bool)obj.GetValue(IsScrolledToEndProperty);
        private static void SetIsScrolledToEnd(ScrollViewer obj, bool value)
            => obj.SetValue(IsScrolledToEndProperty, value);

        private static void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer This)
            {
                bool IsScrolledToEnd = GetIsScrolledToEnd(This);
                if (This.IsLoaded)
                {
                    if (e.ExtentHeightChange == 0)
                        SetIsScrolledToEnd(This, Math.Round(This.VerticalOffset, 3) == Math.Round(This.ScrollableHeight, 3));
                    else if (IsScrolledToEnd)
                        This.ScrollToEnd();
                }
                else if (IsScrolledToEnd)
                {
                    This.ScrollToEnd();
                }
                else if (e.VerticalChange < 0)
                {
                    This.ScrollToVerticalOffset(-e.VerticalChange);
                }
            }
        }

    }
}