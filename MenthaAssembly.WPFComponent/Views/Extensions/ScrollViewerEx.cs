using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly
{
    public static class ScrollViewerEx
    {
        public static readonly DependencyProperty AutoScrollToEndProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEnd", typeof(bool), typeof(ScrollViewerEx), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is ScrollViewer This)
                    {
                        if (This != null)
                        {
                            if (e.NewValue is true)
                            {
                                This.ScrollToEnd();
                                This.ScrollChanged += OnScrollChanged;
                            }
                            else
                            {
                                This.ScrollChanged -= OnScrollChanged;
                            }
                        }
                    }
                }));
        public static bool GetAutoScrollToEnd(ScrollViewer obj)
            => (bool)obj.GetValue(AutoScrollToEndProperty);
        public static void SetAutoScrollToEnd(ScrollViewer obj, bool value)
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
                if (e.ExtentHeightChange == 0)
                    SetIsScrolledToEnd(This, This.VerticalOffset.Equals(This.ScrollableHeight));

                if (GetIsScrolledToEnd(This) &&
                    e.ExtentHeightChange != 0)
                    This.ScrollToVerticalOffset(This.ExtentHeight);
            }
        }

    }
}
