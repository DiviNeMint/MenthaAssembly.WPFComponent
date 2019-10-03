using System;

namespace MenthaAssembly
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class EditorDisplayAttribute : Attribute
    {
        public const int DefaultIndex = byte.MaxValue;

        public string Category { set; get; }

        public string Display { set; get; }

        public string DisplayPath { set; get; }

        public int Index { set; get; } = DefaultIndex;

        public bool Visible { set; get; } = true;

    }
}
