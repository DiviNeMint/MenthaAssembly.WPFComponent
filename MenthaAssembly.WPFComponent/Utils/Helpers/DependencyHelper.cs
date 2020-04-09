using MenthaAssembly;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;

namespace System.Windows
{
    public static class DependencyHelper
    {
        //public static DependencyProperty GetDependencyProperty(this DependencyObject This, string PropertyName)
        //{
        //    if (This.GetType()?
        //            .GetField($"{PropertyName}Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is FieldInfo FieldInfo)
        //        return FieldInfo.GetValue(null) as DependencyProperty;

        //    return null;
        //}

        public static DependencyProperty GetDependencyProperty(this DependencyObject This, string PropertyName)
        {
            if (This.GetType() is Type ThisType)
                return DependencyPropertyDescriptor.FromName(PropertyName, ThisType, ThisType)?.DependencyProperty;

            return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject This) where T : DependencyObject
        {
            if (This != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(This); i++)
                {
                    DependencyObject Child = VisualTreeHelper.GetChild(This, i);
                    if (Child is T)
                        yield return (T)Child;

                    foreach (T ChildOfChild in FindVisualChildren<T>(Child))
                        yield return ChildOfChild;
                }
            }
        }

        public static IEnumerable<T> FindVisualParents<T>(this DependencyObject This)
            where T : DependencyObject
        {
            if (This != null)
            {
                DependencyObject Parent = VisualTreeHelper.GetParent(This);
                if (Parent is T Result)
                    yield return Result;

                foreach (T ParentOfParent in FindVisualParents<T>(Parent))
                    yield return ParentOfParent;
            }
        }

        public static ChangedEventArgs<T> ToChangedEventArgs<T>(this DependencyPropertyChangedEventArgs e)
            => new ChangedEventArgs<T>(e.OldValue is T New ? New : default,
                                       e.NewValue is T Old ? Old : default);

        public static void OnPropertyChanged(this INotifyPropertyChanged This, [CallerMemberName]string PropertyName = null)
        {
            if (PropertyName is null ||
                !(This.GetEventField("PropertyChanged") is MulticastDelegate Handler))
                return;

            Delegate[] Invocations = Handler.GetInvocationList();
            if (Invocations.Length > 0)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(PropertyName);
                foreach (PropertyChangedEventHandler Event in Invocations)
                {
                    if (Event.Target is DispatcherObject DispObj && !DispObj.CheckAccess())
                    {
                        // Invoke handler in the target dispatcher's thread
                        DispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, Event, This, e);
                        continue;
                    }
                    Event(This, e);
                }
            }
        }
        public static void OnPropertyChanged(this INotifyPropertyChanged This, PropertyChangedEventArgs e)
        {
            if (e is null ||
                !(This.GetEventField("PropertyChanged") is MulticastDelegate Handler))
                return;

            Delegate[] Invocations = Handler.GetInvocationList();
            if (Invocations.Length > 0)
            {
                foreach (PropertyChangedEventHandler Event in Invocations)
                {
                    if (Event.Target is DispatcherObject DispObj && !DispObj.CheckAccess())
                    {
                        // Invoke handler in the target dispatcher's thread
                        DispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, Event, This, e);
                        continue;
                    }
                    Event(This, e);
                }
            }
        }

        public static void OnCollectionChanged(this INotifyCollectionChanged This, NotifyCollectionChangedEventArgs e)
        {
            if (!(This.GetEventField("CollectionChanged") is MulticastDelegate Handler))
                return;

            foreach (NotifyCollectionChangedEventHandler Event in Handler.GetInvocationList())
            {
                if (Event.Target is DispatcherObject DispObj && !DispObj.CheckAccess())
                {
                    //Invoke handler in the target dispatcher's thread
                    DispObj.Dispatcher.Invoke(DispatcherPriority.DataBind, Event, This, e);
                    continue;
                }
                Event(This, e);
            }
        }

    }
}
