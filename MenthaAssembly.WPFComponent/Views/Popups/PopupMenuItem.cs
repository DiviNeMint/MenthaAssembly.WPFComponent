using System;
using System.Collections.Generic;
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
    public class PopupMenuItem : MenuItem
    {
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
