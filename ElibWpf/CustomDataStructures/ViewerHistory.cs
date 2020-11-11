using System.Collections.Generic;
using ElibWpf.ViewModels;

namespace ElibWpf.CustomDataStructures
{
    public class ViewerHistory
    {
        private readonly Stack<IViewer> stack = new Stack<IViewer>();

        public int Count => stack.Count;

        public void Clear()
        {
            stack.Clear();
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
            stack.Push(element);
        }
    }
}
