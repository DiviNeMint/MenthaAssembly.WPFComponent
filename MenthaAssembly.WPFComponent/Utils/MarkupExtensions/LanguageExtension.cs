using MenthaAssembly.Globalization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MenthaAssembly.MarkupExtensions
{
    public class LanguageExtension : MarkupExtension
    {
        private string Path { get; }

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
            string Temp = string.IsNullOrEmpty(Default) ? Path : Default;
            MultiBinding Multi = new()
            {
                Mode = BindingMode.OneWay,
                Converter = LanguageMultiConverter.Instance,
                ConverterParameter = new string[] { Path, Default },
                TargetNullValue = Temp,
                FallbackValue = Temp
            };

            // EnableGoogleTranslate
            Multi.Bindings.Add(new Binding() { Path = new PropertyPath($"(0)", EnableGoogleTranslateProperty) });

            if (Source != null)
            {
                Multi.Bindings.Add(new Binding() { Path = new PropertyPath($"(0)", CurrentProperty) });
                Multi.Bindings.Add(Source);
            }
            else
            {
                Multi.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath($"(0)[{Path}]", CurrentProperty),
                    TargetNullValue = null,
                    FallbackValue = null
                });
            }

            object Value = Multi.ProvideValue(Provider);
            if (!string.IsNullOrEmpty(Path) &&
                Value is BindingExpressionBase Expression)
            {
                if (!BindingsTable.TryGetValue(Path, out List<BindingExpressionBase> Bindings))
                {
                    Bindings = [];
                    BindingsTable.Add(Path, Bindings);
                }

                Bindings.Add(Expression);
            }

            return Value;
        }

        private static readonly Dictionary<string, List<BindingExpressionBase>> BindingsTable = [];

        private static readonly PropertyInfo CurrentProperty, EnableGoogleTranslateProperty;
        static LanguageExtension()
        {
            Type t = typeof(LanguageManager);
            CurrentProperty = t.GetProperty(nameof(LanguageManager.Current));
            EnableGoogleTranslateProperty = t.GetProperty(nameof(LanguageManager.EnableGoogleTranslate));

            LanguageManager.StaticPropertyChanged += OnLanguageChanged;
        }

        private static LanguagePacket LastPacket;
        private static void OnLanguageChanged(object sender, PropertyChangedEventArgs e)
        {
            if (nameof(LanguageManager.Current).Equals(e.PropertyName))
            {
                if (LastPacket != null)
                    LastPacket.LanguageContextChanged -= OnLanguageContextChanged;

                if (LanguageManager.Current is LanguagePacket Packet)
                {
                    LastPacket = Packet;
                    Packet.LanguageContextChanged += OnLanguageContextChanged;
                }
            }
        }

        private static void OnLanguageContextChanged(object sender, string e)
        {
            if (BindingsTable.TryGetValue(e, out List<BindingExpressionBase> Bindings))
                foreach (BindingExpressionBase Binding in Bindings)
                    Binding.UpdateTarget();
        }

        public static BindingBase Create(string Path, string Default)
        {
            string Temp = string.IsNullOrEmpty(Default) ? Path : Default;
            MultiBinding Multi = new()
            {
                Mode = BindingMode.OneWay,
                Converter = LanguageMultiConverter.Instance,
                ConverterParameter = new string[] { Path, Default },
                TargetNullValue = Temp,
                FallbackValue = Temp
            };

            // EnableGoogleTranslate
            Multi.Bindings.Add(new Binding() { Path = new PropertyPath($"(0)", EnableGoogleTranslateProperty) });
            Multi.Bindings.Add(new Binding()
            {
                Path = new PropertyPath($"(0)[{Path}]", CurrentProperty),
                TargetNullValue = null,
                FallbackValue = null
            });

            return Multi;
        }

        private class LanguageMultiConverter : IMultiValueConverter
        {
            public static LanguageMultiConverter Instance { get; } = new LanguageMultiConverter();

            public object Convert(object[] Values, Type TargetType, object Parameter, CultureInfo culture)
            {
                string Result, Path, Default;
                if (Values.Length == 2)
                {
                    Result = Values[1]?.ToString();
                    if (!string.IsNullOrEmpty(Result))
                        return Result;

                    string[] Parameters = (string[])Parameter;
                    Path = Parameters[0];
                    Default = Parameters[1];
                    if (string.IsNullOrEmpty(Path))
                        return Default ?? Binding.DoNothing;
                }
                else
                {
                    object Value = Values[2];
                    if (Value == DependencyProperty.UnsetValue)
                        return null;

                    Path = Value?.ToString();
                    Default = ((string[])Parameter)[1];
                    if (string.IsNullOrEmpty(Path))
                        return Default ?? Binding.DoNothing;

                    Result = LanguageManager.Current?[Path];
                    if (!string.IsNullOrEmpty(Result))
                        return Result;
                }

                // Windows System Build-in String
                Result = LanguageManager.CurrentWindowsSystem[Path];
                if (!string.IsNullOrEmpty(Result))
                    return Result;

                // GoogleTranslate
                string ToCulture = LanguageManager.Current?.CultureCode;
                if (!string.IsNullOrEmpty(ToCulture) &&
                    LanguageManager.CanGoogleTranslate &&
                    Values[0] is true)
                {
                    if (LanguageManager.CacheTranslate.TryGetValue(ToCulture, out ConcurrentDictionary<string, string> Caches))
                    {
                        if (Caches.TryGetValue(Path, out Result))
                            return Result;
                    }
                    else
                    {
                        Caches = [];
                        LanguageManager.CacheTranslate.AddOrUpdate(ToCulture, Caches, (k, v) => Caches);
                    }

                    Result = LanguageManager.GoogleTranslate(Path, "en-US", ToCulture);
                    if (!string.IsNullOrEmpty(Result))
                    {
                        Caches.AddOrUpdate(Path, Result, (k, v) => Result);
                        return Result;
                    }
                }

                Debug.WriteLine($"[Language] Not fount {Path}.");
                return Default ?? Path;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
                => throw new NotSupportedException();

        }

    }
}