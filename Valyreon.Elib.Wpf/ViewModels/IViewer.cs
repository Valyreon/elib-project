using System;
using Valyreon.Elib.DataLayer;
using Valyreon.Elib.Wpf.Models;

namespace Valyreon.Elib.Wpf.ViewModels
{
    public interface IViewer : IDisposable
    {
        public abstract string Caption { get; set; }

        public abstract FilterParameters Filter { get; }

        public abstract void Refresh();

        public abstract Action Back { get; set; }

        public abstract void Clear();

        public abstract Func<IViewer> GetCloneFunction(Selector selector);
    }
}
