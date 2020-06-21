using DataLayer;
using Models.Options;

namespace ElibWpf.ViewModels
{
    public interface IViewer
    {
        public abstract string Caption { get; set; }

        public abstract FilterParameters Filter { get; }

        public abstract string NumberOfBooks { get; set; }

        public abstract void Refresh();

        public abstract void Clear();

        public abstract void Search(SearchParameters searchOptions);

    }
}