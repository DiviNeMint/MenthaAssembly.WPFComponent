using Microsoft.Win32;
using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace MenthaAssembly.Views
{
    public class PropertyEditorItem : ContentControl
    {
        private DependencyProperty BindingDependencyProperty { set; get; }
        private Binding BindingContent { set; get; }
        public static readonly DependencyProperty TargetObjectProperty =
            DependencyProperty.Register("TargetObject", typeof(object), typeof(PropertyEditorItem), new PropertyMetadata(default,
                (d, e) =>
                {
                    if (d is PropertyEditorItem This &&
                        This.DataContext is PropertyInfo Info &&
                        e.NewValue is object Target)
                    {
                        bool IsSameType = Target.Equals(e.OldValue?.GetType());
                        if (Target.GetType().Equals(e.OldValue?.GetType()) &&
                            This.Content is FrameworkElement Content &&
                            This.BindingDependencyProperty is DependencyProperty dp &&
                            This.BindingContent is Binding)
                        {
                            Binding bd = new Binding
                            {
                                Path = This.BindingContent.Path,
                                Source = This.TargetObject
                            };

                            Content.ClearValue(dp);
                            Content.SetBinding(dp, bd);
                            This.BindingContent = bd;
                            return;
                        }

                        Type PropertyType = typeof(ValueType).Equals(Info.PropertyType.BaseType) ? Info.PropertyType :
                                              typeof(object).Equals(Info.PropertyType.BaseType) ? Info.PropertyType :
                                              Info.PropertyType.BaseType ?? Info.PropertyType;
                        EditorDisplayAttribute DisplayInfo = Info.GetCustomAttribute<EditorDisplayAttribute>();
                        EditorValueAttribute ValueInfo = Info.GetCustomAttribute<EditorValueAttribute>();

                        #region PropertyName
                        if (This.GetTemplateChild("PART_TextBlock") is TextBlock PART_PropertyNameTextBlock)
                        {
                            if (!string.IsNullOrEmpty(DisplayInfo?.DisplayPath))
                                PART_PropertyNameTextBlock.SetBinding(TextBlock.TextProperty, LanguageBinding.Create(DisplayInfo.DisplayPath, Info.Name));
                            else
                                PART_PropertyNameTextBlock.Text = DisplayInfo?.Display ?? Info.Name;
                        }

                        #endregion
                        #region PropertyValue
                        if (DisplayInfo?.IsReadOnly ?? false)
                        {
                            TextBlock PART_ValueTextBlock = new TextBlock();
                            This.BindingDependencyProperty = TextBlock.TextProperty;
                            This.BindingContent = new Binding
                            {
                                Path = new PropertyPath(Info.Name),
                                Source = This.TargetObject
                            };
                            PART_ValueTextBlock.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                            PART_ValueTextBlock.Style = This.TryFindResource("PART_ValueTextBlockStyle") as Style;
                            This.Content = PART_ValueTextBlock;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(ValueInfo?.Source))
                            {
                                switch (PropertyType.Name)
                                {
                                    case nameof(Boolean):
                                        {
                                            CheckBox PART_CheckBox = new CheckBox();
                                            This.BindingDependencyProperty = CheckBox.IsCheckedProperty;
                                            This.BindingContent = new Binding
                                            {
                                                Path = new PropertyPath(Info.Name),
                                                Source = This.TargetObject
                                            };
                                            PART_CheckBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                                            PART_CheckBox.Style = This.TryFindResource("PART_CheckBoxStyle") as Style;
                                            This.Content = PART_CheckBox;
                                            break;
                                        }
                                    case nameof(Enum):
                                        {
                                            Array ItemsSource = Enum.GetValues(Info.PropertyType);
                                            ComboBox PART_ComboBox = new ComboBox
                                            {
                                                ItemsSource = ItemsSource,
                                                IsEditable = ItemsSource.Length > 5
                                            };
                                            This.BindingDependencyProperty = ComboBox.TextProperty;
                                            This.BindingContent = new Binding
                                            {
                                                Path = new PropertyPath(Info.Name),
                                                Source = This.TargetObject
                                            };
                                            PART_ComboBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                                            if (ValueInfo?.IsEnumLanguageBinding ?? false)
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
                                            break;
                                        }
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
                                        {
                                            TextBox PART_ValueTextBox = new TextBox();
                                            This.BindingDependencyProperty = TextBox.TextProperty;
                                            This.BindingContent = new Binding
                                            {
                                                Path = new PropertyPath(Info.Name),
                                                Source = This.TargetObject
                                            };
                                            PART_ValueTextBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
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
                                            PART_ValueTextBox.Style = This.TryFindResource("PART_ValueTextBoxStyle") as Style;
                                            This.Content = PART_ValueTextBox;
                                            break;
                                        }
                                    case nameof(Object):
                                    case nameof(String):
                                        {
                                            TextBox PART_TextBox = new TextBox();
                                            This.BindingDependencyProperty = TextBox.TextProperty;
                                            This.BindingContent = new Binding
                                            {
                                                Path = new PropertyPath(Info.Name),
                                                Source = This.TargetObject
                                            };
                                            PART_TextBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                                            PART_TextBox.Style = This.TryFindResource("PART_TextBoxStyle") as Style;
                                            This.Content = PART_TextBox;
                                            break;
                                        }
                                    default:
                                        {
                                            TextBox PART_TextBox = new TextBox
                                            {
                                                Text = PropertyType.FullName,
                                                IsReadOnly = true,
                                                IsReadOnlyCaretVisible = false,
                                            };
                                            PART_TextBox.Style = This.TryFindResource("PART_TextBoxStyle") as Style;
                                            This.Content = PART_TextBox;
                                            break;
                                        }
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
                                This.BindingDependencyProperty = ComboBox.SelectedItemProperty;
                                This.BindingContent = new Binding
                                {
                                    Path = new PropertyPath(Info.Name),
                                    Source = This.TargetObject
                                };
                                PART_ComboBox.SetBinding(This.BindingDependencyProperty, This.BindingContent);
                                PART_ComboBox.Style = This.TryFindResource("PART_ComboBoxStyle") as Style;
                                This.Content = PART_ComboBox;
                            }
                        }

                        #endregion
                        #region Menu
                        if (This.GetTemplateChild("PART_PopupMenu") is PopupMenu PART_PopupMenu)
                        {
                            if (This.GetTemplateChild("PART_ResetMenuItem") is MenuItem PART_ResetMenuItem)
                                PART_ResetMenuItem.Click += (s, arg) =>
                                {
                                    PART_PopupMenu.IsOpen = false;
                                    Info.SetValue(This.TargetObject, ValueInfo?.Default is null ?
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
                                                Info.SetValue(This.TargetObject, FileDialog.FileName);
                                            break;
                                        case ExploreType.Folder:
                                            FolderBrowserDialog FolderDialog = new FolderBrowserDialog();
                                            if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                                Info.SetValue(This.TargetObject, FolderDialog.SelectedPath);

                                            FolderDialog.Dispose();
                                            break;
                                    }

                                };
                            }
                        }

                        #endregion
                    }
                }));

        public object TargetObject
        {
            get => GetValue(TargetObjectProperty);
            set => SetValue(TargetObjectProperty, value);
        }

        public static readonly DependencyProperty PropertyMenuStyleProperty =
            DependencyProperty.Register("PropertyMenuStyle", typeof(Style), typeof(PropertyEditorItem), new PropertyMetadata(default));
        public Style PropertyMenuStyle
        {
            get => (Style)GetValue(PropertyMenuStyleProperty);
            set => SetValue(PropertyMenuStyleProperty, value);
        }

        static PropertyEditorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyEditorItem), new FrameworkPropertyMetadata(typeof(PropertyEditorItem)));
        }

    }
}
