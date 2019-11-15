using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly
{
    public static class ScrollViewerHelper
    {
        public static readonly DependencyProperty AutoScrollToEndProperty =
            DependencyProperty.RegisterAttached("AutoScrollToEnd", typeof(bool), typeof(ScrollViewerHelper), new PropertyMetadata(false,
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


        public static readonly DependencyProperty IsAutoScrollToEndProperty =
            DependencyProperty.RegisterAttached("IsAutoScrollToEnd", typeof(bool), typeof(ScrollViewerHelper), new PropertyMetadata(true));
        public static bool GetIsAutoScrollToEnd(ScrollViewer obj)
            => (bool)obj.GetValue(IsAutoScrollToEndProperty);
        private static void SetIsAutoScrollToEnd(ScrollViewer obj, bool value)
            => obj.SetValue(IsAutoScrollToEndProperty, value);

        private static void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer This)
            {
                if (e.ExtentHeightChange == 0)
                    SetIsAutoScrollToEnd(This, This.VerticalOffset.Equals(This.ScrollableHeight));

                if (GetIsAutoScrollToEnd(This) &&
                    e.ExtentHeightChange != 0)
                    This.ScrollToVerticalOffset(This.ExtentHeight);
            }
        }


    }
}
