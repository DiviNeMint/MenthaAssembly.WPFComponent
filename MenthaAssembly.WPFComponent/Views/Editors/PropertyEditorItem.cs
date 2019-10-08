using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MenthaAssembly.Views
{
    public class PropertyEditorItem : ContentControl
    {
        public static readonly DependencyProperty TargeObjectProperty =
            DependencyProperty.Register("TargeObject", typeof(object), typeof(PropertyEditorItem), new PropertyMetadata(default,
                (d, e) =>
                {
                    if (d is PropertyEditorItem This &&
                        This.DataContext is PropertyInfo Info &&
                        e.NewValue is object Targe)
                    {
                        #region PropertyName
                        EditorDisplayAttribute DisplayDatas = Info.GetCustomAttribute<EditorDisplayAttribute>();
                        if (This.GetTemplateChild("PART_TextBlock") is TextBlock PART_PropertyNameTextBlock)
                        {
                            if (!string.IsNullOrEmpty(DisplayDatas?.DisplayPath))
                            {
                                PART_PropertyNameTextBlock.SetBinding(TextBlock.TextProperty,
                                    new Binding()
                                    {
                                        Path = new PropertyPath($"Data.{DisplayDatas.DisplayPath}"),
                                        Source = LanguageManager.Current,
                                        FallbackValue = Info.Name
                                    });
                            }
                            else
                            {
                                PART_PropertyNameTextBlock.Text = DisplayDatas?.Display ?? Info.Name;
                            }
                        }

                        #endregion

                        #region PropertyValue
                        Type PropertyType = typeof(ValueType).Equals(Info.PropertyType.BaseType) ? Info.PropertyType :
                                                  typeof(object).Equals(Info.PropertyType.BaseType) ? Info.PropertyType :
                                                  Info.PropertyType.BaseType ?? Info.PropertyType;
                        switch (PropertyType.Name)
                        {
                            case nameof(Boolean):
                                CheckBox PART_CheckBox = new CheckBox();
                                PART_CheckBox.SetBinding(CheckBox.IsCheckedProperty,
                                    new Binding()
                                    {
                                        Path = new PropertyPath(Info.Name),
                                        Source = Targe
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
                                        Source = Targe
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
                                        Source = Targe
                                    });
                                TextBoxHelper.SetValueType(PART_ValueTextBox, PropertyType);
                                if (Info.GetCustomAttribute<EditorValueAttribute>() is EditorValueAttribute ValueDatas)
                                {
                                    if (ValueDatas.Delta != null)
                                        TextBoxHelper.SetDelta(PART_ValueTextBox, ValueDatas.Delta);
                                    if (ValueDatas.CombineDelta != null)
                                        TextBoxHelper.SetCombineDelta(PART_ValueTextBox, ValueDatas.CombineDelta);
                                    if (ValueDatas.Minimum != null)
                                        TextBoxHelper.SetMinimum(PART_ValueTextBox, ValueDatas.Minimum);
                                    if (ValueDatas.Maximum != null)
                                        TextBoxHelper.SetMaximum(PART_ValueTextBox, ValueDatas.Maximum);
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
                                        Source = Targe
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

                    }
                }));
        public object TargeObject
        {
            get => GetValue(TargeObjectProperty);
            set => SetValue(TargeObjectProperty, value);
        }

        static PropertyEditorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorItem), new FrameworkPropertyMetadata(typeof(PropertyEditorItem)));
        }
    }
}
