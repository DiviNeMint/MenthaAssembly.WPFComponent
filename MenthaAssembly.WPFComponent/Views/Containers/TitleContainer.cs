using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MenthaAssembly.Views
{
    [ContentProperty("Content")]
    public class TitleContainer : Control
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(TitleContainer), new PropertyMetadata(default));
        public object Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleTemplateProperty =
            DependencyProperty.Register("TitleTemplate", typeof(ControlTemplate), typeof(TitleContainer), new PropertyMetadata(default));
        public ControlTemplate TitleTemplate
        {
            get => (ControlTemplate)GetValue(TitleTemplateProperty);
            set => SetValue(TitleTemplateProperty, value);
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(object), typeof(TitleContainer), new PropertyMetadata(default));
        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        static TitleContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleContainer), new FrameworkPropertyMetadata(typeof(TitleContainer)));
        }
    }
}
