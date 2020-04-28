using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Models.Observables
{
    public class ObservableStack<T>
    {
        private readonly ObservableCollection<T> list = new ObservableCollection<T>();

        public int Count { get => list.Count; }

        public void AddHandlerOnStackChange(NotifyCollectionChangedEventHandler x)
        {
            list.CollectionChanged += x;
        }

        public void Clear()
        {
            list.Clear();
        }

        public T Peek()
        {
            return list.Last();
        }

        public T Pop()
        {
            var res = list.Last();
            list.RemoveAt(list.Count - 1);
            return res;
        }

        public void Push(T element)
        {
            list.Add(element);
        }
    }
}