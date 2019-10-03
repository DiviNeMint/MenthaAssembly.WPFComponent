using System.Collections;
using System.Reflection;

namespace MenthaAssembly.Views.Editors.Comparers
{
    public class PropertyEditorComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x is PropertyInfo A &&
                y is PropertyInfo B)
            {
                int temp = (A.GetCustomAttribute<EditorDisplayAttribute>()?.Index ?? EditorDisplayAttribute.DefaultIndex)
                 .CompareTo(B.GetCustomAttribute<EditorDisplayAttribute>()?.Index ?? EditorDisplayAttribute.DefaultIndex);
                if (temp < 0)
                    return -1;
                if (temp > 0)
                    return 1;
                return A.Name.CompareTo(B.Name);
            }
            return 0;
        }
    }
}
