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
using System.Windows.Media;

namespace MenthaAssembly.Views
{

    public class PropertyEditor : Control
    {
        public static IconSource DefaultIcon { get; } = new IconSource
        {
            Geometry = Geometry.Parse("M12.5,12.5L16.5,12.5 16.5,16.5 12.5,16.5z M6.5,12.5L10.5,12.5 10.5,16.5 6.5,16.5z M0.5,12.5L4.5,12.5 4.5,16.5 0.5,16.5z M6.5,6.5L10.5,6.5 10.5,10.5 6.5,10.5z M0.5,6.5L4.5,6.5 4.5,10.5 0.5,10.5z M0.5,0.5L4.5,0.5 4.5,4.5 0.5,4.5z"),
            Fill = Brushes.Black,
            Stroke = Brushes.Black,
            StrokeThickness = 1d,
            Size = new Size(22d, 22d)
        };

        public static readonly RoutedEvent ContentChangedEvent =
            EventManager.RegisterRoutedEvent("ContentChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SwitchView));
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
                          This.ItemsSource.Clear();
                          if (This.GetTemplateChild("PART_SearchBox") is SearchBox PART_SearchBox)
                              PART_SearchBox.Clear();

                          if (e.NewValue?.GetType() is Type ObjectType)
                          {
                              EditorOptionAttribute EditorOption = ObjectType.GetCustomAttribute<EditorOptionAttribute>();
                              // Icon
                              if (This.GetTemplateChild("PART_IconImage") is Image PART_IconImage)
                              {
                                  IconSource IconSource = string.IsNullOrEmpty(EditorOption?.IconPath) ? DefaultIcon : (This.TryFindResource(EditorOption.IconPath) as IconSource ?? DefaultIcon);
                                  PART_IconImage.Source = IconSource.GetIcon();
                                  PART_IconImage.Margin = IconSource.Padding;

                                  if (IconSource.Size.Equals(default))
                                      IconSource.Size = new Size(30d, 30d);
                                  PART_IconImage.Width = IconSource.Size.Width;
                                  PART_IconImage.Height = IconSource.Size.Height;
                              }

                              // Name
                              if (This.GetTemplateChild("PART_NameTextBox") is TextBox PART_NameTextBox)
                              {
                                  if (!string.IsNullOrEmpty(EditorOption?.NamePath) &&
                                      ObjectType.GetProperty(EditorOption.NamePath) != null)
                                  {
                                      PART_NameTextBox.Visibility = Visibility.Visible;
                                      PART_NameTextBox.SetBinding(TextBox.TextProperty,
                                         new Binding()
                                         {
                                             Path = new PropertyPath(EditorOption.NamePath),
                                             Source = e.NewValue
                                         });
                                  }
                                  else
                                  {
                                      PART_NameTextBox.Visibility = Visibility.Hidden;
                                  }
                              }

                              // Type
                              if (This.GetTemplateChild("PART_TypeTextBlock") is TextBlock PART_TypeTextBlock)
                                  PART_TypeTextBlock.Text = EditorOption?.TypeDisplay ?? ObjectType.Name;

                              // Property
                              foreach (PropertyInfo item in ObjectType.GetProperties()
                                  .Where(i => i.Name != EditorOption?.NamePath &&
                                              ObjectType.GetMember($"set_{i.Name}").Length > 0 &&
                                              (i.GetCustomAttribute<EditorDisplayAttribute>()?.Visible ?? true)))
                                  This.AddProperty(item);


                              // Event
                              if (e.OldValue is INotifyPropertyChanged OldContent)
                                  OldContent.PropertyChanged -= This.OnContentPropertyChanged;

                              if ((EditorOption?.EnableContentPropertyChangedEvent ?? false) &&
                                  e.NewValue is INotifyPropertyChanged Content)
                              {
                                  Content.PropertyChanged += This.OnContentPropertyChanged;
                                  if (This.ContentPropertyChanged != null)
                                      foreach (PropertyInfo item in This.ItemsSource.ToArray())
                                          if (This.ItemsSource.Contains(item))
                                              This.ContentPropertyChanged.Invoke(Content, new PropertyChangedEventArgs(item.Name));
                              }
                          }
                          This.OnContentChanged();
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

        static PropertyEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditor), new FrameworkPropertyMetadata(typeof(PropertyEditor)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_SearchBox") is SearchBox PART_SearchBox)
                PART_SearchBox.Predicate = (o, s) =>
                {
                    if (o is PropertyInfo Info)
                        return string.IsNullOrEmpty(s) ? true : Info.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
                    return false;
                };

            if (CollectionViewSource.GetDefaultView(ItemsSource) is ListCollectionView View)
                View.CustomSort = new PropertyEditorComparer();
        }

        protected virtual void OnContentChanged()
            => RaiseEvent(new RoutedEventArgs(ContentChangedEvent, this));
        protected virtual void OnContentPropertyChanged(object sender, PropertyChangedEventArgs e)
            => ContentPropertyChanged?.Invoke(sender, e);

        public void AddProperty(PropertyInfo Info)
        {
            if (!ItemsSource.Contains(Info))
                ItemsSource.Add(Info);
        }
        public void RemoveProperty(PropertyInfo Info)
            => ItemsSource.Remove(Info);

    }
}
