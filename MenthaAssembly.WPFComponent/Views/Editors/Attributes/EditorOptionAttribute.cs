using System;

namespace MenthaAssembly
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
    public sealed class EditorOptionAttribute : Attribute
    {
        public string NamePath { set; get; }

        public string TypeDisplay { set; get; }

        public string IconPath { set; get; }

        public bool EnableContentPropertyChangedEvent { set; get; }

    }
}
