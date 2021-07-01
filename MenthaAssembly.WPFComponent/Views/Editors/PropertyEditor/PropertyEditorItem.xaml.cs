using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static MenthaAssembly.Views.PropertyEditor;

namespace MenthaAssembly.Views
{
    public class PropertyEditorItem : ContentControl
    {
        public static readonly DependencyProperty PropertyNameProperty =
              DependencyProperty.Register("PropertyName", typeof(string), typeof(PropertyEditorItem), new PropertyMetadata("Property"));
        public string PropertyName
        {
            get => (string)GetValue(PropertyNameProperty);
            set => SetValue(PropertyNameProperty, value);
        }

        public static readonly DependencyProperty EnableMenuProperty =
              DependencyProperty.Register("EnableMenu", typeof(bool), typeof(PropertyEditorItem), new PropertyMetadata(true));
        public bool EnableMenu
        {
            get => (bool)GetValue(EnableMenuProperty);
            set => SetValue(EnableMenuProperty, value);
        }

        public static readonly DependencyProperty MenuStyleProperty =
            DependencyProperty.Register("MenuStyle", typeof(Style), typeof(PropertyEditorItem), new PropertyMetadata(default));
        public Style MenuStyle
        {
            get => (Style)GetValue(MenuStyleProperty);
            set => SetValue(MenuStyleProperty, value);
        }

        public static readonly DependencyProperty MenuItemStyleProperty =
            DependencyProperty.Register("MenuItemStyle", typeof(Style), typeof(PropertyEditorItem), new PropertyMetadata(default));
        public Style MenuItemStyle
        {
            get => (Style)GetValue(MenuItemStyleProperty);
            set => SetValue(MenuItemStyleProperty, value);
        }

        public static readonly DependencyProperty MenuItemsSourceProperty =
              DependencyProperty.Register("MenuItemsSource", typeof(ICollection), typeof(PropertyEditorItem), new PropertyMetadata(null));
        public ICollection MenuItemsSource
        {
            get => (ICollection)GetValue(MenuItemsSourceProperty);
            set => SetValue(MenuItemsSourceProperty, value);
        }

        private DependencyProperty BindingDependencyProperty;
        private Binding BindingContent;
        public static readonly DependencyProperty TargetObjectProperty =
              DependencyProperty.Register("TargetObject", typeof(object), typeof(PropertyEditorItem), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is PropertyEditorItem This &&
                          This.DataContext is PropertyEditorData Data &&
                          e.NewValue is object Target)
                      {
                          if (Target.GetType().Equals(e.OldValue?.GetType()) &&
                              This.Content is FrameworkElement Content &&
                              This.BindingDependencyProperty is DependencyProperty dp &&
                              This.BindingContent is Binding)
                          {
                              Binding bd = new Binding
                              {
                                  Path = This.BindingContent.Path,
                                  Source = This.TargetObject,
                                  Mode = This.BindingContent.Mode,
                                  Converter = This.BindingContent.Converter,
                                  ConverterParameter = This.BindingContent.ConverterParameter
                              };

                              Content.ClearValue(dp);
                              Content.SetBinding(dp, bd);
                              This.BindingContent = bd;
                              return;
                          }

                          #region PropertyName
                          if (string.IsNullOrEmpty(Data.Display?.DisplayPath))
                              This.PropertyName = Data.Display?.Display ?? Data.Property.Name;
                          else
                              This.SetBinding(PropertyNameProperty, LanguageBinding.Create(Data.Display.DisplayPath, Data.Property.Name));

                          #endregion
                          #region PropertyValue
                          if (Data.Display?.IsReadOnly ?? false)
                          {
                              TextBlock PART_ValueTextBlock = new TextBlock();
                              This.BindingDependencyProperty = TextBlock.TextProperty;
                              This.BindingContent = new Binding
                              {
                                  Path = new PropertyPath(Data.Property.Name),
                                  Source = This.TargetObject
                              };
                              PART_ValueTextBlock.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                              PART_ValueTextBlock.Style = This.TryFindResource("PART_ValueTextBlockStyle") as Style;
                              This.Content = PART_ValueTextBlock;
                          }
                          else if (string.IsNullOrEmpty(Data.Option?.Source))
                          {
                              if (Data.Converter is BooleanConverter ||
                                  Data.Converter is NullableBoolConverter)
                              {
                                  CheckBox PART_CheckBox = new CheckBox();
                                  This.BindingDependencyProperty = CheckBox.IsCheckedProperty;
                                  This.BindingContent = new Binding
                                  {
                                      Path = new PropertyPath(Data.Property.Name),
                                      Source = This.TargetObject
                                  };
                                  PART_CheckBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                                  PART_CheckBox.Style = This.TryFindResource("PART_CheckBoxStyle") as Style;
                                  This.Content = PART_CheckBox;
                              }
                              else if (Data.Converter is ColorConverter || Data.Converter is BrushConverter)
                              {
                                  ColorBox PART_ColorBox = new ColorBox();
                                  This.BindingDependencyProperty = ColorBox.ColorProperty;
                                  This.BindingContent = new Binding
                                  {
                                      Path = new PropertyPath(Data.Property.Name),
                                      Source = This.TargetObject,
                                      Mode = BindingMode.TwoWay,
                                      Converter = PropertyValueConverter.Instance,
                                      ConverterParameter = Data.Converter
                                  };

                                  PART_ColorBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                                  PART_ColorBox.Style = This.TryFindResource("PART_ColorBoxStyle") as Style;
                                  This.Content = PART_ColorBox;
                              }
                              else if (Data.Converter is EnumConverter)
                              {
                                  Array ItemsSource = Enum.GetValues(Data.Property.PropertyType);
                                  ComboBox PART_ComboBox = new ComboBox
                                  {
                                      ItemsSource = ItemsSource,
                                      IsEditable = ItemsSource.Length > 5
                                  };
                                  This.BindingDependencyProperty = ComboBox.TextProperty;
                                  This.BindingContent = new Binding
                                  {
                                      Path = new PropertyPath(Data.Property.Name),
                                      Source = This.TargetObject
                                  };
                                  PART_ComboBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                                  if (Data.Option?.IsEnumLanguageBinding ?? false)
                                  {
                                      FrameworkElementFactory TemplateTextBlock = new FrameworkElementFactory(typeof(TextBlock));
                                      TemplateTextBlock.SetValue(TextBlock.TextProperty, new LanguageBinding());

                                      PART_ComboBox.ItemTemplate = new DataTemplate
                                      {
                                          VisualTree = TemplateTextBlock
                                      };
                                  }
                                  PART_ComboBox.Style = This.TryFindResource("PART_ComboBoxStyle") as Style;
                                  This.Content = PART_ComboBox;
                              }
                              else if (Data.Converter is SByteConverter || Data.Converter is ByteConverter ||
                                       Data.Converter is Int16Converter || Data.Converter is Int32Converter || Data.Converter is Int64Converter ||
                                       Data.Converter is UInt16Converter || Data.Converter is UInt32Converter || Data.Converter is UInt64Converter ||
                                       Data.Converter is SingleConverter || Data.Converter is DoubleConverter || Data.Converter is DecimalConverter)
                              {
                                  TextBox PART_ValueTextBox = new TextBox();
                                  This.BindingDependencyProperty = TextBox.TextProperty;
                                  This.BindingContent = new Binding
                                  {
                                      Path = new PropertyPath(Data.Property.Name),
                                      Source = This.TargetObject
                                  };
                                  PART_ValueTextBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                                  TextBoxEx.SetValueType(PART_ValueTextBox, Data.Property.PropertyType);
                                  if (Data.Option != null)
                                  {
                                      if (Data.Option.Delta != null)
                                          TextBoxEx.SetDelta(PART_ValueTextBox, Data.Option.Delta);
                                      if (Data.Option.CombineDelta != null)
                                          TextBoxEx.SetCombineDelta(PART_ValueTextBox, Data.Option.CombineDelta);
                                      if (Data.Option.Minimum != null)
                                          TextBoxEx.SetMinimum(PART_ValueTextBox, Data.Option.Minimum);
                                      if (Data.Option.Maximum != null)
                                          TextBoxEx.SetMaximum(PART_ValueTextBox, Data.Option.Maximum);
                                  }
                                  PART_ValueTextBox.Style = This.TryFindResource("PART_ValueTextBoxStyle") as Style;
                                  This.Content = PART_ValueTextBox;
                              }
                              else
                              {
                                  TextBox PART_TextBox = new TextBox();
                                  if (Data.Converter is CharConverter)
                                      PART_TextBox.MaxLength = 1;

                                  if (Data.CanConvertString)
                                  {
                                      This.BindingDependencyProperty = TextBox.TextProperty;
                                      This.BindingContent = new Binding
                                      {
                                          Path = new PropertyPath(Data.Property.Name),
                                          Source = This.TargetObject,

                                      };
                                      PART_TextBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                                      PART_TextBox.PreviewKeyDown += TextBoxEx.UpdateTextBindingWhenEnterKeyDown;
                                  }
                                  else
                                  {
                                      PART_TextBox.Text = Data.Property.GetValue(Target)?.ToString() ?? Data.Property.PropertyType.FullName;
                                      PART_TextBox.IsReadOnly = true;
                                      PART_TextBox.IsReadOnlyCaretVisible = false;
                                  }

                                  PART_TextBox.Style = This.TryFindResource("PART_TextBoxStyle") as Style;
                                  This.Content = PART_TextBox;
                              }
                          }
                          else
                          {
                              ComboBox PART_ComboBox = new ComboBox
                              {
                                  ItemsSource = Data.Option.Source.ParseStaticObject() as IEnumerable,
                                  DisplayMemberPath = Data.Option.DisplayMemberPath,
                                  SelectedValuePath = Data.Option.SelectedValuePath
                              };
                              This.BindingDependencyProperty = ComboBox.SelectedItemProperty;
                              This.BindingContent = new Binding
                              {
                                  Path = new PropertyPath(Data.Property.Name),
                                  Source = This.TargetObject
                              };
                              PART_ComboBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                              PART_ComboBox.Style = This.TryFindResource("PART_ComboBoxStyle") as Style;
                              This.Content = PART_ComboBox;
                          }

                          #endregion

                      }
                  }));
        public object TargetObject
        {
            get => GetValue(TargetObjectProperty);
            set => SetValue(TargetObjectProperty, value);
        }

        static PropertyEditorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorItem), new FrameworkPropertyMetadata(typeof(PropertyEditorItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Menu
            if (this.GetTemplateChild("PART_PopupMenu") is PopupMenu PART_PopupMenu)
                PART_PopupMenu.Opened += OnMenuOpened;
        }

        private void OnMenuOpened(object sender, RoutedEventArgs e)
        {
            if (this.MenuItemsSource is null &&
                sender is PopupMenu PART_PopupMenu &&
                this.DataContext is PropertyEditorData Data)
                this.MenuItemsSource = Data.MenuProvider.CreateMenuDatas(PART_PopupMenu, Data.Property, Data.Display, Data.Option).ToList();
        }

        private class PropertyValueConverter : IValueConverter
        {
            public static PropertyValueConverter Instance { get; } = new PropertyValueConverter();

            private static ColorConverter _ColorConverter = null;
            private static ColorConverter ColorConverter
            {
                get
                {
                    if (_ColorConverter is null)
                        _ColorConverter = new ColorConverter();

                    return _ColorConverter;
                }
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (parameter is TypeConverter Converter)
                {
                    if (parameter is ColorConverter ||
                        parameter is BrushConverter)
                        return value is null ? null : ColorConverter.ConvertFromString(Converter.ConvertToString(value));
                }

                return Binding.DoNothing;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (parameter is TypeConverter Converter)
                {
                    if (parameter is ColorConverter ||
                        parameter is BrushConverter)
                        return value is null ? null : Converter.ConvertFromString(ColorConverter.ConvertToString(value));
                }
                return Binding.DoNothing;
            }
        }

    }
}
