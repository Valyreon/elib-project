using Domain;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ElibWpf.ViewModels.Flyouts
{
    public class BookDetailsViewModel : ViewModelBase
    {
        private readonly Book book;

        public BookDetailsViewModel(Book book)
        {
            this.book = book;
        }
    }
}
