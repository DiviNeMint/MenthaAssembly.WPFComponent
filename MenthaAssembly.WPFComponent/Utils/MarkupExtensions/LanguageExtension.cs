using MenthaAssembly.Globalization;
using System;
using System.Collections.Concurrent;
using System.Globalization;
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
            LanguageMultiConverter Converter = new(Path, Default);
            MultiBinding Multi = new()
            {
                Mode = BindingMode.OneWay,
                Converter = Converter,
                TargetNullValue = Temp,
                FallbackValue = Temp
            };

            Multi.Bindings.Add(new Binding() { Path = new PropertyPath($"(0)", EnableGoogleTranslateProperty) });
            Multi.Bindings.Add(new Binding() { Path = new PropertyPath($"(0)", CustomLanguageProperty) });

            if (Source != null)
                Multi.Bindings.Add(Source);

            if (Multi.ProvideValue(Provider) is not BindingExpressionBase Expression)
                return this;

            Converter.Expression = Expression;
            return Expression;
        }

        private static readonly PropertyInfo CustomLanguageProperty, EnableGoogleTranslateProperty;
        static LanguageExtension()
        {
            Type t = typeof(LanguageManager);
            CustomLanguageProperty = t.GetProperty(nameof(LanguageManager.Custom));
            EnableGoogleTranslateProperty = t.GetProperty(nameof(LanguageManager.EnableGoogleTranslate));
        }

        public static BindingBase Create(string Path, string Default)
        {
            string Temp = string.IsNullOrEmpty(Default) ? Path : Default;
            MultiBinding Multi = new()
            {
                Mode = BindingMode.OneWay,
                Converter = new LanguageMultiConverter(Path, Default),
                TargetNullValue = Temp,
                FallbackValue = Temp
            };

            // EnableGoogleTranslate
            Multi.Bindings.Add(new Binding() { Path = new PropertyPath($"(0)", EnableGoogleTranslateProperty) });
            Multi.Bindings.Add(new Binding()
            {
                Path = new PropertyPath($"(0)[{Path}]", CustomLanguageProperty),
                TargetNullValue = null,
                FallbackValue = null
            });

            return Multi;
        }

        private class LanguageMultiConverter(string Path, string Default) : IMultiValueConverter
        {
            private static readonly ConcurrentQueue<BindingExpressionBase> OSLanguageQueue = [];
            private static readonly ConcurrentQueue<BindingExpressionBase> GoogleTranslateQueue = [];
            public BindingExpressionBase Expression;

            private bool IsFirst = true;
            private LanguagePacket LastLanguage;
            private CancellationTokenSource OSLanguageCancellation, GoogleTranslateCancellation;
            public object Convert(object[] Values, Type TargetType, object Parameter, CultureInfo culture)
            {
                try
                {
                    if (Values[1] is not LanguagePacket Language)
                        return string.IsNullOrEmpty(Path) ? Default ?? Binding.DoNothing : Path;

                    if (LastLanguage != Language)
                    {
                        if (LastLanguage != null)
                            LastLanguage.LanguageContextChanged -= OnLanguageContextChanged;

                        LastLanguage = Language;
                        LastLanguage.LanguageContextChanged += OnLanguageContextChanged;
                    }

                    string Result;
                    if (Values.Length == 2)
                    {
                        Result = Language[Path];
                        if (!string.IsNullOrEmpty(Result))
                            return Result;

                        if (string.IsNullOrEmpty(Path))
                            return Default ?? Binding.DoNothing;
                    }
                    else
                    {
                        object Value = Values[2];
                        if (Value == DependencyProperty.UnsetValue)
                            return null;

                        Path = Value?.ToString();
                        if (string.IsNullOrEmpty(Path))
                            return Default ?? Binding.DoNothing;

                        Result = Language[Path];
                        if (!string.IsNullOrEmpty(Result))
                            return Result;
                    }

                    if (LanguageManager.LazySystem?.IsValueCreated is not true)
                    {
                        OSLanguageQueue.Enqueue(Expression);
                        if (OSLanguageQueue.Count == 1)
                        {
                            OSLanguageCancellation?.Cancel();
                            OSLanguageCancellation = new CancellationTokenSource();
                            CancellationToken Token = OSLanguageCancellation.Token;
                            Task.Run(() =>
                            {
                                _ = LanguageManager.System;
                                if (!Token.IsCancellationRequested)
                                {
                                    Application.Current.Invoke(() =>
                                    {
                                        while (OSLanguageQueue.TryDequeue(out BindingExpressionBase Binding))
                                            Binding.UpdateTarget();
                                    });
                                }
                            });
                        }

                        return IsFirst && Default != null ? Default : Binding.DoNothing;
                    }

                    Result = LanguageManager.System[Path];
                    if (!string.IsNullOrEmpty(Result))
                        return Result;

                    // GoogleTranslate
                    if (Values[0] is true)
                    {
                        string ToCulture = LanguageManager.Custom?.CultureCode;
                        if (!string.IsNullOrEmpty(ToCulture) &&
                            LanguageManager.CanGoogleTranslate)
                        {
                            (string, string ToCulture) Key = ("en-US", ToCulture);
                            if (!LanguageManager.CacheTranslate.TryGetValue(Key, out ConcurrentDictionary<string, Lazy<string>> Caches))
                                Caches = LanguageManager.CacheTranslate.GetOrAdd(Key, k => []);

                            Lazy<string> LazyResult = Caches.GetOrAdd(Path, k => new Lazy<string>(() => LanguageManager.InternalGoogleTranslate(k, "en-US", ToCulture)));
                            if (!LazyResult.IsValueCreated)
                            {
                                GoogleTranslateQueue.Enqueue(Expression);
                                if (GoogleTranslateQueue.Count == 1)
                                {
                                    GoogleTranslateCancellation?.Cancel();
                                    GoogleTranslateCancellation = new CancellationTokenSource();
                                    CancellationToken Token = GoogleTranslateCancellation.Token;
                                    Task.Run(() =>
                                    {
                                        _ = LazyResult.Value;
                                        if (!Token.IsCancellationRequested)
                                        {
                                            Application.Current.Invoke(() =>
                                            {
                                                while (GoogleTranslateQueue.TryDequeue(out BindingExpressionBase Binding))
                                                    Binding.UpdateTarget();
                                            });
                                        }
                                    });
                                }

                                return IsFirst && Default != null ? Default : Binding.DoNothing;
                            }

                            Result = LazyResult.Value;
                        }
                    }

                    return Result ?? Default;
                }
                finally
                {
                    IsFirst = false;
                }
            }

            private void OnLanguageContextChanged(object sender, string e)
            {
                if (e == Path)
                    Application.Current.Invoke(Expression.UpdateTarget);
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
                => throw new NotSupportedException();

        }

    }
}