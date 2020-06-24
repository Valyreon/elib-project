using ElibWpf.ViewModels;
using System.Collections.Generic;

namespace ElibWpf.CustomDataStructures
{
    public class ViewerHistory
    {
        private readonly Stack<IViewer> stack = new Stack<IViewer>();

        public int Count => this.stack.Count;

        public void Clear()
        {
            this.stack.Clear();
        }

        public IViewer Peek()
        {
            return stack.Peek();
        }

        public IViewer Pop()
        {
            return stack.Pop();
        }

        public void Push(IViewer element)
        {
            element.Clear();
            this.stack.Push(element);
        }
    }
}
