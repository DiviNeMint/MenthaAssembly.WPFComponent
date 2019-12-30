using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MenthaAssembly
{
    public class LanguageBinding : MarkupExtension
    {
        private string Path { get; }

        public bool IsObjectProperty { set; get; }

        public string Default { set; get; }

        public LanguageBinding()
        {
            IsObjectProperty = true;
        }
        public LanguageBinding(string Path)
        {
            this.Path = Path;

            if (string.IsNullOrEmpty(Path))
                IsObjectProperty = true;
        }

        public override object ProvideValue(IServiceProvider Provider)
        {
            if (IsObjectProperty &&
                Provider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget ValueTarget)
            {
                if (ValueTarget.TargetObject is DependencyObject Item)
                {
                    if (string.IsNullOrEmpty(Path))
                    {
                        // Only DataContext
                        if (ValueTarget.TargetProperty is DependencyProperty DependencyProperty &&
                            DependencyPropertyDescriptor.FromProperty(FrameworkElement.DataContextProperty, Item.GetType()) is DependencyPropertyDescriptor DPDescriptor &&
                            Item.GetValue(FrameworkElement.DataContextProperty)?.ToString() is string DataContextString)
                        {
                            DPDescriptor.AddValueChanged(Item,
                                (s, e) =>
                                {
                                    if (Item.GetValue(FrameworkElement.DataContextProperty)?.ToString() is string NewDataContextString)
                                        BindingOperations.SetBinding(Item, DependencyProperty, Create(NewDataContextString, NewDataContextString));
                                });

                            return Create(DataContextString, DataContextString).ProvideValue(Provider);
                        }

                        return new Binding().ProvideValue(Provider);
                    }
                    else
                    {
                        // DataContext Property
                        if (Item.GetValue(FrameworkElement.DataContextProperty) is object DataContext &&
                            DataContext.GetType().GetProperty(this.Path) is PropertyInfo DataProperty)
                        {
                            if (DataContext is INotifyPropertyChanged NotifyData &&
                                ValueTarget.TargetProperty is DependencyProperty DependencyProperty)
                                NotifyData.PropertyChanged += (s, e) =>
                                {
                                    if (e.PropertyName.Equals(DependencyProperty.Name) &&
                                        DataProperty.GetValue(DataContext)?.ToString() is string DataPath)
                                        BindingOperations.SetBinding(Item, DependencyProperty, Create(DataPath, this.Default));
                                };

                            return Create(DataProperty.GetValue(DataContext)?.ToString() ?? this.Path, this.Default).ProvideValue(Provider);
                        }

                        // Control Property
                        if (Item.GetType() is Type ControlType &&
                            ControlType.GetProperty(this.Path) is PropertyInfo ControlProperty)
                        {
                            //if (TypeDescriptor.GetProperties(Item)[Path] is PropertyDescriptor prop)
                            //{
                            //    prop.AddValueChanged(Item, (s, e) => 
                            //    {
                            //        Console.WriteLine("Test");
                            //    });
                            //}

                            //    if (ValueTarget.TargetProperty is DependencyProperty DependencyProperty)
                            //        DependencyPropertyDescriptor.FromName(Path, ControlType, ControlType).AddValueChanged(Item,
                            //            (s, e) =>
                            //            {
                            //                if (ControlProperty.GetValue(Item)?.ToString() is string NewControlPath)
                            //                    BindingOperations.SetBinding(Item, DependencyProperty, Create(NewControlPath));
                            //            });

                            return Create(ControlProperty.GetValue(Item)?.ToString() ?? this.Path, this.Default).ProvideValue(Provider);
                        }
                    }

                    return Create(this.Path, this.Default).ProvideValue(Provider);
                }
                return this;
            }
            return Create(this.Path, this.Default).ProvideValue(Provider);
        }

        private static string CurrentLanguagePath { get; } = $"{nameof(LanguageManager)}.{nameof(LanguageManager.Current)}";
        public static Binding Create(string Path, string Default)
            => new Binding
            {
                Path = new PropertyPath($"({CurrentLanguagePath}).{Path}"),
                FallbackValue = string.IsNullOrEmpty(Default) ? Path : Default
            };
    }
}
