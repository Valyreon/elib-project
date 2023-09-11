using System;
using Valyreon.Elib.DataLayer.Filters;

namespace Valyreon.Elib.Wpf.ViewModels
{
    public interface IViewer : IDisposable
    {
        Action Back { get; set; }
        string Caption { get; set; }

        void Clear();

        Func<IViewer> GetCloneFunction();

        IFilterParameters GetFilter();

        void Refresh();
    }
}
