using System;

namespace MenthaAssembly
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class EditorValueAttribute : Attribute
    {
        /// <summary>
        /// ComboBox ItemsSource Path ( Only Static )
        /// </summary>
        public string Source { set; get; }

        /// <summary>
        /// ComboBox DisplayMemberPath
        /// </summary>
        public string DisplayMemberPath { set; get; }

        /// <summary>
        /// ComboBox SelectedValuePath
        /// </summary>
        public string SelectedValuePath { get; set; }

        public bool IsEnumLanguageBinding { set; get; } = false;

        /// <summary>
        /// Binding StringFormat
        /// </summary>
        public string StringFormat { set; get; }

        /// <summary>
        /// Binding Converter's Type
        /// </summary>
        public Type ConverterType { set; get; }

        /// <summary>
        /// Binding ConverterParameter
        /// </summary>
        public object ConverterParameter { set; get; }

        public object Default { set; get; }

        public ExploreType ExploreType { set; get; }

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
