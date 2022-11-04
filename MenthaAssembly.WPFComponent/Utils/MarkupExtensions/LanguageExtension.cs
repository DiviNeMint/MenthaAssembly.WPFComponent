using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MenthaAssembly.MarkupExtensions
{
    public class LanguageExtension : MarkupExtension
    {
        internal string Path { get; }

        public string Default { set; get; }

        public BindingBase Source { set; get; }

        public LanguageExtension()
        {
        }
        public LanguageExtension(string Path)
        {
            this.Path = Path;
        }

        public override object ProvideValue(IServiceProvider Provider)
        {
            if (Source != null)
            {
                Binding Base = Create(null, Default);
                MultiBinding Multi = new MultiBinding
                {
                    Mode = BindingMode.OneWay,
                    Converter = LanguageMultiConverter.Instance,
                    ConverterParameter = Default
                };

                Multi.Bindings.Add(Base);
                Multi.Bindings.Add(Source);

                return Multi.ProvideValue(Provider);
            }

            return Create(Path, Default).ProvideValue(Provider);
        }

        private static readonly PropertyInfo LanguageCurrentInfo = typeof(LanguageManager).GetProperty(nameof(LanguageManager.Current));
        public static Binding Create(string Path, string Default)
        {
            string Temp = string.IsNullOrEmpty(Default) ? Path : Default;
            return new Binding
            {
                Path = new PropertyPath($"(0)[{Path}]", LanguageCurrentInfo),
                TargetNullValue = Temp,
                FallbackValue = Temp
            };
        }

        private class LanguageMultiConverter : IMultiValueConverter
        {
            public static LanguageMultiConverter Instance { get; } = new LanguageMultiConverter();

            public object Convert(object[] Values, Type TargetType, object Parameter, CultureInfo culture)
            {
                string Path = Values[1]?.ToString();
                return string.IsNullOrEmpty(Path) ? Parameter ?? Binding.DoNothing : LanguageManager.Current?[Path] ?? Parameter ?? Path;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
                => throw new NotSupportedException();

        }

    }
}
