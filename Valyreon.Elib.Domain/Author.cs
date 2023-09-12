using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valyreon.Elib.Domain
{
    [Table("Authors")]
    public class Author : ObservableEntity
    {
        private string name;
        private int numberOfBooks;

        public IEnumerable<Book> Books { get; set; }

        [Required]
        [Column]
        [StringLength(100)]
        public string Name
        {
            get => name;
            set => Set(() => Name, ref name, value);
        }

        [NotMapped]
        public int NumberOfBooks
        {
            get => numberOfBooks;
            set
            {
                Set(() => NumberOfBooks, ref numberOfBooks, value);
                RaisePropertyChanged(() => NumberOfBooksString);
            }
        }

        public string NumberOfBooksString
        {
            get
            {
                if (NumberOfBooks > 0)
                {
                    return NumberOfBooks == 1 ? "1 book" : NumberOfBooks + " books";
                }

                return string.Empty;
            }
        }
    }
}
