using System;
using System.Windows.Markup;

namespace MenthaAssembly
{
    public class EnumExtension : MarkupExtension
    {
        public bool GetNames { get; set; }

        private readonly Type Type;
        public EnumExtension(Type Type)
        {
            this.Type = Type;
        }

        public override object ProvideValue(IServiceProvider Provider)
            => GetNames ? Enum.GetNames(Type) : Enum.GetValues(Type);

    }
}
