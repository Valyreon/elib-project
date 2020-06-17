using MVVMLibrary;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Series")]
    public class BookSeries : ObservableObject
    {
        private string name;

        public IEnumerable<Book> Books { get; set; }
        public int Id { get; set; }

        public string Name
        {
            get => this.name;
            set => Set(() => Name, ref name, value);
        }
    }
}