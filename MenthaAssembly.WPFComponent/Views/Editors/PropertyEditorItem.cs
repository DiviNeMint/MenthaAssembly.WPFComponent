using Microsoft.Win32;
using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

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

                        if (string.IsNullOrEmpty(ValueInfo?.Source))
                        {
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
                        }
                        else
                        {
                            ComboBox PART_ComboBox = new ComboBox
                            {
                                ItemsSource = ValueInfo.Source.ParseStaticObject() as IEnumerable,
                                DisplayMemberPath = ValueInfo.DisplayMemberPath,
                                SelectedValuePath = ValueInfo.SelectedValuePath
                            };
                            PART_ComboBox.SetBinding(ComboBox.SelectedItemProperty,
                                new Binding()
                                {
                                    Path = new PropertyPath(Info.Name),
                                    Source = Target
                                });
                            This.Content = PART_ComboBox;
                        }

                        #endregion

                        // Menu
                        if (This.GetTemplateChild("PART_PopupMenu") is PopupMenu PART_PopupMenu)
                        {
                            if (This.GetTemplateChild("PART_ResetMenuItem") is MenuItem PART_ResetMenuItem)
                                PART_ResetMenuItem.Click += (s, arg) =>
                                {
                                    PART_PopupMenu.IsOpen = false;
                                    Info.SetValue(Target, ValueInfo?.Default is null ?
                                                          (PropertyType.IsValueType ? Activator.CreateInstance(PropertyType) : null) :
                                                          Convert.ChangeType(ValueInfo.Default, PropertyType));
                                };
                            if (This.GetTemplateChild("PART_BrowseMenuItem") is MenuItem PART_BrowseMenuItem)
                            {
                                if (!ValueInfo?.ExploreType.Equals(ExploreType.None) ?? false)
                                    PART_BrowseMenuItem.IsEnabled = true;

                                PART_BrowseMenuItem.Click += (s, arg) =>
                                {
                                    PART_PopupMenu.IsOpen = false;
                                    switch (ValueInfo?.ExploreType)
                                    {
                                        case ExploreType.File:
                                            OpenFileDialog FileDialog = new OpenFileDialog();
                                            if (FileDialog.ShowDialog() is true)
                                                Info.SetValue(Target, FileDialog.FileName);
                                            break;
                                        case ExploreType.Folder:
                                            FolderBrowserDialog FolderDialog = new FolderBrowserDialog();
                                            if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                                Info.SetValue(Target, FolderDialog.SelectedPath);

                                            FolderDialog.Dispose();
                                            break;
                                    }

                                };
                            }
                        }
                    }
                }));
        public object TargetObject
        {
            get => GetValue(TargetObjectProperty);
            set => SetValue(TargetObjectProperty, value);
        }

        public static readonly DependencyProperty MenuStyleProperty =
            DependencyProperty.Register("MenuStyle", typeof(Style), typeof(PropertyEditorItem), new PropertyMetadata(default));
        public Style MenuStyle
        {
            get => (Style)GetValue(MenuStyleProperty);
            set => SetValue(MenuStyleProperty, value);
        }

        static PropertyEditorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorItem), new FrameworkPropertyMetadata(typeof(PropertyEditorItem)));
        }
    }
}
