using System;
using Domain;

namespace ElibWpf.ViewModels
{
    public interface IViewer : ICloneable
    {
        public abstract string Caption { get; set; }

        public abstract Func<Book, bool> DefaultCondition { get; }

        public abstract string NumberOfBooks { get; set; }

        public new abstract object Clone();
    }
}