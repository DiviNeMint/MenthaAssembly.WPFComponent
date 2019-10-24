using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace MenthaAssembly.Views
{
    public class PropertyEditorItem : ContentControl
    {
        public static readonly DependencyProperty TargetObjectProperty =
            DependencyProperty.Register("TargetObject", typeof(object), typeof(PropertyEditorItem), new PropertyMetadata(default,
                (d, e) =>
                {
                    if (d is PropertyEditorItem This &&
                        This.DataContext is PropertyInfo Info &&
                        e.NewValue is object Target)
                    {
                        #region PropertyName
                        EditorDisplayAttribute DisplayInfo = Info.GetCustomAttribute<EditorDisplayAttribute>();
                        if (This.GetTemplateChild("PART_TextBlock") is TextBlock PART_PropertyNameTextBlock)
                        {
                            if (!string.IsNullOrEmpty(DisplayInfo?.DisplayPath))
                            {
                                PART_PropertyNameTextBlock.SetBinding(TextBlock.TextProperty,
                                    new Binding()
                                    {
                                        Path = new PropertyPath($"Data.{DisplayInfo.DisplayPath}"),
                                        Source = LanguageManager.Current,
                                        FallbackValue = Info.Name
                                    });
                            }
                            else
                            {
                                PART_PropertyNameTextBlock.Text = DisplayInfo?.Display ?? Info.Name;
                            }
                        }

                        #endregion

                        #region PropertyValue
                        Type PropertyType = typeof(ValueType).Equals(Info.PropertyType.BaseType) ? Info.PropertyType :
                                                  typeof(object).Equals(Info.PropertyType.BaseType) ? Info.PropertyType :
                                                  Info.PropertyType.BaseType ?? Info.PropertyType;
                        EditorValueAttribute ValueInfo = Info.GetCustomAttribute<EditorValueAttribute>();
                        switch (PropertyType.Name)
                        {
                            case nameof(Boolean):
                                CheckBox PART_CheckBox = new CheckBox();
                                PART_CheckBox.SetBinding(CheckBox.IsCheckedProperty,
                                    new Binding()
                                    {
                                        Path = new PropertyPath(Info.Name),
                                        Source = Target
                                    });
                                This.Content = PART_CheckBox;
                                break;
                            case nameof(Enum):
                                Array ItemsSource = Enum.GetValues(Info.PropertyType);
                                ComboBox PART_ComboBox = new ComboBox
                                {
                                    ItemsSource = ItemsSource,
                                    IsEditable = ItemsSource.Length > 5
                                };
                                PART_ComboBox.SetBinding(ComboBox.TextProperty,
                                    new Binding()
                                    {
                                        Path = new PropertyPath(Info.Name),
                                        Source = Target
                                    });
                                This.Content = PART_ComboBox;
                                break;
                            case nameof(SByte):
                            case nameof(Int16):
                            case nameof(Int32):
                            case nameof(Int64):
                            case nameof(Byte):
                            case nameof(UInt16):
                            case nameof(UInt32):
                            case nameof(UInt64):
                            case nameof(Decimal):
                            case nameof(Single):
                            case nameof(Double):
                                TextBox PART_ValueTextBox = new TextBox();
                                PART_ValueTextBox.SetBinding(TextBox.TextProperty,
                                    new Binding()
                                    {
                                        Path = new PropertyPath(Info.Name),
                                        Source = Target
                                    });
                                TextBoxHelper.SetValueType(PART_ValueTextBox, PropertyType);
                                if (ValueInfo != null)
                                {
                                    if (ValueInfo.Delta != null)
                                        TextBoxHelper.SetDelta(PART_ValueTextBox, ValueInfo.Delta);
                                    if (ValueInfo.CombineDelta != null)
                                        TextBoxHelper.SetCombineDelta(PART_ValueTextBox, ValueInfo.CombineDelta);
                                    if (ValueInfo.Minimum != null)
                                        TextBoxHelper.SetMinimum(PART_ValueTextBox, ValueInfo.Minimum);
                                    if (ValueInfo.Maximum != null)
                                        TextBoxHelper.SetMaximum(PART_ValueTextBox, ValueInfo.Maximum);
                                }
                                This.Content = PART_ValueTextBox;
                                break;
                            case nameof(Object):
                            case nameof(String):
                                TextBox PART_TextBox = new TextBox();
                                PART_TextBox.SetBinding(TextBox.TextProperty,
                                    new Binding()
                                    {
                                        Path = new PropertyPath(Info.Name),
                                        Source = Target
                                    });
                                This.Content = PART_TextBox;
                                break;
                            default:
                                This.Content = new TextBox
                                {
                                    Text = PropertyType.FullName,
                                    IsReadOnly = true,
                                    IsReadOnlyCaretVisible = false,
                                };
                                break;
                        }

                        #endregion

                        // Menu
                        if (This.GetTemplateChild("PART_MenuPopup") is Popup PART_MenuPopup &&
                            This.GetTemplateChild("PART_MenuItemsControl") is ItemsControl PART_MenuItemsControl &&
                            PART_MenuItemsControl.Items is ItemCollection Menu)
                        {
                            MenuItem ResetItem = new MenuItem { Header = "Reset" };
                            ResetItem.Click += (s, arg) =>
                            {
                                Info.SetValue(Target, ValueInfo.Default is null ?
                                                      (PropertyType.IsValueType ? Activator.CreateInstance(PropertyType) : null) :
                                                      Convert.ChangeType(ValueInfo.Default, PropertyType));
                                PART_MenuPopup.IsOpen = false;
                            };
                            Menu.Add(ResetItem);
                        }
                    }
                }));
        public object TargetObject
        {
            get => GetValue(TargetObjectProperty);
            set => SetValue(TargetObjectProperty, value);
        }

        public static readonly DependencyProperty MenuTemplateProperty =
            DependencyProperty.Register("MenuTemplate", typeof(ControlTemplate), typeof(PropertyEditorItem), new PropertyMetadata(default));
        public ControlTemplate MenuTemplate
        {
            get => (ControlTemplate)GetValue(MenuTemplateProperty);
            set => SetValue(MenuTemplateProperty, value);
        }

        static PropertyEditorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorItem), new FrameworkPropertyMetadata(typeof(PropertyEditorItem)));
        }

    }
}
