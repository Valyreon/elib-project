using System;
using System.Threading.Tasks;
using DataLayer;

namespace ElibWpf.ViewModels
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
