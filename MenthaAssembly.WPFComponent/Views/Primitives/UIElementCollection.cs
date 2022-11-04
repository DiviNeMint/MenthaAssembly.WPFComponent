using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly.Views
{
    public class UIElementCollection<T> : IList<T>, IList
        where T : UIElement
    {
        protected readonly UIElementCollection Collection;

        public int Count
            => Collection.Count;

        public T this[int Index]
        {
            get => (T)Collection[Index];
            set => Collection[Index] = value;
        }

        public UIElementCollection(FrameworkElement Parent) : this(Parent, Parent)
        {
        }
        public UIElementCollection(UIElement VisualParent, FrameworkElement LogicalParent)
        {
            Collection = new UIElementCollection(VisualParent, LogicalParent);
        }

        public virtual void Add(T Element)
            => Collection.Add(Element);

        public void Insert(int Index, T Element)
            => Collection.Insert(Index, Element);

        public int IndexOf(T Element)
            => Collection.IndexOf(Element);

        public virtual bool Remove(T Element)
        {
            Collection.Remove(Element);
            return true;
        }

        public virtual void RemoveAt(int Index)
            => Collection.RemoveAt(Index);

        public virtual void Clear()
            => Collection.Clear();

        public virtual bool Contains(T Element)
            => Collection.Contains(Element);

        public void CopyTo(T[] Array, int Index)
        {
            int Length = Math.Min(Array.Length, Count - Index);
            for (int i = 0; i < Length; i++, Index++)
                Array[i] = this[Index];
        }

        public IEnumerator<T> GetEnumerator()
            => Collection.OfType<T>().GetEnumerator();

        object IList.this[int Index]
        {
            get => this[Index];
            set
            {
                if (value is not T Element)
                    throw new NotSupportedException();

                this[Index] = Element;
            }
        }
        bool IList.IsReadOnly
            => ((IList)Collection).IsReadOnly;
        bool IList.IsFixedSize
            => ((IList)Collection).IsFixedSize;
        int IList.Add(object Value)
            => Value is UIElement Element ? Collection.Add(Element) : -1;
        void IList.Insert(int Index, object Value)
            => ((IList)Collection).Insert(Index, Value);
        void IList.Remove(object Value)
            => ((IList)Collection).Remove(Value);
        bool IList.Contains(object Value)
            => Value is T Element && Collection.Contains(Element);
        int IList.IndexOf(object Value)
            => Value is T Element ? Collection.IndexOf(Element) : -1;

        object ICollection.SyncRoot
            => ((ICollection)Collection).SyncRoot;
        bool ICollection.IsSynchronized
            => ((ICollection)Collection).IsSynchronized;
        void ICollection.CopyTo(Array Array, int Index)
            => ((ICollection)Collection).CopyTo(Array, Index);

        bool ICollection<T>.IsReadOnly
            => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

    }
}