using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.ViewModels
{
    public interface IViewer : ICloneable
    {
        public abstract string Caption { get; set; }
        public new abstract object Clone();
        public abstract string NumberOfBooks { get; set; }
    }
}
