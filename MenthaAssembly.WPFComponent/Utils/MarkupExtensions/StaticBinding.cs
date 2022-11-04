using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace MenthaAssembly.MarkupExtensions
{
    public class StaticBinding : MarkupExtension
    {
        public IValueConverter Converter { get; set; }
        public object ConverterParameter { get; set; }

        private readonly string Path;
        public StaticBinding(string Path)
        {
            this.Path = Path;
        }

        //var a = Provider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
        //var b = a.Resolve("Views:TreeView");

        public override object ProvideValue(IServiceProvider Provider)
            => Create(Path, Converter, ConverterParameter).ProvideValue(Provider);

        public static Binding Create(string Path, IValueConverter Converter = null, object ConverterParameter = null)
        {
            string[] Paths = Path.Split('.');
            if (Path.Length < 2)
                return null;

            if (Paths.Length.Equals(2))
                return new Binding($"({Path})")
                {
                    Converter = Converter,
                    ConverterParameter = ConverterParameter
                };

            return new Binding($"({string.Join(".", Paths, 0, 2)}).{string.Join(".", Paths, 2, Paths.Length - 2)}")
            {
                Converter = Converter,
                ConverterParameter = ConverterParameter
            };
        }
    }
}
