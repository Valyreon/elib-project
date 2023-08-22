using System;
using Valyreon.Elib.DataLayer;

namespace Valyreon.Elib.Wpf.ViewModels
{
    public interface IViewer
    {
        public abstract string Caption { get; set; }

        public abstract FilterParameters Filter { get; }

        public abstract void Refresh();

        public abstract Action Back { get; set; }

        public abstract void Clear();
    }
}
