using MenthaAssembly;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace System.Windows
{
    public static class DependencyHelper
    {
        private static FieldInfo DisconnectedItemField;
        private static object DisconnectedItem;
        public static bool IsDisconnectedItem(object Obj)
        {
            if (Obj is null)
                return false;

            if (DisconnectedItemField is null &&
                typeof(BindingExpressionBase).TryGetStaticInternalField(nameof(DisconnectedItem), out DisconnectedItemField))
                DisconnectedItem = DisconnectedItemField.GetValue(null);

            return Obj == DisconnectedItem;
        }

        /// <summary>
        ///     Returns true if the binding (or any part of it) is OneWay.
        /// </summary>
        public static bool IsOneWay(BindingBase BindingBase)
        {
            if (BindingBase is null)
                return false;

            // If it is a standard Binding, then check if it's Mode is OneWay
            if (BindingBase is Binding binding)
                return binding.Mode == BindingMode.OneWay;

            // A multi-binding can be OneWay as well
            if (BindingBase is MultiBinding multiBinding)
                return multiBinding.Mode == BindingMode.OneWay;

            // A priority binding is a list of bindings, if any are OneWay, we'll call it OneWay
            if (BindingBase is PriorityBinding priBinding)
            {
                Collection<BindingBase> SubBindings = priBinding.Bindings;
                int count = SubBindings.Count;
                for (int i = 0; i < count; i++)
                {
                    if (IsOneWay(SubBindings[i]))
                        return true;
                }
            }

            return false;
        }

        public static string GetPathFromBinding(Binding Binding)
        {
            if (Binding != null)
            {
                if (!string.IsNullOrEmpty(Binding.XPath))
                    return Binding.XPath;

                if (Binding.Path != null)
                    return Binding.Path.Path;
            }

            return null;
        }

        /// <summary>
        /// Forces the ValidationRules to be checked for the specified BindingGroup.
        /// </summary>
        public static bool Validate(this BindingGroup Group)
        {
            // If you commit to editing a new item without making any modifications,
            // the validation rules will not be triggered and therefore an update will be forced.
            foreach (BindingExpressionBase Binding in Group.BindingExpressions)
            {
                Binding.UpdateSource();
                if (Binding.HasError)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Assigns the Binding to the desired property
        /// </summary>
        public static void ApplyBinding(this DependencyObject This, BindingBase Binding, DependencyProperty Property)
        {
            if (Binding != null)
                BindingOperations.SetBinding(This, Property, Binding);
            else
                BindingOperations.ClearBinding(This, Property);
        }

        public static bool IsDefaultValue(this DependencyObject This, DependencyProperty dp)
            => DependencyPropertyHelper.GetValueSource(This, dp).BaseValueSource == BaseValueSource.Default;

        public static DependencyProperty GetDependencyProperty(this DependencyObject This, string PropertyName)
        {
            if (This.GetType() is Type ThisType)
                return DependencyPropertyDescriptor.FromName(PropertyName, ThisType, ThisType)?.DependencyProperty;

            return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject This) where T : DependencyObject
        {
            if (This is null)
                yield break;

            int Count = VisualTreeHelper.GetChildrenCount(This);
            for (int i = 0; i < Count; i++)
            {
                DependencyObject Child = VisualTreeHelper.GetChild(This, i);
                if (Child is T TChild)
                    yield return TChild;

                foreach (T ChildOfChild in FindVisualChildren<T>(Child))
                    yield return ChildOfChild;
            }
        }

        public static IEnumerable<T> FindVisualParents<T>(this DependencyObject This)
            where T : DependencyObject
        {
            if (This is null)
                yield break;

            DependencyObject Parent = VisualTreeHelper.GetParent(This);
            if (Parent is T Result)
                yield return Result;

            foreach (T ParentOfParent in FindVisualParents<T>(Parent))
                yield return ParentOfParent;
        }

        public static IEnumerable<T> FindVisuaBrothers<T>(this DependencyObject This)
            where T : DependencyObject
        {
            if (This is null)
                yield break;

            if (VisualTreeHelper.GetParent(This) is DependencyObject Parent)
            {
                int Count = VisualTreeHelper.GetChildrenCount(Parent);
                for (int i = 0; i < Count; i++)
                    if (VisualTreeHelper.GetChild(Parent, i) is T Brother)
                        yield return Brother;
            }
        }

        public static IEnumerable<T> FindLogicalParents<T>(this DependencyObject This)
            where T : DependencyObject
        {
            if (This != null)
            {
                DependencyObject Parent = LogicalTreeHelper.GetParent(This);
                if (Parent is T Result)
                    yield return Result;

                foreach (T ParentOfParent in FindLogicalParents<T>(Parent))
                    yield return ParentOfParent;
            }
        }

        private static PropertyInfo PropertyPath_Length;
        private static MethodInfo PropertyPath_SetContext,
                                  PropertyPath_GetAccessor,
                                  PropertyPath_GetItem;
        public static bool TryGetPropertyByPath<TInfo, TValue>(this DependencyObject This, string PropertyPath, out TInfo PropertyInfo, out TValue ParentObject)
            where TInfo : class
            => TryGetPropertyByPath(This, new PropertyPath(PropertyPath), out PropertyInfo, out ParentObject);
        public static bool TryGetPropertyByPath<TInfo, TValue>(this DependencyObject This, PropertyPath PropertyPath, out TInfo PropertyInfo, out TValue ParentObject)
            where TInfo : class
        {
            if (PropertyPath_Length is null)
                ReflectionHelper.TryGetInternalProperty<PropertyPath>("Length", out PropertyPath_Length);

            if (PropertyPath_SetContext is null)
                ReflectionHelper.TryGetInternalMethod<PropertyPath>("SetContext", out PropertyPath_SetContext);

            if (PropertyPath_Length?.GetValue(PropertyPath) is int PropertyPathLength &&
                PropertyPath_SetContext?.Invoke(PropertyPath, [This]) is IDisposable Worker)
            {

                if (PropertyPath_GetAccessor is null)
                    ReflectionHelper.TryGetInternalMethod<PropertyPath>("GetAccessor", out PropertyPath_GetAccessor);

                if (PropertyPath_GetItem is null)
                    ReflectionHelper.TryGetInternalMethod<PropertyPath>("GetItem", out PropertyPath_GetItem);

                try
                {
                    object[] Args = [PropertyPathLength - 1];
                    PropertyInfo = PropertyPath_GetAccessor?.Invoke(PropertyPath, Args) as TInfo;
                    ParentObject = (TValue)PropertyPath_GetItem?.Invoke(PropertyPath, Args);

                    return PropertyInfo != null && ParentObject != null;
                }
                finally
                {
                    Worker.Dispose();
                }
            }

            PropertyInfo = default;
            ParentObject = default;
            return false;
        }

        private static MethodInfo PropertyPath_GetValue;
        public static bool TryGetPropertyValueByPath<T>(this DependencyObject This, string PropertyPath, out T Value)
            => TryGetPropertyValueByPath(This, new PropertyPath(PropertyPath), out Value);
        public static bool TryGetPropertyValueByPath<T>(this DependencyObject This, PropertyPath PropertyPath, out T Value)
        {
            if (PropertyPath_Length is null)
                ReflectionHelper.TryGetInternalProperty<PropertyPath>("Length", out PropertyPath_Length);

            if (PropertyPath_SetContext is null)
                ReflectionHelper.TryGetInternalMethod<PropertyPath>("SetContext", out PropertyPath_SetContext);

            if (PropertyPath_SetContext?.Invoke(PropertyPath, [This]) is IDisposable Worker)
            {

                if (PropertyPath_GetValue is null)
                    ReflectionHelper.TryGetInternalMethod<PropertyPath>("GetValue", out PropertyPath_GetValue);

                try
                {
                    Value = (T)PropertyPath_GetValue?.Invoke(PropertyPath, null);
                    return true;
                }
                finally
                {
                    Worker.Dispose();
                }
            }

            Value = default;
            return false;
        }

        public static ChangedEventArgs<T> ToChangedEventArgs<T>(this DependencyPropertyChangedEventArgs e)
            => new(e.OldValue is T New ? New : default,
                   e.NewValue is T Old ? Old : default);

        public static RoutedPropertyChangedEventArgs<T> ToRoutedPropertyChangedEventArgs<T>(this DependencyPropertyChangedEventArgs e, RoutedEvent Event)
            => new(e.OldValue is T New ? New : default,
                   e.NewValue is T Old ? Old : default,
                   Event);
        public static RoutedPropertyChangedEventArgs<T> ToRoutedPropertyChangedEventArgs<T>(this ChangedEventArgs<T> e, RoutedEvent Event)
            => new(e.OldValue, e.NewValue, Event);

        public static void OnPropertyChanged(this INotifyPropertyChanged This, [CallerMemberName] string PropertyName = null)
        {
            if (PropertyName is null)
                return;

            if (This.TryGetEventField("PropertyChanged", out MulticastDelegate Handler))
            {
                Delegate[] Invocations = Handler.GetInvocationList();
                if (Invocations.Length > 0)
                {
                    PropertyChangedEventArgs e = new(PropertyName);
                    foreach (Delegate Event in Invocations)
                    {
                        if (Event.Target is DispatcherObject DispObj &&
                            !DispObj.CheckAccess())
                        {
                            // Invoke handler in the target dispatcher's thread
                            DispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, Event, This, e);
                            continue;
                        }
                        Event.DynamicInvoke(This, e);
                    }
                }
            }
        }
        public static void OnPropertyChanged(this INotifyPropertyChanged This, PropertyChangedEventArgs e)
        {
            if (e is null)
                return;

            if (This.TryGetEventField("PropertyChanged", out MulticastDelegate Handler))
            {
                foreach (Delegate Event in Handler.GetInvocationList())
                {
                    if (Event.Target is DispatcherObject DispObj && !DispObj.CheckAccess())
                    {
                        // Invoke handler in the target dispatcher's thread
                        DispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, Event, This, e);
                        continue;
                    }
                    Event.DynamicInvoke(This, e);
                }
            }
        }

        public static void OnCollectionChanged(this INotifyCollectionChanged This, NotifyCollectionChangedEventArgs e)
        {
            if (This.TryGetEventField("CollectionChanged", out MulticastDelegate Handler))
            {
                foreach (Delegate Event in Handler.GetInvocationList())
                {
                    if (Event.Target is DispatcherObject DispObj && !DispObj.CheckAccess())
                    {
                        //Invoke handler in the target dispatcher's thread
                        DispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, Event, This, e);
                        continue;
                    }
                    Event.DynamicInvoke(This, e);
                }
            }
        }

    }
}