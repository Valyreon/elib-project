using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valyreon.Elib.Domain
{
    [Table("Series")]
    public class BookSeries : ObservableEntity
    {
        private string name;
        private int numberOfBooks;

        public IEnumerable<Book> Books { get; set; }

        [Column]
        public string Name
        {
            get => name;
            set => Set(() => Name, ref name, value);
        }

        [NotMapped]
        public int NumberOfBooks
        {
            get => numberOfBooks;
            set => Set(() => NumberOfBooks, ref numberOfBooks, value);
        }

        public string NumberOfBooksString => NumberOfBooks == 1 ? "1 book" : NumberOfBooks + " books";
    }
}
