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
        public Book Book { get; private set; }

        public BookDetailsViewModel(Book book)
        {
            this.Book = book;
        }
    }
}
