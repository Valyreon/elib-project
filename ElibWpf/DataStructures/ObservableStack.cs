using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.DataStructures
{
    public class ObservableStack<T>
    {
        private readonly ObservableCollection<T> list = new ObservableCollection<T>();

        public void Push(T element)
        {
            list.Add(element);
        }

        public T Pop()
        {
            var res = list.Last();
            list.RemoveAt(list.Count - 1);
            return res;
        }

        public T Peek()
        {
            return list.Last();
        }

        public void AddHandlerOnStackChange(NotifyCollectionChangedEventHandler x)
        {
            list.CollectionChanged += x;
        }

        public int Count { get => list.Count; }

        public void Clear()
        {
            list.Clear();
        }
    }
}
