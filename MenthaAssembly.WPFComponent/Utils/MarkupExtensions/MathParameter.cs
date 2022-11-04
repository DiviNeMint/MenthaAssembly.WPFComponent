using System.Windows.Data;

namespace MenthaAssembly.MarkupExtensions
{
    public sealed class MathParameter
    {
        public string Name { get; set; }

        public BindingBase Value { get; set; }

        public MathParameter()
        {
        }
        public MathParameter(string Name, BindingBase Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

    }
}