using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Models.Observables
{
    public class ObservableStack<T>
    {
        private readonly ObservableCollection<T> list = new ObservableCollection<T>();

        public int Count => this.list.Count;

        public void AddHandlerOnStackChange(NotifyCollectionChangedEventHandler x)
        {
            this.list.CollectionChanged += x;
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public T Peek()
        {
            return this.list.Last();
        }

        public T Pop()
        {
            T res = this.list.Last();
            this.list.RemoveAt(this.list.Count - 1);
            return res;
        }

        public void Push(T element)
        {
            this.list.Add(element);
        }
    }
}