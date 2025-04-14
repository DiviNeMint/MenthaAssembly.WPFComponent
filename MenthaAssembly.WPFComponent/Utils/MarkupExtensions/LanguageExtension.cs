using MenthaAssembly.Globalization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MenthaAssembly.MarkupExtensions
{
    public class LanguageExtension : MarkupExtension
    {
        private const string DefaultLoadingText = "...";

        private string Path { get; }

        public string Default { set; get; }

        public BindingBase Source { set; get; }

        public string LoadingText { get; set; } = DefaultLoadingText;

        public LanguageExtension()
        {
        }
        public LanguageExtension(string Path)
        {
            this.Path = Path;
        }

        public override object ProvideValue(IServiceProvider Provider)
        {
            // If TargetObject is a Shared DependencyObject (for example, in a DataTemplate or Style)
            return Provider?.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget ValueTarget ||
                ValueTarget.TargetObject is not DependencyObject
                ? this
                : Create(this).ProvideValue(Provider);
        }

        private static readonly Dictionary<string, LanguageAdapter> KeyAdapters = [];
        private static readonly List<LanguageAdapter> SourceAdapters = [];

        public static MultiBinding Create(string Path, string Default)
            => Create(Path, Default, DefaultLoadingText);
        public static MultiBinding Create(string Path, string Default, string LoadingText)
            => Create(new LanguageExtension(Path) { Default = Default, LoadingText = LoadingText });
        public static MultiBinding Create(BindingBase Source)
            => Create(Source, DefaultLoadingText);
        public static MultiBinding Create(BindingBase Source, string LoadingText)
            => Create(new LanguageExtension() { Source = Source, LoadingText = LoadingText });
        private static MultiBinding Create(LanguageExtension Language)
        {
            LanguageAdapter Adapter;
            if (string.IsNullOrEmpty(Language.Path))
            {
                Adapter = new(Language);
                Adapter.Refresh();

                SourceAdapters.Add(Adapter);
            }
            else if (!KeyAdapters.TryGetValue(Language.Path, out Adapter))
            {
                Adapter = new(Language);
                Adapter.Refresh();

                KeyAdapters[Language.Path] = Adapter;
            }

            MultiBinding Multi = new()
            {
                Mode = BindingMode.OneWay,
                Converter = LanguageMultiConverter.Instance,
                ConverterParameter = DesignerProperties.GetIsInDesignMode(Adapter) ? string.IsNullOrEmpty(Language.Default) ? Language.Path : Language.Default : Adapter
            };
            Multi.Bindings.Add(new Binding(nameof(LanguageAdapter.Value)) { Source = Adapter });

            if (Language.Source != null)
                Multi.Bindings.Add(Language.Source);

            return Multi;
        }

        public static IReadOnlyDictionary<object, string> GetCurrentLanguageMap()
        {
            Dictionary<object, string> Map = KeyAdapters.ToDictionary(i => (object)i.Key, i => i.Value.Value);
            foreach (LanguageAdapter Adapter in SourceAdapters)
            {
                object Key = Adapter.Source;
                if (!Map.ContainsKey(Key))
                    Map[Key] = Adapter.Value;
            }

            return Map;
        }

        private sealed class LanguageAdapter : DependencyObject
        {
            private object _Source;
            public object Source
            {
                get => _Source;
                set
                {
                    if (_Source == value)
                        return;

                    _Source = value;
                    Refresh();
                }
            }

            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register(nameof(Value), typeof(string), typeof(LanguageAdapter), new PropertyMetadata(null));
            public string Value
            {
                get => (string)GetValue(ValueProperty);
                set => SetValue(ValueProperty, value);
            }

            private readonly LanguageExtension Binding;
            public LanguageAdapter(LanguageExtension Binding)
            {
                this.Binding = Binding;
                LanguageManager.StaticPropertyChanged += OnLanguageManagerPropertyChanged;
            }

            private LanguagePacket LastPacket;
            private void OnLanguageManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(LanguageManager.Current):
                        {
                            void OnLanguageContextChanged(object sender, string e)
                            {
                                string Key = GetLanguageKey();
                                if (e == Key)
                                    InternalRefresh((LanguagePacket)sender, Key);
                            }

                            if (LastPacket != null)
                                LastPacket.LanguageContextChanged -= OnLanguageContextChanged;

                            if (LanguageManager.Current is LanguagePacket Packet)
                            {
                                LastPacket = Packet;
                                Packet.LanguageContextChanged += OnLanguageContextChanged;
                            }

                            this.Invoke(Refresh);
                            break;
                        }
                    case nameof(LanguageManager.EnableGoogleTranslate):
                        {
                            this.Invoke(Refresh);
                            break;
                        }
                }
            }

            public void Refresh()
            {
                if (LanguageManager.Current is LanguagePacket Packet)
                {
                    InternalRefresh(Packet, GetLanguageKey());
                    return;
                }

                Value = Binding.Default;
            }

            private bool IsFirst = true;
            private CancellationTokenSource TokeSource;
            private void InternalRefresh(LanguagePacket Packet, string Key)
            {
                TokeSource?.Cancel();
                TokeSource = null;

                try
                {
                    if (string.IsNullOrEmpty(Key))
                    {
                        Value = Binding.Default;
                        return;
                    }

                    string Result = Packet[Key];
                    if (!string.IsNullOrEmpty(Result))
                    {
                        Value = Result;
                        return;
                    }

                    string Default = Binding.Default;
                    if (string.IsNullOrEmpty(Default))
                        Default = Key;

                    TokeSource = new CancellationTokenSource();
                    CancellationToken Token = TokeSource.Token;
                    Task.Run(() =>
                    {
                        // Windows System Build-in String
                        string Result = LanguageManager.CurrentWindowsSystem[Key];

                        // Checks if the action is canceled
                        if (Token.IsCancellationRequested)
                            return;

                        if (string.IsNullOrEmpty(Result))
                        {
                            // GoogleTranslate
                            if (!LanguageManager.EnableGoogleTranslate ||
                                !LanguageManager.CanGoogleTranslate ||
                                !GoogleTranslate(Key, out Result))
                                Result = Default;
                        }

                        if (!Token.IsCancellationRequested)
                            this.Invoke(() => Value = Result);
                    });

                    Value = IsFirst ? Default : Binding.LoadingText;
                }
                finally
                {
                    IsFirst = false;
                }
            }

            private string GetLanguageKey()
            {
                string Key = Binding.Path;
                if (string.IsNullOrEmpty(Key))
                {
                    object Source = this.Source;
                    if (Source is Enum)
                    {
                        string Path = Source.ToString();
                        Type Type = Source.GetType();

                        FieldInfo Field = Type.GetField(Path);
                        return Attribute.GetCustomAttribute(Field, typeof(DescriptionAttribute)) is DescriptionAttribute Description ? Description.Description : Path;
                    }

                    return Source?.ToString();
                }

                return Key;
            }
            private static bool GoogleTranslate(string Key, out string Result)
            {
                string ToCulture = LanguageManager.Current?.CultureCode;
                if (!string.IsNullOrEmpty(ToCulture))
                {
                    if (LanguageManager.CacheTranslate.TryGetValue(ToCulture, out ConcurrentDictionary<string, string> Caches))
                    {
                        if (Caches.TryGetValue(Key, out Result))
                            return true;
                    }
                    else
                    {
                        Caches = [];
                        LanguageManager.CacheTranslate.AddOrUpdate(ToCulture, Caches, (k, v) => Caches);
                    }

                    Result = LanguageManager.GoogleTranslate(Key, "en-US", ToCulture);
                    if (!string.IsNullOrEmpty(Result))
                    {
                        string Value = Result;
                        Caches.AddOrUpdate(Key, Result, (k, v) => Value);
                        return true;
                    }
                }

                Result = null;
                return false;
            }

        }

        private class LanguageMultiConverter : IMultiValueConverter
        {
            public static LanguageMultiConverter Instance { get; } = new LanguageMultiConverter();

            public object Convert(object[] Values, Type TargetType, object Parameter, CultureInfo culture)
            {
                // DesignMode
                if (Parameter is string)
                    return Values.Length > 1 ? Values[1] : Parameter;

                // Runtime & Has binding source 
                if (Values.Length > 1 &&
                    Parameter is LanguageAdapter Adapter &&
                    Values[1]?.ToString() != Adapter.Source?.ToString())
                {
                    Adapter.Source = Values[1];
                    return Binding.DoNothing;
                }

                return Values[0];
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
                => throw new NotSupportedException();

        }

    }
}