using System;
using System.Windows.Markup;

namespace MenthaAssembly
{
    public class EnumExtension : MarkupExtension
    {
        public bool DisplayNames { get; set; }

        private readonly Type Type;
        public EnumExtension(Type Type)
        {
            this.Type = Type;
        }

        public override object ProvideValue(IServiceProvider Provider)
            => DisplayNames ? Enum.GetNames(Type) : Enum.GetValues(Type);

    }
}
