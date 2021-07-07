using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace MenthaAssembly
{
    public class LanguageBinding : MarkupExtension
    {
        internal string Path { get; }

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
                            DependencyPropertyDescriptor.FromProperty(FrameworkElement.DataContextProperty, Item.GetType()) is DependencyPropertyDescriptor DPDescriptor)
                        {
                            DPDescriptor.AddValueChanged(Item,
                                (s, e) =>
                                {
                                    if (Item.GetValue(FrameworkElement.DataContextProperty)?.ToString() is string NewDataContextString)
                                        BindingOperations.SetBinding(Item, DependencyProperty, Create(NewDataContextString, NewDataContextString));
                                });

                            if (Item.GetValue(FrameworkElement.DataContextProperty)?.ToString() is string DataContextString)
                                return Create(DataContextString, DataContextString).ProvideValue(Provider);
                        }

                        return new Binding().ProvideValue(Provider);
                    }
                    else
                    {
                        // DataContext Property
                        if (Item.GetValue(FrameworkElement.DataContextProperty) is object DataContext &&
                            TryGetValue(DataContext, Path, out object DataValue))
                        {
                            // TODO : 
                            // When DataContext's property changed, update binding.

                            //if (DataContext is INotifyPropertyChanged NotifyData &&
                            //    ValueTarget.TargetProperty is DependencyProperty DependencyProperty)
                            //    NotifyData.PropertyChanged += (s, e) =>
                            //    {
                            //        //if (GetValue(DataContext, Path)?.ToString() is string DataPath)
                            //        //else
                            //        //BindingOperations.ClearBinding(Item, DependencyProperty);
                            //        //if (e.PropertyName.Equals(DependencyProperty.Name) &&
                            //        //    GetValue(DataContext, Path)?.ToString() is string DataPath)
                            //        //    BindingOperations.SetBinding(Item, DependencyProperty, Create(DataPath, this.Default));
                            //    };

                            string DataPath = DataValue?.ToString();
                            return string.IsNullOrEmpty(DataPath) ? "Null" :
                                                                    Create(DataPath, this.Default).ProvideValue(Provider);
                        }

                        // Control Property
                        if (TryGetValue(Item, Path, out object ControlValue))
                        {
                            string ControlPath = ControlValue?.ToString();
                            return string.IsNullOrEmpty(ControlPath) ? "Null" :
                                                                       Create(ControlPath?.ToString() ?? this.Path, this.Default).ProvideValue(Provider);
                        }


                        // TODO : 
                        // When Control's property changed, update binding.

                        //if (Item.GetType() is Type ControlType &&
                        //    ControlType.GetProperty(this.Path) is PropertyInfo ControlProperty)
                        //{
                        //    if (TypeDescriptor.GetProperties(Item)[Path] is PropertyDescriptor prop)
                        //    {
                        //        prop.AddValueChanged(Item, (s, e) =>
                        //        {
                        //            Console.WriteLine("Test");
                        //        });
                        //    }

                        //    if (ValueTarget.TargetProperty is DependencyProperty DependencyProperty)
                        //        DependencyPropertyDescriptor.FromName(Path, ControlType, ControlType).AddValueChanged(Item,
                        //            (s, e) =>
                        //            {
                        //                if (ControlProperty.GetValue(Item)?.ToString() is string NewControlPath)
                        //                    BindingOperations.SetBinding(Item, DependencyProperty, Create(NewControlPath));
                        //            });

                        //    return Create(ControlProperty.GetValue(Item)?.ToString() ?? this.Path, this.Default).ProvideValue(Provider);
                        //}
                        return Create(this.Path, this.Default).ProvideValue(Provider);
                    }
                }
                return this;
            }
            return Create(this.Path, this.Default).ProvideValue(Provider);
        }
        private bool TryGetValue(object Item, string Path, out object Value)
        {
            Value = Item;
            if (Value is null)
                return false;

            Type ValueType = Value.GetType();

            // Index
            Match mItem = Regex.Match(Path, @"^\[?(?<Index>[\d\w]+)\]$");
            if (mItem.Success)
            {
                string Key = mItem.Groups["Index"].Value;
                foreach (MethodInfo Method in ValueType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                                       .Where(i => i.Name.Equals("get_Item")))
                {
                    ParameterInfo[] Args = Method.GetParameters();
                    if (Args.Length > 1)
                        continue;

                    object Arg = Convert.ChangeType(Key, Args[0].ParameterType);
                    Value = Method.Invoke(Item, new[] { Arg });
                    return true;
                }

                return false;
            }

            // Property
            foreach (Match m in Regex.Matches(Path, @"(?<PropertyName>[\w-[\[\]]]+)(\[(?<Index>\d+)\])?"))
            {
                if (m.Success &&
                    ValueType?.GetProperty(m.Groups["PropertyName"].Value) is PropertyInfo TempProperty)
                {
                    string IndexStr = m.Groups["Index"].Value;
                    if (string.IsNullOrEmpty(IndexStr))
                    {
                        Value = TempProperty.GetValue(Value);
                    }
                    else if (int.TryParse(m.Groups["Index"].Value, out int Index))
                    {
                        object Collection = TempProperty.GetValue(Value);
                        if (Collection is IList List)
                        {
                            if (List.Count <= Index)
                                return false;

                            Value = List[Index];
                        }
                        else if (Collection is Array Array)
                        {
                            if (Array.Length <= Index)
                                return false;

                            Value = Array.GetValue(Index);
                        }
                        else
                        {
                            Value = TempProperty.GetValue(Value, new object[] { Index });
                        }
                    }
                    else
                        break;

                    ValueType = Value?.GetType();
                    continue;
                }

                return false;
            }

            return !Item.Equals(Value);
        }

        private static readonly PropertyInfo LanguageCurrentInfo = typeof(LanguageManager).GetProperty(nameof(LanguageManager.Current));
        public static Binding Create(string Path, string Default)
            => new Binding
            {
                Path = new PropertyPath($"(0).{Path}", LanguageCurrentInfo),
                FallbackValue = string.IsNullOrEmpty(Default) ? Path : Default
            };

    }
}
