using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    [Table("Authors")]
    public class Author
    {
        public ICollection<Book> Books { get; set; }
        public int Id { get; set; }

        [Required] [StringLength(100)] public string Name { get; set; }
    }
}