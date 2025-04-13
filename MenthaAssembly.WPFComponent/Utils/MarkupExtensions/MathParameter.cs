using System;
using System.Windows.Data;

namespace MenthaAssembly.MarkupExtensions
{
    public sealed class MathParameter
    {
        public string Name { get; set; }

        public Type Type { set; get; }

        public BindingBase Value { get; set; }

        public MathParameter()
        {
        }
        public MathParameter(string Name, Type Type, BindingBase Value)
        {
            this.Name = Name;
            this.Type = Type;
            this.Value = Value;
        }

    }
}