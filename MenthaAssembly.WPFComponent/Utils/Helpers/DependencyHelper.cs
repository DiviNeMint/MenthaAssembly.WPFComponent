using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace System.Windows
{
    public static class DependencyHelper
    {
        //public static DependencyProperty GetDependencyProperty(this DependencyObject This, string PropertyName)
        //{
        //    if (This.GetType()?
        //            .GetField($"{PropertyName}Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is FieldInfo FieldInfo)
        //        return FieldInfo.GetValue(null) as DependencyProperty;

        //    return null;
        //}

        public static DependencyProperty GetDependencyProperty(this DependencyObject This, string PropertyName)
        {
            if (This.GetType() is Type ThisType)
                return DependencyPropertyDescriptor.FromName(PropertyName, ThisType, ThisType)?.DependencyProperty;

            return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject This) where T : DependencyObject
        {
            if (This != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(This); i++)
                {
                    DependencyObject Child = VisualTreeHelper.GetChild(This, i);
                    if (Child is T)
                        yield return (T)Child;

                    foreach (T ChildOfChild in FindVisualChildren<T>(Child))
                        yield return ChildOfChild;
                }
            }
        }

    }
}
