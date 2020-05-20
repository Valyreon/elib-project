using System;
using Domain;
using ElibWpf.Messages;
using Models.Options;

namespace ElibWpf.ViewModels
{
    public interface IViewer
    {
        public abstract string Caption { get; set; }

        public abstract Filter Filter { get; }

        public abstract string NumberOfBooks { get; set; }

        public abstract void Refresh();

        public abstract void Clear();

        public abstract void Search(SearchOptions searchOptions);
    }
}