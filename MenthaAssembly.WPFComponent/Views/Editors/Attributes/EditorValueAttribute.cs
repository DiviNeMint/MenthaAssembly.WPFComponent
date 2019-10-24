using System;

namespace MenthaAssembly
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class EditorValueAttribute : Attribute
    {
        public object Default { set; get; }

        /// <summary>
        /// MouseWheel、KeyUp、KeyDown
        /// </summary>
        public object Delta { get; set; }

        /// <summary>
        /// Ctrl + MouseWheel、KeyUp、KeyDown
        /// </summary>
        public object CombineDelta { get; set; }

        public object Maximum { set; get; }

        public object Minimum { get; set; }

    }
}
