using Domain;
using System;

namespace ElibWpf.ViewModels
{
    public interface IViewer : ICloneable
    {
        public abstract string Caption { get; set; }

        public abstract Func<Book, bool> DefaultCondition { get; }

        public new abstract object Clone();

        public abstract string NumberOfBooks { get; set; }
    }
}