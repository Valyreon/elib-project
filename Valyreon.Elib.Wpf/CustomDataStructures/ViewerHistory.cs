using System;
using System.Collections.Generic;
using Valyreon.Elib.Wpf.ViewModels;

namespace Valyreon.Elib.Wpf.CustomDataStructures
{
    public class ViewerHistory
    {
        private readonly Stack<Func<IViewer>> stack = new Stack<Func<IViewer>>();

        public int Count => stack.Count;

        public void Clear()
        {
            stack.Clear();
        }

        public Func<IViewer> Peek()
        {
            return stack.Peek();
        }

        public Func<IViewer> Pop()
        {
            return stack.Pop();
        }

        public void Push(Func<IViewer> element)
        {
            stack.Push(element);
        }
    }
}
