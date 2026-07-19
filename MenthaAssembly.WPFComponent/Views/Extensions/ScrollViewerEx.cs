using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        public static readonly DependencyProperty PassMouseWheelAtBoundaryProperty =
            DependencyProperty.RegisterAttached("PassMouseWheelAtBoundary", typeof(bool), typeof(ScrollViewerEx), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is not ScrollViewer Viewer)
                        return;

                    if (e.NewValue is true)
                        Viewer.PreviewMouseWheel += OnPreviewMouseWheelAtBoundary;
                    else
                        Viewer.PreviewMouseWheel -= OnPreviewMouseWheelAtBoundary;
                }));
        public static bool GetPassMouseWheelAtBoundary(DependencyObject obj)
            => (bool)obj.GetValue(PassMouseWheelAtBoundaryProperty);
        public static void SetPassMouseWheelAtBoundary(DependencyObject obj, bool value)
            => obj.SetValue(PassMouseWheelAtBoundaryProperty, value);

        public static readonly DependencyProperty IsScrolledToEndProperty =
            DependencyProperty.RegisterAttached("IsScrolledToEnd", typeof(bool), typeof(ScrollViewerEx), new PropertyMetadata(true));
        public static bool GetIsScrolledToEnd(ScrollViewer obj)
            => (bool)obj.GetValue(IsScrolledToEndProperty);
        private static void SetIsScrolledToEnd(ScrollViewer obj, bool value)
            => obj.SetValue(IsScrolledToEndProperty, value);

        private static void OnPreviewMouseWheelAtBoundary(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer This)
                return;

            DependencyObject OriginalSource = e.OriginalSource as DependencyObject;
            ScrollViewer SourceViewer = OriginalSource as ScrollViewer ?? OriginalSource.FindParent<ScrollViewer>();
            if (SourceViewer is not null &&
                !ReferenceEquals(SourceViewer, This))
                return;

            if ((e.Delta > 0 && This.VerticalOffset > 0d) ||
                (e.Delta < 0 && This.VerticalOffset < This.ScrollableHeight))
                return;

            if (This.FindParent<UIElement>() is not UIElement Parent)
                return;

            e.Handled = true;
            MouseWheelEventArgs Args = new(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = Parent
            };
            Parent.RaiseEvent(Args);
        }

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
                    else
                        SetIsScrolledToEnd(This, Math.Round(This.VerticalOffset, 3) == Math.Round(This.ScrollableHeight, 3));
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
