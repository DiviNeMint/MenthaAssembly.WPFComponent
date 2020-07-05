using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MenthaAssembly.Views.Primitives
{
    public class PropertyEditorMenuItem : MenuItem
    {
        public static new readonly DependencyProperty IconProperty =
              DependencyProperty.Register("Icon", typeof(ImageSource), typeof(PropertyEditorMenuItem), new PropertyMetadata(null));
        public new ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        static PropertyEditorMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorMenuItem), new FrameworkPropertyMetadata(typeof(PropertyEditorMenuItem)));
        }

    }
}
