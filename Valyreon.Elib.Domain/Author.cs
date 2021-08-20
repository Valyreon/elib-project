using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Valyreon.Elib.Domain
{
    [Table("Authors")]
    public class Author : ObservableEntity
    {
        private string name;

        public IEnumerable<Book> Books { get; set; }

        [Required]
        [StringLength(100)]
        public string Name
        {
            get => name;
            set => Set(() => Name, ref name, value);
        }
    }
}
