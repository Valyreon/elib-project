using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Series")]
    public class BookSeries : ObservableEntity
    {
        private string name;

        public IEnumerable<Book> Books { get; set; }

        public string Name
        {
            get => name;
            set => Set(() => Name, ref name, value);
        }
    }
}
