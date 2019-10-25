using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.ViewModels
{
    public class BooksViewModel : ViewModelBase, IPageViewModel
    {
        private string _caption = "Books";

        public string Caption
        {
            get => _caption;
            set => Set(ref _caption, value);
        }
    }
}
