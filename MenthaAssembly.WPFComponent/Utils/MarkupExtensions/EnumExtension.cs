using System;
using System.Windows.Markup;

namespace MenthaAssembly.MarkupExtensions
{
    public class EnumExtension(Type Type) : MarkupExtension
    {
        public bool GetNames { get; set; }

        public override object ProvideValue(IServiceProvider Provider)
            => GetNames ? Enum.GetNames(Type) : Enum.GetValues(Type);

    }
}