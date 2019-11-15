using MenthaAssembly.Views.Editors.Comparers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    [DefaultEvent("ContentChanged")]
    [ContentProperty("Content")]
    public class PropertyEditor : Control
    {
        public static IconContext DefaultIcon { get; } = new IconContext
        {
            Geometry = Geometry.Parse("M12.5,12.5L16.5,12.5 16.5,16.5 12.5,16.5z M6.5,12.5L10.5,12.5 10.5,16.5 6.5,16.5z M0.5,12.5L4.5,12.5 4.5,16.5 0.5,16.5z M6.5,6.5L10.5,6.5 10.5,10.5 6.5,10.5z M0.5,6.5L4.5,6.5 4.5,10.5 0.5,10.5z M0.5,0.5L4.5,0.5 4.5,4.5 0.5,4.5z"),
            Fill = Brushes.Black,
            Stroke = Brushes.Black,
            StrokeThickness = 1d,
            Size = new Size(22d, 22d)
        };

        public static readonly RoutedEvent ContentChangedEvent =
            EventManager.RegisterRoutedEvent("ContentChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PropertyEditor));
        public event RoutedEventHandler ContentChanged
        {
            add { AddHandler(ContentChangedEvent, value); }
            remove { RemoveHandler(ContentChangedEvent, value); }
        }

        public event EventHandler<PropertyChangedEventArgs> ContentPropertyChanged;

        public static readonly DependencyProperty ItemsSourceProperty =
              DependencyProperty.Register("ItemsSource", typeof(IList<PropertyInfo>), typeof(PropertyEditor), new PropertyMetadata(new ObservableCollection<PropertyInfo>()));
        internal IList<PropertyInfo> ItemsSource => (IList<PropertyInfo>)GetValue(ItemsSourceProperty);

        public static readonly DependencyProperty ContentProperty =
              DependencyProperty.Register("Content", typeof(object), typeof(PropertyEditor), new PropertyMetadata(default,
                  (d, e) =>
                  {
                      if (d is PropertyEditor This)
                      {
                          Type ObjectType = e.NewValue?.GetType();
                          bool IsSameType = ObjectType?.Equals(e.OldValue?.GetType()) ?? false;
                          if (!IsSameType)
                          {
                              This.ItemsSource.Clear();
                              This.PART_SearchBox?.Clear();
                          }

                          This.Dispatcher.BeginInvoke(new Action(() =>
                          {
                              if (ObjectType != null)
                              {
                                  EditorOptionAttribute EditorOption = ObjectType.GetCustomAttribute<EditorOptionAttribute>();
                                  if (!IsSameType)
                                  {
                                      //Icon
                                      if (This.PART_IconImage != null)
                                      {
                                          IconContext Icon = string.IsNullOrEmpty(EditorOption?.IconPath) ? DefaultIcon :
                                                             This.TryFindResource(EditorOption.IconPath) as IconContext ??
                                                             Application.Current.TryFindResource(EditorOption.IconPath) as IconContext ?? DefaultIcon;
                                          This.PART_IconImage.Source = Icon.ImageSource;
                                          This.PART_IconImage.Margin = Icon.Padding;

                                          if (Icon.Size.Equals(default))
                                              Icon.Size = new Size(30d, 30d);
                                          This.PART_IconImage.Width = Icon.Size.Width;
                                          This.PART_IconImage.Height = Icon.Size.Height;
                                      }

                                      //Name
                                      if (This.PART_NameTextBox != null)
                                      {
                                          if (!string.IsNullOrEmpty(EditorOption?.NamePath) &&
                                              ObjectType.GetProperty(EditorOption.NamePath) != null)
                                          {
                                              This.PART_NameTextBox.Visibility = Visibility.Visible;
                                              This.PART_NameTextBox.SetBinding(TextBox.TextProperty,
                                                 new Binding()
                                                 {
                                                     Path = new PropertyPath($"{nameof(This.Content)}.{EditorOption.NamePath}"),
                                                     Source = This
                                                 });
                                          }
                                          else
                                          {
                                              This.PART_NameTextBox.Visibility = Visibility.Hidden;
                                          }
                                      }

                                      //Type
                                      if (This.PART_TypeTextBlock != null)
                                          This.PART_TypeTextBlock.Text = EditorOption?.TypeDisplay ?? ObjectType.Name;

                                      //Property
                                      foreach (PropertyInfo item in ObjectType.GetProperties()
                                          .Where(i => i.Name != EditorOption?.NamePath &&
                                                      ObjectType.GetMember($"set_{i.Name}").Length > 0 &&
                                                      (i.GetCustomAttribute<EditorDisplayAttribute>()?.Visible ?? true)))
                                          This.AddProperty(item);
                                  }

                                  //Event
                                  if (e.OldValue is INotifyPropertyChanged OldContent)
                                      OldContent.PropertyChanged -= This.OnContentPropertyChanged;

                                  if ((EditorOption?.EnableContentPropertyChangedEvent ?? false) &&
                                      e.NewValue is INotifyPropertyChanged Content)
                                  {
                                      Content.PropertyChanged += This.OnContentPropertyChanged;
                                      if (This.ContentPropertyChanged != null)
                                          foreach (PropertyInfo item in This.ItemsSource.ToArray())
                                              if (This.ItemsSource.Contains(item))
                                                  This.ContentPropertyChanged.Invoke(This, new PropertyChangedEventArgs(item.Name));
                                  }
                              }
                              This.OnContentChanged();

                              if (e.OldValue != null)
                                  GC.Collect();
                          }));
                      }
                  }));

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(PropertyEditor), new PropertyMetadata(default));
        public object Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleTemplateProperty =
            DependencyProperty.Register("TitleTemplate", typeof(ControlTemplate), typeof(PropertyEditor), new PropertyMetadata(default));
        public ControlTemplate TitleTemplate
        {
            get => (ControlTemplate)GetValue(TitleTemplateProperty);
            set => SetValue(TitleTemplateProperty, value);
        }

        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(PropertyEditor), new PropertyMetadata(default));
        public Style ItemContainerStyle
        {
            get => (Style)GetValue(ItemContainerStyleProperty);
            set => SetValue(ItemContainerStyleProperty, value);
        }

        public static readonly DependencyProperty PropertyMenuStyleProperty =
            DependencyProperty.Register("PropertyMenuStyle", typeof(Style), typeof(PropertyEditor), new PropertyMetadata(default));
        public Style PropertyMenuStyle
        {
            get => (Style)GetValue(PropertyMenuStyleProperty);
            set => SetValue(PropertyMenuStyleProperty, value);
        }

        protected Image PART_IconImage;
        protected TextBox PART_NameTextBox;
        protected TextBlock PART_TypeTextBlock;
        protected SearchBox PART_SearchBox;

        static PropertyEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditor), new FrameworkPropertyMetadata(typeof(PropertyEditor)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_IconImage") is Image PART_IconImage)
                this.PART_IconImage = PART_IconImage;

            if (GetTemplateChild("PART_NameTextBox") is TextBox PART_NameTextBox)
                this.PART_NameTextBox = PART_NameTextBox;

            if (GetTemplateChild("PART_TypeTextBlock") is TextBlock PART_TypeTextBlock)
                this.PART_TypeTextBlock = PART_TypeTextBlock;

            if (GetTemplateChild("PART_SearchBox") is SearchBox PART_SearchBox)
            {
                this.PART_SearchBox = PART_SearchBox;
                PART_SearchBox.Predicate = (o, s) =>
                {
                    if (o is PropertyInfo Info)
                        return string.IsNullOrEmpty(s) ? true : Info.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
                    return false;
                };
            }

            if (CollectionViewSource.GetDefaultView(ItemsSource) is ListCollectionView View)
                View.CustomSort = new PropertyEditorComparer();
        }

        protected virtual void OnContentChanged()
            => RaiseEvent(new RoutedEventArgs(ContentChangedEvent, this));
        protected virtual void OnContentPropertyChanged(object sender, PropertyChangedEventArgs e)
            => ContentPropertyChanged?.Invoke(this, e);

        public void AddProperty(PropertyInfo Info)
        {
            if (!ItemsSource.Contains(Info))
                ItemsSource.Add(Info);
        }
        public void RemoveProperty(PropertyInfo Info)
            => ItemsSource.Remove(Info);
    }
}
