using System;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer;

namespace Valyreon.Elib.Wpf.ViewModels
{
    public interface IViewer
    {
        public abstract string Caption { get; set; }

        public abstract FilterParameters Filter { get; }

        public abstract void Refresh();

        public abstract Task<IViewer> Search(SearchParameters searchOptions);

        public abstract Action Back { get; set; }

        public abstract void Clear();
    }
}
