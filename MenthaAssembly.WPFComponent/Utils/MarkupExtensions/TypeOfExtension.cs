using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MenthaAssembly.MarkupExtensions
{
    public class TypeOfExtension : MarkupExtension
    {
        private readonly Binding Base;

        public PropertyPath Path
        {
            get => Base.Path;
            set => Base.Path = value;
        }

        public string XPath
        {
            get => Base.XPath;
            set => Base.XPath = value;
        }

        public object Source
        {
            get => Base.Source;
            set => Base.Source = value;
        }

        public RelativeSource RelativeSource
        {
            get => Base.RelativeSource;
            set => Base.RelativeSource = value;
        }

        public string ElementName
        {
            get => Base.ElementName;
            set => Base.ElementName = value;
        }

        public IValueConverter Converter
        {
            get => Base.Converter;
            set => Base.Converter = value;
        }

        public object ConverterParameter
        {
            get => Base.ConverterParameter;
            set => Base.ConverterParameter = value;
        }

        public TypeOfExtension()
        {
            Base = new Binding();
        }
        public TypeOfExtension(string Path)
        {
            Base = new Binding(Path);
        }

        public override object ProvideValue(IServiceProvider Provider)
        {
            MultiBinding Multi = new();
            Multi.Bindings.Add(Base);
            Multi.Converter = TypeOfMultiConverter.Instance;

            return Multi.ProvideValue(Provider);
        }

        private class TypeOfMultiConverter : IMultiValueConverter
        {
            public static TypeOfMultiConverter Instance { get; } = new TypeOfMultiConverter();

            public object Convert(object[] Values, Type TargetType, object Parameter, CultureInfo culture)
                => Values.FirstOrDefault()?.GetType();

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
                => throw new NotSupportedException();

        }
    }
}