using System.Reflection;

namespace System.Windows
{
    public static class DependencyHelper
    {
        public static DependencyProperty GetDependencyPropertyByName(this DependencyObject This, string PropertyName)
        {
            if (This.GetType()?
                    .GetField($"{PropertyName}Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is FieldInfo FieldInfo)
                return FieldInfo.GetValue(null) as DependencyProperty;

            return null;
        }
    }
}
