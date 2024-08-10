using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class PropertyEditorItemMenuProvider : IPropertyEditorItemMenuProvider
    {
        private static readonly ResourceDictionary IconResources = new ResourceDictionary
        {
            Source = new Uri("/MenthaAssembly.WPFComponent;component/Themes/Generic.xaml", UriKind.Relative)
        };

        public static PropertyEditorItemMenuProvider Instance { get; } = new PropertyEditorItemMenuProvider();

        /// <summary>
        /// CreateMenuDatas<para></para>
        /// return Empty(Null) would add separator.
        /// </summary>
        /// <param name="Menu"></param>
        /// <param name="Property"></param>
        /// <param name="Option"></param>
        /// <returns></returns>
        public virtual IEnumerable<PropertyEditorItemMenuData> CreateMenuDatas(PopupMenu Menu, PropertyInfo Property, EditorDisplayAttribute Display, EditorValueAttribute Option)
        {
            yield return new PropertyEditorItemMenuData
            {
                Icon = IconResources["Icon_Eraser"] as ImageSource,
                IsEnabled = !Display?.IsReadOnly ?? true,
                Header = "Reset",
                Handler = (Target) =>
                {
                    Menu.IsOpen = false;
                    Property.SetValue(Target, Option?.Default is null ?
                                      (Property.PropertyType.IsValueType ? Activator.CreateInstance(Property.PropertyType) : null) :
                                      Convert.ChangeType(Option.Default, Property.PropertyType));
                }
            };

            yield return new PropertyEditorItemMenuData
            {
                Icon = IconResources["Icon_Explore"] as ImageSource,
                Header = "Browse",
                IsEnabled = (!Display?.IsReadOnly ?? true) && (!Option?.ExploreType.Equals(ExploreType.None) ?? false),
                Handler = (Target) =>
                {
                    Menu.IsOpen = false;
                    switch (Option?.ExploreType)
                    {
                        case ExploreType.File:
                            {
                                OpenFileDialog FileDialog = new OpenFileDialog();
                                if (FileDialog.ShowDialog() is true)
                                    Property.SetValue(Target, FileDialog.FileName);

                                break;
                            }
                        case ExploreType.Folder:
                            {
#if NET8_0_OR_GREATER
                                OpenFolderDialog FolderDialog = new();
                                if (FolderDialog.ShowDialog() is true)
                                    Property.SetValue(Target, FolderDialog.FolderName);
#else
                                FolderBrowserDialog FolderDialog = new();
                                if (FolderDialog.ShowDialog() is true)
                                    Property.SetValue(Target, FolderDialog.SelectedPath);
#endif
                                break;
                            }
                    }
                }
            };
        }

    }
}
