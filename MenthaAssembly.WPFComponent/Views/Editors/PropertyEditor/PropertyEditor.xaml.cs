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
    [ContentProperty("Content")]
    [DefaultEvent("ContentChanged")]
    [StyleTypedProperty(Property = "MenuStyleStyle", StyleTargetType = typeof(PropertyEditorItem))]
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
            add => AddHandler(ContentChangedEvent, value);
            remove => RemoveHandler(ContentChangedEvent, value);
        }

        public event EventHandler<PropertyChangedEventArgs> ContentPropertyChanged;

        internal static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(ICollection<PropertyEditorData>), typeof(PropertyEditor), new PropertyMetadata(new ObservableCollection<PropertyEditorData>()));
        internal ICollection<PropertyEditorData> ItemsSource
            => (ICollection<PropertyEditorData>)GetValue(ItemsSourceProperty);

        public static readonly DependencyProperty EnableMenuProperty =
            PropertyEditorItem.EnableMenuProperty.AddOwner(typeof(PropertyEditor), new PropertyMetadata(true));
        public bool EnableMenu
        {
            get => (bool)GetValue(EnableMenuProperty);
            set => SetValue(EnableMenuProperty, value);
        }

        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(PropertyEditor), new PropertyMetadata(default));
        public Style ItemContainerStyle
        {
            get => (Style)GetValue(ItemContainerStyleProperty);
            set => SetValue(ItemContainerStyleProperty, value);
        }

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

                        if (ObjectType != null)
                            This.Dispatcher.BeginInvoke(new Action(() =>
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
                                                                            .Where(i => ObjectType.GetMember($"set_{i.Name}").Length > 0))
                                        This.AddProperty(item, item.GetCustomAttribute<EditorDisplayAttribute>(), EditorOption);
                                }

                                //Event
                                if (e.OldValue is INotifyPropertyChanged OldContent)
                                    OldContent.PropertyChanged -= This.OnContentPropertyChanged;

                                if ((EditorOption?.EnableContentPropertyChangedEvent ?? false) &&
                                    e.NewValue is INotifyPropertyChanged Content)
                                {
                                    Content.PropertyChanged += This.OnContentPropertyChanged;
                                    if (This.ContentPropertyChanged != null)
                                        foreach (PropertyEditorData item in This.ItemsSource)
                                            if (This.ItemsSource.Contains(item))
                                                This.ContentPropertyChanged.Invoke(This, new PropertyChangedEventArgs(item.PropertyName));
                                }

                            }));

                        if (e.OldValue != null)
                            GC.Collect();
                    }
                }));
        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        private SortDescription AlphabeticSortDescription = new SortDescription("PropertyName", ListSortDirection.Ascending);
        public static readonly DependencyProperty AlphabeticProperty =
              DependencyProperty.Register("Alphabetic", typeof(ListSortDirection?), typeof(PropertyEditor), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is PropertyEditor This &&
                          CollectionViewSource.GetDefaultView(This.ItemsSource) is ListCollectionView View)
                      {
                          View.SortDescriptions.Remove(This.AlphabeticSortDescription);
                          if (e.NewValue is ListSortDirection Sorting)
                          {
                              This.AlphabeticSortDescription.Direction = Sorting;
                              View.SortDescriptions.Add(This.AlphabeticSortDescription);
                          }
                      }
                  }));
        public ListSortDirection Alphabetic
        {
            get => (ListSortDirection)GetValue(AlphabeticProperty);
            set => SetValue(AlphabeticProperty, value);
        }

        public static readonly DependencyProperty EnableGroupingProperty =
              DependencyProperty.Register("EnableGrouping", typeof(bool), typeof(PropertyEditor), new PropertyMetadata(false,
                  (d, e) =>
                  {
                      if (d is PropertyEditor This &&
                          CollectionViewSource.GetDefaultView(This.ItemsSource) is ListCollectionView View)
                      {
                          if (e.NewValue is true)
                              View.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
                          else
                              View.GroupDescriptions.Clear();
                      }
                  }));
        public bool EnableGrouping
        {
            get => (bool)GetValue(EnableGroupingProperty);
            set => SetValue(EnableGroupingProperty, value);
        }

        public static readonly DependencyProperty GroupContainerStyleProperty =
            DependencyProperty.Register("GroupContainerStyle", typeof(Style), typeof(PropertyEditor), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Style GroupContainerStyle
        {
            get => (Style)GetValue(GroupContainerStyleProperty);
            set => SetValue(GroupContainerStyleProperty, value);
        }

        static PropertyEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditor), new FrameworkPropertyMetadata(typeof(PropertyEditor)));
        }

        protected Image PART_IconImage;
        protected TextBox PART_NameTextBox;
        protected TextBlock PART_TypeTextBlock;
        protected SearchBox PART_SearchBox;
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
                    if (o is PropertyEditorData Data)
                        return string.IsNullOrEmpty(s) ||
                               Data.PropertyName.IndexOf(s, StringComparison.OrdinalIgnoreCase) > -1;

                    return false;
                };
            }

            if (this.GetTemplateChild("PART_ItemsControl") is ItemsControl PART_ItemsControl)
                PART_ItemsControl.GroupStyle.Add(new GroupStyle
                {
                    ContainerStyle = this.GroupContainerStyle
                });

            // Sorting
            if (CollectionViewSource.GetDefaultView(ItemsSource) is ListCollectionView View)
                View.SortDescriptions.Add(new SortDescription("Index", ListSortDirection.Ascending));
        }

        protected virtual void OnContentChanged()
            => RaiseEvent(new RoutedEventArgs(ContentChangedEvent, this));
        protected virtual void OnContentPropertyChanged(object sender, PropertyChangedEventArgs e)
            => ContentPropertyChanged?.Invoke(this, e);


        public void AddProperty(PropertyInfo Info)
            => AddProperty(Info, Info.GetCustomAttribute<EditorDisplayAttribute>(), Info.DeclaringType.GetCustomAttribute<EditorOptionAttribute>());
        private void AddProperty(PropertyInfo Info, EditorDisplayAttribute Display, EditorOptionAttribute Option)
        {
            if (Info.Name.Equals(Option?.NamePath))
                return;

            if ((Display?.Visible ?? true) &&
                !ItemsSource.Any(i => i.Property.Equals(Info)))
                ItemsSource.Add(new PropertyEditorData(Info, Display, Info.GetCustomAttribute<EditorValueAttribute>()));
        }

        public void RemoveProperty(PropertyInfo Info)
        {
            if (ItemsSource.FirstOrDefault(i => i.Property.Equals(Info)) is PropertyEditorData Item)
                ItemsSource.Remove(Item);
        }

        internal class PropertyEditorData
        {
            public PropertyInfo Property { get; }

            public EditorDisplayAttribute Display { get; }

            public EditorValueAttribute Option { get; }


            public TypeConverter Converter { get; }

            public bool CanConvertString { get; }

            public IPropertyEditorItemMenuProvider MenuProvider { get; }

            public string PropertyName => Property.Name;

            public int Index => Display?.Index ?? EditorDisplayAttribute.DefaultIndex;


            private readonly CategoryAttribute CategoryAttribute;
            public string GroupName => Display?.Category ?? CategoryAttribute?.Category ?? string.Empty;

            public PropertyEditorData(PropertyInfo Property, EditorDisplayAttribute Display, EditorValueAttribute Option)
            {
                this.Property = Property;
                this.Display = Display;
                this.Option = Option;

                CategoryAttribute = Property.GetCustomAttribute<CategoryAttribute>();

                MenuProvider = Display?.MenuProviderType?.GetInterface(nameof(IPropertyEditorItemMenuProvider)) is Type ProviderType ?
                               (IPropertyEditorItemMenuProvider)Activator.CreateInstance(ProviderType) :
                               PropertyEditorItemMenuProvider.Instance;

                Converter = TypeDescriptor.GetConverter(Property.PropertyType);
                Type StringType = typeof(string);
                CanConvertString = Converter.CanConvertFrom(StringType) && Converter.CanConvertTo(StringType);
            }

        }

    }
}
