using System.Collections.Generic;
using System.Reflection;

namespace MenthaAssembly.Views
{
    public interface IPropertyEditorItemMenuProvider
    {
        IEnumerable<PropertyEditorItemMenuData> CreateMenuDatas(PopupMenu Menu,
                                                                PropertyInfo Property,
                                                                EditorDisplayAttribute Display,
                                                                EditorValueAttribute Option);

    }
}
