using System;

namespace ElibWpf.ViewModels
{
    public interface IViewer : ICloneable
    {
        public abstract string Caption { get; set; }

        public new abstract object Clone();

        public abstract string NumberOfBooks { get; set; }
    }
}