using System;
using Valyreon.Elib.DataLayer.Filters;

namespace Valyreon.Elib.Wpf.ViewModels
{
    public interface IViewer : IDisposable
    {
        string Caption { get; set; }

        void Refresh();

        Action Back { get; set; }

        void Clear();

        Func<IViewer> GetCloneFunction();

        IFilterParameters GetFilter();
    }
}
