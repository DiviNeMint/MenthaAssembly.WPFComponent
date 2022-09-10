using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly.Views
{
    /// <summary>
    ///     The base class for all controls that contain single content and have a footer.
    /// </summary>
    [Localizability(LocalizationCategory.Text)]
    public class FooteredContentControl : ContentControl
    {
        public static readonly DependencyProperty FooterProperty =
              DependencyProperty.Register("Footer", typeof(object), typeof(FooteredContentControl), new FrameworkPropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is FooteredContentControl This)
                      {
                          This.SetValue(HasFooterPropertyKey, e.NewValue is not null);
                          This.OnFooterChanged(e.ToChangedEventArgs<object>());
                      }
                  }));
        [Category("Content")]
        public object Footer
        {
            get => GetValue(FooterProperty);
            set => SetValue(FooterProperty, value);
        }

        internal static readonly DependencyPropertyKey HasFooterPropertyKey =
                DependencyProperty.RegisterReadOnly("HasFooter", typeof(bool), typeof(FooteredContentControl), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty HasFooterProperty =
              HasFooterPropertyKey.DependencyProperty;
        [Bindable(false)]
        [Browsable(false)]
        public bool HasFooter
            => (bool)GetValue(HasFooterProperty);

        public static readonly DependencyProperty FooterTemplateProperty =
              DependencyProperty.Register("FooterTemplate", typeof(DataTemplate), typeof(FooteredContentControl), new FrameworkPropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is FooteredContentControl This)
                          This.OnFooterTemplateChanged(e.ToChangedEventArgs<DataTemplate>());
                  }));
        [Category("Content")]
        public DataTemplate FooterTemplate
        {
            get => (DataTemplate)GetValue(FooterTemplateProperty);
            set => SetValue(FooterTemplateProperty, value);
        }

        public static readonly DependencyProperty FooterTemplateSelectorProperty =
              DependencyProperty.Register("FooterTemplateSelector", typeof(DataTemplateSelector), typeof(FooteredContentControl), new FrameworkPropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is FooteredContentControl This)
                          This.OnFooterTemplateSelectorChanged(e.ToChangedEventArgs<DataTemplateSelector>());
                  }));
        [Category("Content")]
        public DataTemplateSelector FooterTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(FooterTemplateSelectorProperty);
            set => SetValue(FooterTemplateSelectorProperty, value);
        }

        public static readonly DependencyProperty FooterStringFormatProperty =
              DependencyProperty.Register("FooterStringFormat", typeof(string), typeof(FooteredContentControl), new FrameworkPropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is FooteredContentControl This)
                          This.OnFooterStringFormatChanged(e.ToChangedEventArgs<string>());
                  }));
        [Category("Content")]
        public string FooterStringFormat
        {
            get => (string)GetValue(FooterStringFormatProperty);
            set => SetValue(FooterStringFormatProperty, value);
        }

        static FooteredContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FooteredContentControl), new FrameworkPropertyMetadata(typeof(FooteredContentControl)));
        }

        protected virtual void OnFooterChanged(ChangedEventArgs<object> e)
        {

        }

        protected virtual void OnFooterTemplateChanged(ChangedEventArgs<DataTemplate> e)
        {

        }

        protected virtual void OnFooterStringFormatChanged(ChangedEventArgs<string> e)
        {

        }

        protected virtual void OnFooterTemplateSelectorChanged(ChangedEventArgs<DataTemplateSelector> e)
        {

        }

    }
}