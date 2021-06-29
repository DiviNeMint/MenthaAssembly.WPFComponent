using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class PopupMenuItem : MenuItem
    {
        public static new readonly DependencyProperty IconProperty =
              DependencyProperty.Register("Icon", typeof(ImageSource), typeof(PopupMenuItem), new PropertyMetadata(null));
        public new ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty DataTemplateProperty =
            DependencyProperty.Register("DataTemplate", typeof(DataTemplate), typeof(PopupMenuItem), new PropertyMetadata(default));
        public DataTemplate DataTemplate
        {
            get => (DataTemplate)GetValue(DataTemplateProperty);
            set => SetValue(DataTemplateProperty, value);
        }

        static PopupMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupMenuItem), new FrameworkPropertyMetadata(typeof(PopupMenuItem)));
        }

    }
}
