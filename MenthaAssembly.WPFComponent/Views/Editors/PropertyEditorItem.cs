using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                        string PropertyTypeName = Info.PropertyType.Name.Equals(nameof(Boolean)) ? nameof(Boolean) : Info.PropertyType.BaseType?.Name;
                        switch (PropertyTypeName)
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
                            default:
                                TextBox PART_TextBox = new TextBox();
                                PART_TextBox.SetBinding(TextBox.TextProperty,
                                    new Binding()
                                    {
                                        Path = new PropertyPath(Info.Name),
                                        Source = Targe
                                    });
                                This.Content = PART_TextBox;

                                //if (TextBox is ValueTextBox ValueTextBox)
                                //{
                                //    EditorOptionAttribute PropertyOption = Info.GetCustomAttribute<EditorOptionAttribute>() ?? new EditorOptionAttribute();
                                //    ValueTextBox.Delta = PropertyOption.ValueDelta;
                                //    ValueTextBox.CtrlDelta = PropertyOption.ValueCtrlDelta;
                                //    ValueTextBox.Maximum = PropertyOption.ValueMaximum;
                                //    ValueTextBox.Minimum = PropertyOption.ValueMinimum;
                                //}
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
